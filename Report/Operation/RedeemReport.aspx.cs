using Microsoft.Reporting.WebForms;
using Report.Models;
using System;
using System.Collections.Generic;
using System.Data;
using Report.Utils;

namespace Report.Operation
{
    public partial class Redeem : System.Web.UI.Page
    {
        private DBConnect db = new DBConnect();
        static List<Currency> currencyList;
        public static string fromDate, systemDateStr;
        public string format = "dd/MM/yyyy";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DataHelper.checkLoginSession();
                var sysDate = DataHelper.getSystemDate();
                systemDateStr = sysDate.ToString(format);
                DataHelper.populateBranchDDL(ddBranchName, DataHelper.getUserId());
                populateOfficer();
                chkFromDate.Checked = true;
                dtpFromDate.Enabled = false;
                dtpFromDate.Text = "";
                if (dtpFromDate.Text != "")
                {
                    fromDate = dtpFromDate.Text;
                }
                else
                {
                    fromDate = null;
                }
                
                currencyList = DataHelper.populateCurrencyDDL(ddCurrency);
                dtpSystemDate.Text = sysDate.ToString(format);
            }
        }
        
        private void GenerateReport(DataTable redeemDT)
        {
            var reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("Branch", ddBranchName.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("Currency", ddCurrency.SelectedItem.Text));
            if (dtpFromDate.Text != "")
            {
                reportParameters.Add(new ReportParameter("FromDate", DateTime.ParseExact(dtpFromDate.Text, format, null).ToString("dd-MMM-yyyy")));
            }
            else
            {
                reportParameters.Add(new ReportParameter("FromDate", " "));
            }
            reportParameters.Add(new ReportParameter("SystemDate", DateTime.ParseExact(dtpSystemDate.Text, format, null).ToString("dd-MMM-yyyy")));
            reportParameters.Add(new ReportParameter("PawnOfficer", ddOfficer.SelectedItem.Text));

            var _redeem = new ReportDataSource("RedeemDS", redeemDT);
            DataHelper.generateOperationReport(ReportViewer1, "RedeemReport", reportParameters, _redeem);
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            var fromDay = "";
            var fromDayDate = "";
            if (dtpFromDate.Text != "")
            {
                var fromDateSql = DateTime.ParseExact(dtpFromDate.Text, format, null);
                fromDay = fromDateSql.ToString("yyyy-MM-dd");
                fromDayDate = fromDateSql.ToString("dd");
            }

            var systemDateSql = DateTime.ParseExact(dtpSystemDate.Text, format, null);
            var systemDate = systemDateSql.ToString("yyyy-MM-dd");
            var month = systemDateSql.ToString("MM");
            var year = systemDateSql.ToString("yyyy");

            var sql = "";
            if (ddBranchName.SelectedItem.Value == "ALL")
            {
                sql = "SELECT c.id, c.redeem_date, c.product_id, cus.customer_name, pro.lob_name, st.ticket_no, " +
                " p.principle_pay AS principle, p.interest_pay + early_redeem_pay AS interest, " +
                " IFNULL(OI.total_other_income, 0) AS redeem_other_income, " +
                " penalty_pay AS total_penalty_paid,cur.currency, staff.`name` pawn_officer, " +
                " st.serial_number, IFNULL(WV.waive_amount, 0) AS waive_amount " +
                " FROM payment_total ptt " +
                " INNER JOIN system_date s ON ptt.system_date_id = s.id " +
                " INNER JOIN payment p ON ptt.id = p.payment_total_id " +
                " LEFT JOIN schedule_ticket st ON st.id = p.schedule_ticket_id " +
                " INNER JOIN contract c ON ptt.contract_id = c.id " +
                " INNER JOIN customer cus ON cus.id = c.customer_id " +
                " INNER JOIN product pro ON pro.id = c.product_id " +
                " INNER JOIN currency cur ON cur.id = c.currency_id " +
                " INNER JOIN staff_info staff ON staff.id = c.pawn_officer_id " +
                " LEFT JOIN " +
                " ( " +
                "     SELECT payment_total_id, SUM(other_income_amount) AS total_other_income " +
                "     FROM payment_other_income " +
                "     GROUP BY payment_total_id " +
                " ) OI ON OI.payment_total_id = ptt.id " +
                " LEFT JOIN " +
                " ( " +
                "     SELECT system_date_id, contract_id, SUM(waive_amount) waive_amount " +
                "     FROM waive " +
                "     WHERE waive_status = 2 AND trxn_type <= 2 GROUP BY system_date_id,contract_id " +
                " ) WV ON c.id = WV.contract_id AND ptt.system_date_id = WV.system_date_id " +
                " WHERE ptt.payment_flag = 3 AND c.redeem_type = 2 AND ptt.payment_total_status = TRUE " +
                " AND c.contract_status = 6 AND c.b_status = 1 " +
                " AND c.currency_id = " + ddCurrency.SelectedItem.Value;
            }
            else
            {
                sql = "SELECT c.id, c.redeem_date, c.product_id, cus.customer_name, pro.lob_name, st.ticket_no, " +
                " p.principle_pay AS principle, p.interest_pay + early_redeem_pay AS interest, " +
                " IFNULL(OI.total_other_income, 0) AS redeem_other_income, " +
                " penalty_pay AS total_penalty_paid,cur.currency, staff.`name` pawn_officer, " +
                " st.serial_number, IFNULL(WV.waive_amount, 0) AS waive_amount " +
                " FROM payment_total ptt " +
                " INNER JOIN system_date s ON ptt.system_date_id = s.id " +
                " INNER JOIN payment p ON ptt.id = p.payment_total_id " +
                " LEFT JOIN schedule_ticket st ON st.id = p.schedule_ticket_id " +
                " INNER JOIN contract c ON ptt.contract_id = c.id " +
                " INNER JOIN customer cus ON cus.id = c.customer_id " +
                " INNER JOIN product pro ON pro.id = c.product_id " +
                " INNER JOIN currency cur ON cur.id = c.currency_id " +
                " INNER JOIN staff_info staff ON staff.id = c.pawn_officer_id " +
                " LEFT JOIN " +
                " ( " +
                "     SELECT payment_total_id, SUM(other_income_amount) AS total_other_income " +
                "     FROM payment_other_income " +
                "     GROUP BY payment_total_id " +
                " ) OI ON OI.payment_total_id = ptt.id " +
                " LEFT JOIN " +
                " ( " +
                "     SELECT system_date_id, contract_id, SUM(waive_amount) waive_amount " +
                "     FROM waive " +
                "     WHERE waive_status = 2 AND trxn_type <= 2 GROUP BY system_date_id,contract_id " +
                " ) WV ON c.id = WV.contract_id AND ptt.system_date_id = WV.system_date_id " +
                " WHERE ptt.payment_flag = 3 AND c.redeem_type = 2 AND ptt.payment_total_status = TRUE " +
                " AND c.contract_status = 6 AND c.b_status = 1 " +
                " AND c.branch_id = " + ddBranchName.SelectedItem.Value +
                " AND c.currency_id = " + ddCurrency.SelectedItem.Value;
            }
            if (fromDay != "")
            {
                sql += " AND C.redeem_date BETWEEN DATE('" + fromDay + "') AND DATE('" + systemDate + "') ";
            }
            else
            {
                sql += " AND MONTH(C.redeem_date) = " + month + " AND YEAR(C.disbursement_date) = " + year;
            }
            if (ddOfficer.SelectedItem.Value != "0")
            {
                sql += " AND c.pawn_officer_id = " + ddOfficer.SelectedItem.Value;
            }
            sql += " ORDER BY c.redeem_date;";
            DataTable redeemDT = db.getDataTable(sql);
            GenerateReport(redeemDT);
        }

        protected void chkFromDate_CheckedChanged(object sender, EventArgs e)
        {
            if (chkFromDate.Checked)
            {
                dtpFromDate.Text = null;
                dtpFromDate.Enabled = false;
            }
            else
            {
                dtpFromDate.Text = systemDateStr;
                dtpFromDate.Enabled = true;
            }
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