using Microsoft.Reporting.WebForms;
using Report.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
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
            DataHelper.checkLoginSession();
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

        private void GenerateReport(DataTable newGrantDT)
        {
            ReportParameterCollection reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("BranchParameter", ddBranchName.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("OfficerParameter", ddOfficer.SelectedItem.Text));
            if (dtpFromDate.Text != "")
            {
                reportParameters.Add(new ReportParameter("FromDateParameter", DateTime.ParseExact(dtpFromDate.Text, format, null).ToString("dd-MMM-yyyy")));
            }
            else
            {
                reportParameters.Add(new ReportParameter("FromDateParameter", " "));
            }
            reportParameters.Add(new ReportParameter("SystemDateParameter", DateTime.ParseExact(dtpSystemDate.Text, format, null).ToString("dd-MMM-yyyy")));
            reportParameters.Add(new ReportParameter("CurrencyParameter", ddCurrency.SelectedItem.Text));
            var _newGrant = new ReportDataSource("NewGrantDS", newGrantDT);

            DataHelper.generateOperationReport(ReportViewer1, "NewGrant", reportParameters, _newGrant);
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

            var newGrantSql = "SELECT C.id,C.disbursement_date,CUS.customer_name,ST.ticket_no,ST.due_date,C.pawn_price_approved,CUR.currency,CUR.currency_code,SI.`name` pawn_officer,PD.lob_name, " +
                              "CASE WHEN ST.ticket_status = 'P' THEN ST.interest WHEN ST.ticket_status = 'FPP' THEN ST.interest WHEN ST.ticket_status = 'PAP' OR ST.ticket_status = 'PPP' THEN(ST.interest - ST.interest_less) ELSE 0 END AS int_amt " +
                              "FROM contract C " +
                              "LEFT JOIN schedule_ticket ST    ON ST.contract_id = C.id " +
                              "LEFT JOIN customer CUS ON C.customer_id = CUS.id " +
                              "LEFT JOIN currency CUR ON C.currency_id = CUR.id " +
                              "LEFT JOIN staff_info SI ON C.pawn_officer_id = SI.id " +
                              "LEFT JOIN product PD ON PD.id = C.product_id " +
                              "WHERE C.branch_id = " + ddBranchName.SelectedValue + " AND C.contract_status >= 4 AND C.`b_status`= 1 AND C.currency_id = " + ddCurrency.SelectedValue +
                              " AND CASE WHEN " + fromDay + " IS NULL THEN MONTH(C.disbursement_date) = " + month + " AND YEAR(C.disbursement_date) = " + year + " ELSE C.disbursement_date BETWEEN '" + fromDay + "' AND '" + systemDate + "' END AND ST.order_no = 1 " +
                              "AND C.pawn_officer_id = CASE WHEN " + ddOfficer.SelectedValue + " IS NULL THEN C.pawn_officer_id ELSE " + ddOfficer.SelectedValue + " END " +
                              "ORDER BY C.currency_id; ";

            DataTable newGrantDT = db.getDataTable(newGrantSql);
            GenerateReport(newGrantDT);
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

        protected void chkFromDate_CheckedChanged(object sender, EventArgs e)
        {
            if (chkFromDate.Checked)
            {
                dtpFromDate.Text = null;
                dtpFromDate.Enabled = false;
            }
            else
            {
                dtpFromDate.Text = DataHelper.getSystemDate().ToString("dd/MM/yyyy");
                dtpFromDate.Enabled = true;
            }
        }
    }
}