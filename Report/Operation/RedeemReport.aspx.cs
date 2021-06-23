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
        DateTime currentDate = DateTime.Today;
        static List<Currency> currencyList;
        public static string fromDate, systemDateStr;
        public string format = "dd/MM/yyyy";
        protected void Page_Load(object sender, EventArgs e)
        {
            DataHelper.checkLoginSession();
            
            //Convert Date Block
            systemDateStr = DataHelper.getSystemDateStr();

            //Adding Text and Value to Branch DropdownList block
            if (!IsPostBack)
            {
                DataHelper.populateBranchDDL(ddBranchName, DataHelper.getUserId());
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
                var sysDate = DataHelper.getSystemDate();
                currencyList = DataHelper.populateCurrencyDDL(ddCurrency);
                dtpSystemDate.Text = sysDate.ToString(format);
            }
        }

        //GenerateReport Function
        private void GenerateReport(DataTable redeemDT)
        {
            var reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("Branch", ddBranchName.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("Currency", ddCurrency.SelectedItem.Text));
            if (dtpFromDate.Text != "")
            {
                reportParameters.Add(new ReportParameter("FromDater", DateTime.ParseExact(dtpFromDate.Text, format, null).ToString("dd-MMM-yyyy")));
            }
            else
            {
                reportParameters.Add(new ReportParameter("FromDate", " "));
            }
            reportParameters.Add(new ReportParameter("SystemDate", DateTime.ParseExact(dtpSystemDate.Text, format, null).ToString("dd-MMM-yyyy")));
            //reportParameters.Add(new ReportParameter("CurrencyLabelParameter", currencyList.Find(x => x.id == Convert.ToInt32(ddCurrency.SelectedItem.Value)).currency_label));
          
            var _redeem = new ReportDataSource("RedeemDS", redeemDT);
            DataHelper.generateOperationReport(ReportViewer1, "RedeemReport", reportParameters, _redeem);
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            //Split System Date Time variable
            var fromDay = new Object();
            var fromDayDate = new Object();
            if (dtpFromDate.Text != "")
            {
                var fromDateSql = DateTime.ParseExact(dtpFromDate.Text, format, null);
                fromDay = fromDateSql.ToString("yyyy-MM-dd");
                fromDayDate =  fromDateSql.ToString("dd") ;
            }
            else
            {
                fromDay = "null";
                fromDayDate = "null";
            }
           
            var systemDateSql = DateTime.ParseExact(dtpSystemDate.Text, format, null);
            var systemDate = systemDateSql.ToString("yyyy-MM-dd");
            var day = systemDateSql.ToString("dd");
            var month = systemDateSql.ToString("MM");
            var year = systemDateSql.ToString("yyyy");

            var redeemSql = "SELECT C.id,C.redeem_date,CUS.customer_name,PD.lob_name,ST.ticket_no,PM.principle_pay + PM.prepaid_principle_pay AS principle " +
                            ", PM.interest_pay + PM.prepaid_interest_pay + PM.early_redeem_pay AS interest " +
                            ", PM.penalty_pay AS total_penalty_paid,CUR.currency,CUR.currency_code,SI.name pawn_officer, ST.serial_number " +
                            " FROM  contract C " +
                            "LEFT JOIN payment PM ON PM.contract_id = C.id AND PM.payment_type = 'RD' " +
                            "LEFT JOIN customer CUS ON C.customer_id = CUS.id " +
                            "LEFT JOIN currency CUR ON C.currency_id = CUR.id " +
                            "LEFT JOIN staff_info SI ON C.pawn_officer_id = SI.id " +
                            "LEFT JOIN product PD ON PD.id = C.product_id " +
                            "LEFT JOIN(SELECT contract_id, MIN(ticket_no) AS ticket_no, MIN(serial_number) AS serial_number " +
                            "FROM schedule_ticket WHERE CASE WHEN '" + fromDay + "' IS NOT NULL THEN paid_date BETWEEN '" + fromDay + "' AND '" + systemDate + 
                            "' ELSE MONTH(paid_date) = " + month + " AND YEAR(paid_date) = " + year + " END GROUP BY contract_id) ST ON C.id = ST.contract_id " +
                            "WHERE C.branch_id = " + ddBranchName.SelectedValue + " AND C.contract_status = 6 AND C.b_status = 1 AND C.currency_id = " + ddCurrency.SelectedValue +
                            " AND CASE WHEN '" + fromDay + "' IS NOT NULL THEN C.redeem_date BETWEEN '" + fromDay + "' AND '" + systemDate + "' ELSE MONTH(C.redeem_date) = " + month + " AND YEAR(C.redeem_date) = " + year + " END " +
                            "AND C.pawn_officer_id = CASE WHEN " + ddOfficer.SelectedValue + " IS NULL THEN C.pawn_officer_id ELSE " + ddOfficer.SelectedValue + " END " +
                            "GROUP BY ST.contract_id";
            var sql = "SELECT  " +
                            "   c.id, c.redeem_date, c.product_id, cus.customer_name, pro.lob_name, st.ticket_no,   " +
                            "  p.principle_pay AS principle,  " +
                            "  p.interest_pay + early_redeem_pay AS interest,  " +
                            "   IFNULL(OI.total_other_income, 0) AS redeem_other_income,  " +
                            "   penalty_pay AS total_penalty_paid,  " +
                            "    cur.currency, staff.`name` pawn_officer,  " +
                            "   st.serial_number, IFNULL(WV.waive_amount, 0) AS waive_amount  " +
                            "   FROM payment_total ptt  " +
                            "   INNER JOIN system_date s ON ptt.system_date_id = s.id  " +
                            "   INNER JOIN payment p ON ptt.id = p.payment_total_id  " +
                            "   LEFT JOIN schedule_ticket st ON st.id = p.schedule_ticket_id  " +
                            "   INNER JOIN contract c ON ptt.contract_id = c.id  " +
                            "   INNER JOIN customer cus ON cus.id = c.customer_id  " +
                            "   INNER JOIN product pro ON pro.id = c.product_id  " +
                            "   INNER JOIN currency cur ON cur.id = c.currency_id  " +
                            "   INNER JOIN staff_info staff ON staff.id = c.pawn_officer_id  " +
                            "   LEFT JOIN   (  " +
                            "       SELECT payment_total_id, SUM(other_income_amount) AS total_other_income  " +
                            "       FROM payment_other_income  " +
                            "       GROUP BY payment_total_id  " +
                            "   ) OI ON OI.payment_total_id = ptt.id  " +
                            "   LEFT JOIN   (  " +
                            "       SELECT system_date_id, contract_id, SUM(waive_amount) waive_amount  " +
                            "       FROM waive  " +
                            "       WHERE waive_status = 2 AND trxn_type <= 2 GROUP BY system_date_id,contract_id  " +
                            "  ) WV ON c.id = WV.contract_id AND ptt.system_date_id = WV.system_date_id  " +
                            "   WHERE ptt.payment_flag = 3 AND c.redeem_type = 2  " +
                            "              AND ptt.payment_total_status = TRUE  " +
                            "   AND c.contract_status = 6 AND c.b_status = 1  " +
                            "   AND c.branch_id =   " + ddBranchName.SelectedValue +"  " +
                            "   AND c.currency_id =  " +ddCurrency.SelectedValue +"  " +
                            "   AND CASE WHEN "+dtpFromDate.Text +" IS NOT NULL THEN c.redeem_date BETWEEN DATE("+ dtpFromDate.Text +")  " +
                            "  AND DATE("+dtpSystemDate.Text+") ELSE MONTH(c.redeem_date) = MONTH("+dtpFromDate.Text+")  " +
                            "  AND YEAR(c.redeem_date) = YEAR("+dtpSystemDate.Text+") END  " +
                            "  AND c.pawn_officer_id = CASE WHEN " +ddOfficer.SelectedValue + " IS NULL THEN c.pawn_officer_id ELSE "+ ddOfficer.SelectedValue +" END  " +
                            "  rder by c.redeem_date ";
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
                dtpFromDate.Text = DataHelper.getSystemDateStr();
                dtpFromDate.Enabled = true;
            }
        }

        protected void ddBranchName_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddBranchName.SelectedValue != "")
            {
                ddOfficer.Enabled = true;
                DataHelper.populateOfficerDDL(ddOfficer, Convert.ToInt32(ddBranchName.SelectedValue));
            }
            else
            {
                ddOfficer.Enabled = false;
                ddOfficer.SelectedItem.Text = "";
            }
        }
    }
}