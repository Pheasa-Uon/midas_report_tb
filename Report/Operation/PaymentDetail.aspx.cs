using Microsoft.Reporting.WebForms;
using Report.Utils;
using System;
using System.Data;

namespace Report.Operation
{
    public partial class PaymentDetailV2 : System.Web.UI.Page
    {
        private DBConnect db = new DBConnect();
        public static string fromDate, systemDateStr;
        public string format = "dd/MM/yyyy";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DataHelper.checkLoginSession();
                systemDateStr = DataHelper.getSystemDate().ToString("dd/MM/yyyy");
                DataHelper.populateBranchDDL(ddBranchName, DataHelper.getUserId());
                populateOfficer();
                dtpFromDate.Text = systemDateStr;
                dtpToDate.Text = systemDateStr;
            }
        }

        private void GenerateReport(DataTable dt)
        {
            ReportParameterCollection reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("Branch", ddBranchName.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("PawnOfficer", ddOfficer.SelectedItem.Text));     
            reportParameters.Add(new ReportParameter("FromDate", DateTime.ParseExact(dtpFromDate.Text, format, null).ToString("dd-MMM-yyyy")));
            reportParameters.Add(new ReportParameter("ToDate", DateTime.ParseExact(dtpToDate.Text, format, null).ToString("dd-MMM-yyyy")));
            //reportParameters.Add(new ReportParameter("Currency", ddCurrency.SelectedItem.Text));

            var ds = new ReportDataSource("PaymentDetailDS", dt);
            DataHelper.generateOperationReport(ReportViewer1, "PaymentDetail", reportParameters, ds);
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            var fromDate = DateTime.ParseExact(dtpFromDate.Text, format, null).ToString("yyyy-MM-dd");
            var toDate = DateTime.ParseExact(dtpToDate.Text, format, null).ToString("yyyy-MM-dd");

            var sql = "";
            if (ddBranchName.SelectedItem.Value == "ALL")
            {
                sql = "SELECT PM.id,PM.payment_type,PM.payment_date,C.customer_name,ST.ticket_no,P.lob_name, " +
                " CASE WHEN PT.payment_flag = 3 AND CON.redeem_type = 1 THEN " +
                " CASE WHEN(PM.principle_pay - IFNULL((SELECT pawn_price_approved FROM contract WHERE parent_id_id = CON.id AND contract_status >= 4 AND contract_status != 5 LIMIT 1), 0)) > 0 THEN " +
                " PM.principle_pay - IFNULL((SELECT pawn_price_approved FROM contract WHERE parent_id_id = CON.id AND contract_status >= 4 AND contract_status != 5 LIMIT 1), 0) ELSE 0 END " +
                " ELSE PM.principle_pay END AS principle_pay, " +
                " PM.interest_pay,PM.early_redeem_pay,PM.penalty_pay,ST.principle_due principle_less, " +
                " CASE WHEN CON.product_type_id = 1 THEN STT.due_date ELSE ST.due_date END due_date,ST.serial_number,SI.`name` pawn_officer " +
                " ,IFNULL(WV.waive_amount, 0) AS waive_amount, ST.ticket_type, IFNULL(PO.other_income_amount, 0) AS other_income_amount, " +
                " CASE WHEN CON.contract_status = 6 THEN 'Redeem' " +
                " WHEN CON.contract_status = 7 THEN 'In-Forfeit' " +
                " WHEN CON.contract_status = 4 THEN 'Active' END AS contract_status, " +
                " PT.or_ref, 0 AS fee_collect, PT.payment_method " +
                " FROM payment PM " +
                " LEFT JOIN payment_total PT ON PM.payment_total_id = PT.id " +
                " LEFT JOIN schedule_ticket ST ON PM.schedule_ticket_id = ST.id " +
                " LEFT JOIN contract CON ON PM.contract_id = CON.id " +
                " LEFT JOIN product P ON CON.product_id = P.id " +
                " LEFT JOIN customer C ON CON.customer_id = C.id " +
                " LEFT JOIN schedule_ticket STT ON ST.id + 1 = STT.id " +
                " LEFT JOIN staff_info SI ON CON.pawn_officer_id = SI.id " +
                " LEFT JOIN(SELECT system_date_id, contract_id, schedule_ticket_id, SUM(waive_amount) waive_amount FROM waive WHERE waive_status = 2 AND trxn_type >= 2 GROUP BY system_date_id, contract_id, " +
                " schedule_ticket_id) WV ON PM.contract_id = WV.contract_id AND PM.system_date_id = WV.system_date_id AND PM.schedule_ticket_id = WV.schedule_ticket_id " +
                " LEFT JOIN(SELECT payment_total_id, SUM(other_income_amount) AS other_income_amount FROM payment_other_income GROUP BY payment_total_id) AS PO ON PM.payment_total_id = PO.payment_total_id " +
                " WHERE PM.payment_date BETWEEN DATE('" + fromDate + "') AND DATE('" + toDate + "') AND PM.payment_status = 1 AND(PM.total_pay + IFNULL(PO.other_income_amount, 0)) > 0 ";
            }
            else
            {
                sql = "SELECT PM.id,PM.payment_type,PM.payment_date,C.customer_name,ST.ticket_no,P.lob_name, " +
                " CASE WHEN PT.payment_flag = 3 AND CON.redeem_type = 1 THEN " +
                " CASE WHEN(PM.principle_pay - IFNULL((SELECT pawn_price_approved FROM contract WHERE parent_id_id = CON.id AND contract_status >= 4 AND contract_status != 5 LIMIT 1), 0)) > 0 THEN " +
                " PM.principle_pay - IFNULL((SELECT pawn_price_approved FROM contract WHERE parent_id_id = CON.id AND contract_status >= 4 AND contract_status != 5 LIMIT 1), 0) ELSE 0 END " +
                " ELSE PM.principle_pay END AS principle_pay, " +
                " PM.interest_pay,PM.early_redeem_pay,PM.penalty_pay,ST.principle_due principle_less, " +
                " CASE WHEN CON.product_type_id = 1 THEN STT.due_date ELSE ST.due_date END due_date,ST.serial_number,SI.`name` pawn_officer " +
                " ,IFNULL(WV.waive_amount, 0) AS waive_amount, ST.ticket_type, IFNULL(PO.other_income_amount, 0) AS other_income_amount, " +
                " CASE WHEN CON.contract_status = 6 THEN 'Redeem' " +
                " WHEN CON.contract_status = 7 THEN 'In-Forfeit' " +
                " WHEN CON.contract_status = 4 THEN 'Active' END AS contract_status, " +
                " PT.or_ref, 0 AS fee_collect, PT.payment_method " +
                " FROM payment PM " +
                " LEFT JOIN payment_total PT ON PM.payment_total_id = PT.id " +
                " LEFT JOIN schedule_ticket ST ON PM.schedule_ticket_id = ST.id " +
                " LEFT JOIN contract CON ON PM.contract_id = CON.id " +
                " LEFT JOIN product P ON CON.product_id = P.id " +
                " LEFT JOIN customer C ON CON.customer_id = C.id " +
                " LEFT JOIN schedule_ticket STT ON ST.id + 1 = STT.id " +
                " LEFT JOIN staff_info SI ON CON.pawn_officer_id = SI.id " +
                " LEFT JOIN(SELECT system_date_id, contract_id, schedule_ticket_id, SUM(waive_amount) waive_amount FROM waive WHERE waive_status = 2 AND trxn_type >= 2 GROUP BY system_date_id, contract_id, " +
                " schedule_ticket_id) WV ON PM.contract_id = WV.contract_id AND PM.system_date_id = WV.system_date_id AND PM.schedule_ticket_id = WV.schedule_ticket_id " +
                " LEFT JOIN(SELECT payment_total_id, SUM(other_income_amount) AS other_income_amount FROM payment_other_income GROUP BY payment_total_id) AS PO ON PM.payment_total_id = PO.payment_total_id " +
                " WHERE CON.branch_id = " + ddBranchName.SelectedItem.Value +
                " AND PM.payment_date BETWEEN DATE('" + fromDate + "') AND DATE('" + toDate + "') AND PM.payment_status = 1 AND(PM.total_pay + IFNULL(PO.other_income_amount, 0)) > 0 ";
            }
            if (ddOfficer.SelectedItem.Value != "0")
            {
                sql += " AND CON.pawn_officer_id = " + ddOfficer.SelectedItem.Value;
            }
            if (ddBranchName.SelectedItem.Value == "ALL")
            {
                sql += " UNION " +
                      " SELECT A.* FROM " +
                      " ( " +
                      " SELECT CON.id, 'DB' AS 'payment_type', CON.disbursement_date AS 'payment_date', C.customer_name, CON.contract_no, P.lob_name, " +
                      " 0 AS 'principle', 0 AS 'interest', 0 AS 'early_redeem', 0 AS 'penalty', CON.pawn_price_approved, " +
                      " CON.disbursement_date, '', SI.name, 0 AS 'other_income', CON.ticket_type, 0 AS 'waive', " +
                      " CASE WHEN CON.contract_status = 6 THEN 'Redeem' " +
                      " WHEN CON.contract_status = 7 THEN 'In-Forfeit' " +
                      " WHEN CON.contract_status = 4 THEN 'Active' END AS contract_status, " +
                      " CON.or_ref, (SELECT SUM(fee_collect) FROM contract_fee WHERE b_status = 1 AND contract_id = CON.id GROUP BY contract_id) AS fee_collect, 1 " +
                      " FROM contract CON " +
                      " LEFT JOIN product P ON CON.product_id = P.id " +
                      " LEFT JOIN customer C ON CON.customer_id = C.id " +
                      " LEFT JOIN staff_info SI ON CON.pawn_officer_id = SI.id " +
                      " WHERE CON.disbursement_date BETWEEN DATE('" + fromDate + "') AND DATE('" + toDate + "') AND CON.b_status = 1 ";
            }
            else
            {
                sql += " UNION " +
                         " SELECT A.* FROM " +
                         " ( " +
                         " SELECT CON.id, 'DB' AS 'payment_type', CON.disbursement_date AS 'payment_date', C.customer_name, CON.contract_no, P.lob_name, " +
                         " 0 AS 'principle', 0 AS 'interest', 0 AS 'early_redeem', 0 AS 'penalty', CON.pawn_price_approved, " +
                         " CON.disbursement_date, '', SI.name, 0 AS 'other_income', CON.ticket_type, 0 AS 'waive', " +
                         " CASE WHEN CON.contract_status = 6 THEN 'Redeem' " +
                         " WHEN CON.contract_status = 7 THEN 'In-Forfeit' " +
                         " WHEN CON.contract_status = 4 THEN 'Active' END AS contract_status, " +
                         " CON.or_ref, (SELECT SUM(fee_collect) FROM contract_fee WHERE b_status = 1 AND contract_id = CON.id GROUP BY contract_id) AS fee_collect, 1 " +
                         " FROM contract CON " +
                         " LEFT JOIN product P ON CON.product_id = P.id " +
                         " LEFT JOIN customer C ON CON.customer_id = C.id " +
                         " LEFT JOIN staff_info SI ON CON.pawn_officer_id = SI.id " +
                         " WHERE CON.branch_id = " + ddBranchName.SelectedItem.Value +
                         " AND CON.disbursement_date BETWEEN DATE('" + fromDate + "') AND DATE('" + toDate + "') AND CON.b_status = 1 ";
            }
            if (ddOfficer.SelectedItem.Value != "0")
            {
                sql += " AND CON.pawn_officer_id = " + ddOfficer.SelectedItem.Value;
            }
            sql += " ) A " +
             " WHERE A.fee_collect > 0 " +
             " ORDER BY or_ref, payment_date ";
            DataTable dt = db.getDataTable(sql);
            GenerateReport(dt);
        }

        protected void ddBranchName_SelectedIndexChanged(object sender, EventArgs e)
        {
            populateOfficer();
        }
        private void populateOfficer()
        {
            if (ddBranchName.SelectedItem.Value != "")
            {
                if (ddBranchName.SelectedItem.Value == "ALL")
                {
                    ddOfficer.Enabled = true;
                    DataHelper.populateOfficerDDLAll(ddOfficer);
                }
                else
                {
                    ddOfficer.Enabled = true;
                    DataHelper.populateOfficerDDL(ddOfficer, Convert.ToInt32(ddBranchName.SelectedItem.Value));
                }

            }
            else
            {
                ddOfficer.Enabled = false;
                ddOfficer.Items.Clear();
            }
        }

    }
}