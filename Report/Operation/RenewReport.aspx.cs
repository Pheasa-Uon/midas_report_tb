using Microsoft.Reporting.WebForms;
using Report.Models;
using System;
using System.Collections.Generic;
using System.Data;
using Report.Utils;

namespace Report.Operation
{
    public partial class Renew : System.Web.UI.Page
    {
        private DBConnect db = new DBConnect();
        static List<Currency> currencyList;
        public static string fromDate, systemDateStr;
        public string format = "dd/MM/yyyy";

        protected void Page_Load(object sender, EventArgs e)
        {
            DataHelper.checkLoginSession();
            //Convert System Date Block
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
        private void GenerateReport(DataTable renewDT)
        {
            //Generate Report Block
            ReportParameterCollection reportParameters = new ReportParameterCollection();
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

            var _renew = new ReportDataSource("RenewDS", renewDT);
            DataHelper.generateOperationReport(ReportViewer1, "Renew", reportParameters, _renew);
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

        protected void btnView_Click(object sender, EventArgs e)
        {
            //Split System Date Time variable
            var fromDay = new Object();
            var fromDayDate = new Object();
            if (dtpFromDate.Text != "")
            {
                var fromDateSql = DateTime.ParseExact(dtpFromDate.Text, format, null);
                fromDay = fromDateSql.ToString("yyyy-MM-dd");
                fromDayDate = fromDateSql.ToString("dd");
            }
            else
            {
                fromDay = "null";
                fromDayDate = "null";
            }

            var systemDateSql = DateTime.ParseExact(dtpSystemDate.Text, format, null);
            var systemDate = systemDateSql.ToString("yyyy-MM-dd");
            var month = systemDateSql.ToString("MM");
            var year = systemDateSql.ToString("yyyy");

            var renewSql = "SELECT CASE WHEN ST.`ticket_status` = 'P' THEN ST.paid_date WHEN ST.`ticket_status` = 'FPP' THEN ST.prepaid_date END as trnx_date, " +
                           "C.customer_name,ST.ticket_no,P.lob_name,ST.principle,ST.interest,ST.total_penalty_paid, ST.principle_due principle_less " +
                           ", CASE WHEN IC.product_type_id = 1 THEN STT.due_date ELSE ST.due_date END due_date,ST.serial_number, SI.name pawn_officer " +
                           "FROM schedule_ticket ST LEFT JOIN contract IC ON ST.contract_id = IC.id " +
                           "LEFT JOIN schedule_ticket STT ON ST.id + 1 = STT.id " +
                           "LEFT JOIN product P ON IC.product_id = P.id " +
                           "LEFT JOIN customer C ON IC.customer_id = C.id " +
                           "LEFT JOIN contract CON ON ST.contract_id = CON.id " +
                           "LEFT JOIN staff_info SI ON CON.pawn_officer_id = SI.id " +
                           "WHERE(ST.`ticket_status` = 'P' OR ST.`ticket_status` = 'FPP') " +
                           "AND CASE WHEN '" + fromDay + "' IS NULL THEN " +
                           "            CASE WHEN ST.`ticket_status` = 'P' THEN MONTH(ST.paid_date) = " + month + " AND YEAR(ST.paid_date) = " + year +
                           "            WHEN ST.`ticket_status` = 'FPP' THEN MONTH(ST.prepaid_date) = " + month + " AND YEAR(ST.prepaid_date) = " + year +
                           "            END " +
                           "    ELSE CASE WHEN ST.ticket_status = 'P' THEN ST.paid_date WHEN ST.ticket_status = 'FPP' THEN ST.prepaid_date END BETWEEN '" + fromDay + "' AND '" + systemDate + "' " +
                           "END " +
                           "AND ST.order_no > 1 AND ST.last_schedule = FALSE AND IC.contract_status = 4 AND IC.`b_status`= 1 " +
                           "AND ST.branch_id = " + ddBranchName.SelectedValue + " AND IC.currency_id = " + ddCurrency.SelectedValue +
                           " AND CON.pawn_officer_id = CASE WHEN " + ddOfficer.SelectedValue + " IS NULL THEN CON.pawn_officer_id ELSE " + ddOfficer.SelectedValue + " END";

            DataTable renewDT = db.getDataTable(renewSql);
            GenerateReport(renewDT);
        }
    }
}
