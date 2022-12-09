using Microsoft.Reporting.WebForms;
using Report.Models;
using System;
using System.Collections.Generic;
using System.Data;
using Report.Utils;
using MySql.Data.MySqlClient;

namespace Report.Operation
{
    public partial class TurnOver : System.Web.UI.Page
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

            var ds = new ReportDataSource("TurnoverDS", dt);

            DataHelper.generateOperationReport(ReportViewer1, "TurnoverReport", reportParameters, ds);
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            string fromDay = null;
            if (dtpFromDate.Text != "")
            {
                var fromDateSql = DateTime.ParseExact(dtpFromDate.Text, format, null);
                fromDay = fromDateSql.ToString("yyyy-MM-dd");
            }

            var systemDateSql = DateTime.ParseExact(dtpSystemDate.Text, format, null);
            var systemDate = systemDateSql.ToString("yyyy-MM-dd");

            string officer = null;
            if (ddOfficer.SelectedItem.Value != "0")
            {
                officer = ddOfficer.SelectedItem.Value;
            }

            var spd = "PS_TURNOVER";
            List<Procedure> parameters = new List<Procedure>();
            parameters.Add(item: new Procedure() { field_name = "@branchId", sql_db_type = MySqlDbType.VarChar, value_name = ddBranchName.SelectedItem.Value });
            parameters.Add(item: new Procedure() { field_name = "@currencyId", sql_db_type = MySqlDbType.VarChar, value_name = ddCurrency.SelectedItem.Value });
            parameters.Add(item: new Procedure() { field_name = "@pFromDate", sql_db_type = MySqlDbType.Date, value_name = fromDay });
            parameters.Add(item: new Procedure() { field_name = "@pToDate", sql_db_type = MySqlDbType.Date, value_name = systemDate });
            parameters.Add(item: new Procedure() { field_name = "@pOfficer", sql_db_type = MySqlDbType.VarChar, value_name = officer });

            DataTable dt = db.getProcedureDataTable(spd, parameters);
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
                dtpFromDate.Text = DataHelper.getSystemDate().ToString("dd/MM/yyyy");
                dtpFromDate.Enabled = true;
            }
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