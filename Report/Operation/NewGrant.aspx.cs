using Microsoft.Reporting.WebForms;
using Report.Models;
using System;
using System.Collections.Generic;
using System.Data;
using Report.Utils;

namespace Report.Operation
{
    public partial class NewGrant : System.Web.UI.Page
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
                systemDateStr = DataHelper.getSystemDate().ToString("dd/MM/yyyy");
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
                dtpSystemDate.Text = systemDateStr;
            }
        }

        private void GenerateReport(DataTable dt)
        {
            ReportParameterCollection reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("Branch", ddBranchName.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("PawnOfficer", ddOfficer.SelectedItem.Text));
            if (dtpFromDate.Text != "")
            {
                reportParameters.Add(new ReportParameter("FromDate", DateTime.ParseExact(dtpFromDate.Text, format, null).ToString("dd-MMM-yyyy")));
            }
            else
            {
                reportParameters.Add(new ReportParameter("FromDate", " "));
            }
            reportParameters.Add(new ReportParameter("ToDate", DateTime.ParseExact(dtpSystemDate.Text, format, null).ToString("dd-MMM-yyyy")));
            reportParameters.Add(new ReportParameter("Currency", ddCurrency.SelectedItem.Text));

            var ds = new ReportDataSource("NewGrantDS", dt);

            DataHelper.generateOperationReport(ReportViewer1, "NewGrant", reportParameters, ds);
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

            var sql = "SELECT C.id, CUS.customer_no,C.disbursement_date,CUS.customer_name,STINFO.ticket_no,STINFO.due_date,C.pawn_price_approved,  " +
                            "    CUR.currency, CUR.currency_code,SI.name pawn_officer,PD.lob_name,0 principle_amt,0 int_amt,  " +
                            "    ROUND(IFNULL(CF.fee, 0), 2) AS other_income_amt, " +
                            "	 STINFO.serial_number,C.contract_type, C.come_through, C.dv_ref , " +
                            "    ContractGetBalance(c.contract_no,now()) as outstanding, c.interest_rate, vd.vendor_name, " +
                            "    c.num_of_install,c.market_price,OC.occupation,cus.address,CLT.coll_type_name, " +
                            "    CLC.coll_cate_code, MKK.mark_name, CL.seal_no , CL.year_of_model,  " +
                            "    CL.vh_plate_no, CL.vh_cylinder, CL.vh_frame, CL.vh_engine_no, CRT.cert_name,  " +
                            "    CL.pr_issue_date, CL.pr_square_side, CL.pr_north_by, CL.pr_south_by, CL.pr_west_by, CL.pr_east_by,  " +
                            "    CL.jr_weight, CL.jr_quality , CL.remark,CL.el_model,CL.vh_mark,CL.vh_type,CL.vh_color,br.branch_code,  " +
                            "    case when CL.is_ownership = 1 then 'Yes' else 'No' end is_ownership  " +
                        "     FROM contract C  " +
                        "     LEFT JOIN(SELECT contract_id, SUM(fee_amount) AS fee FROM contract_fee WHERE b_status= 1 GROUP BY contract_id) CF ON C.id = CF.contract_id  " +
                        "     LEFT JOIN customer CUS ON C.customer_id = CUS.id  " +
                        "     LEFT JOIN currency CUR ON C.currency_id = CUR.id  " +
                        "     LEFT JOIN staff_info SI ON C.pawn_officer_id = SI.id   " +
                        "     LEFT JOIN acc_vendor VD ON VD.id = c.vendor_id  " +
                        "     LEFT JOIN product PD ON PD.id = C.product_id   " +
                        "     LEFT JOIN occupation OC ON OC.id = CUS.occupation_id   " +
                        "     LEFT JOIN schedule_ticket STINFO ON STINFO.contract_id = C.id AND STINFO.order_no = 1  " +
                        "     LEFT JOIN(SELECT* FROM collateral WHERE b_status=1 GROUP BY contract_id) CL ON C.id = CL.contract_id  " +
                        "     LEFT JOIN collateral_category CLC ON CL.collateral_category_id = CLC.id  " +
                        "     LEFT JOIN collateral_type CLT ON CL.collateral_type_id = CLT.id  " +
                        "     LEFT JOIN mark MKK ON CL.mark_id = MKK.id  " +
                        "     LEFT JOIN branch br ON br.id = c.branch_id  " +
                        "     LEFT JOIN certificate_type CRT ON CL.pr_certificate_type_id = CRT.id   " +
                        "     WHERE C.contract_status >= 4 AND C.contract_status != 5 AND C.b_status= 1 AND C.contract_type = 'New'   " +
                        "     AND C.branch_id =  " + ddBranchName.SelectedItem.Value +
                        "     AND C.currency_id =  " + ddCurrency.SelectedItem.Value;

            if (fromDay != "")
            {
                sql += " AND C.disbursement_date BETWEEN DATE('" + fromDay + "') AND DATE('" + systemDate + "') ";
            }
            else
            {
                sql += " AND MONTH(C.disbursement_date) = "+ month +" AND YEAR(C.disbursement_date) = " + year;
            }

            if (ddOfficer.SelectedItem.Value != "0")
            {
                sql += " AND C.pawn_officer_id = " + ddOfficer.SelectedItem.Value;
            }
            sql += " UNION " +
                   "SELECT C.id,P.payment_date,CUS.customer_name,ST.ticket_no,ST.due_date,0,  " +
                        "        CUR.currency,CUR.currency_code,SI.`name` pawn_officer,PD.lob_name,P.principle_pay,P.interest_pay,  " +
                        "        ROUND(IFNULL(OI.total_other_income, 0), 2) AS other_income_amt,  " +
                        "        ST.serial_number,C.contract_type, C.`come_through`, C.dv_ref,  " +
                        "        CUS.customer_no, ContractGetBalance(c.contract_no,now()), c.interest_rate, vd.vendor_name, " +
                        "        c.num_of_install,c.market_price,OC.occupation,cus.address,CLT.coll_type_name,  " +
                        "        CLC.coll_cate_code, MKK.mark_name, CL.seal_no , CL.year_of_model,  " +
                        "        CL.vh_plate_no, CL.vh_cylinder, CL.vh_frame, CL.vh_engine_no, CRT.cert_name,  " +
                        "        CL.pr_issue_date, CL.pr_square_side, CL.pr_north_by, CL.pr_south_by, CL.pr_west_by, CL.pr_east_by,  " +
                        "        CL.jr_weight, CL.jr_quality , CL.remark,CL.el_model,CL.vh_mark,CL.vh_type,CL.vh_color,br.branch_code, " +
                        "		 case when CL.is_ownership = 1 then 'Yes' else 'No' end is_ownership  " +
                    "       FROM payment P   " +
                    "       INNER JOIN payment_total PTT ON P.payment_total_id = PTT.id  " +
                    "       INNER JOIN contract C ON PTT.`contract_id`= C.id    " +
                    "       LEFT JOIN schedule_ticket ST ON ST.id = PTT.schedule_ticket_id    " +
                    "       LEFT JOIN customer CUS ON C.customer_id = CUS.id   " +
                    "       LEFT JOIN currency CUR ON C.currency_id = CUR.id   " +
                    "       LEFT JOIN staff_info SI ON C.pawn_officer_id = SI.id   " +
                    "       LEFT JOIN product PD ON PD.id = C.product_id   " +
                    "       LEFT JOIN acc_vendor VD ON VD.id = c.vendor_id  " +
                    "       LEFT JOIN branch br ON br.id = c.branch_id  " +
                    "       LEFT JOIN occupation OC ON OC.id = CUS.occupation_id   " +
                    "       LEFT JOIN(SELECT* FROM collateral WHERE b_status=1 GROUP BY contract_id) CL ON C.id = CL.contract_id   " +
                    "  	    LEFT JOIN collateral_category CLC ON CL.collateral_category_id = CLC.id   " +
                    "       LEFT JOIN collateral_type CLT ON CL.collateral_type_id = CLT.id   " +
                    "       LEFT JOIN mark MKK ON CL.mark_id = MKK.id  " +
                    "       LEFT JOIN certificate_type CRT ON CL.pr_certificate_type_id = CRT.id  " +
                    "       LEFT JOIN    " +
                    "       (  " +
                    "           SELECT payment_total_id, SUM(other_income_amount) AS total_other_income  " +
                    "           FROM payment_other_income   " +
                    "           GROUP BY payment_total_id   " +
                    "       ) OI ON OI.payment_total_id = PTT.id  " +
                    "       WHERE PTT.system_date_id = C.disburse_sys_date_id  AND C.contract_type = 'New' AND C.`b_status`= 1  " +
                    "       AND PTT.payment_total_status = 1 AND PTT.payment_flag != 3  " +
                    "       AND C.branch_id =  " + ddBranchName.SelectedItem.Value +
                    "       AND C.currency_id =  " + ddCurrency.SelectedItem.Value;

            if (fromDay != "")
            {
                sql += " AND P.payment_date BETWEEN DATE('" + fromDay + "') AND DATE('" + systemDate + "') ";
            }
            else
            {
                sql += " AND MONTH(P.payment_date) = " + month + " AND YEAR(P.payment_date) = " + year + " ";
            }

            if (ddOfficer.SelectedItem.Value != "0")
            {
                sql += " AND C.pawn_officer_id = " + ddOfficer.SelectedItem.Value;
            }
            sql += " ORDER BY disbursement_date;";
            
            DataTable dt = db.getDataTable(sql);
            GenerateReport(dt);
        }

        protected void ddBranchName_SelectedIndexChanged(object sender, EventArgs e)
        {
            populateOfficer();
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
        private void populateOfficer()
        {
            if (ddBranchName.SelectedValue != "")
            {
                ddOfficer.Enabled = true;
                DataHelper.populateOfficerDDL(ddOfficer, Convert.ToInt32(ddBranchName.SelectedValue));
            }
            else
            {
                ddOfficer.Enabled = false;
                ddOfficer.Items.Clear();
            }
        }
    }
}