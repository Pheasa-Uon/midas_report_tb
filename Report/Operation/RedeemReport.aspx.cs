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
                currencyList = DataHelper.populateCurrencyDDL(ddCurrency);
                dtpSystemDate.Text = systemDateStr;
            }
        }

        //GenerateReport Function
        private void GenerateReport(DataTable redeemDT)
        {
            var reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("BranchParameter", ddBranchName.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("CurrencyParameter", ddCurrency.SelectedItem.Text));
            if (dtpFromDate.Text != "")
            {
                reportParameters.Add(new ReportParameter("FromDateParameter", DateTime.ParseExact(dtpFromDate.Text, format, null).ToString("dd-MMM-yyyy")));
            }
            else
            {
                reportParameters.Add(new ReportParameter("FromDateParameter", " "));
            }
            reportParameters.Add(new ReportParameter("SystemDateParameter", DateTime.ParseExact(dtpSystemDate.Text, format, null).ToString("dd-MMM-yyyy")));
            reportParameters.Add(new ReportParameter("CurrencyLabelParameter", currencyList.Find(x => x.id == Convert.ToInt32(ddCurrency.SelectedItem.Value)).currency_label));
          
            var _redeem = new ReportDataSource("RedeemDS", redeemDT);
            DataHelper.generateOperationReport(ReportViewer1, "Redeem", reportParameters, _redeem);
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

            DataTable redeemDT = db.getDataTable(redeemSql);
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