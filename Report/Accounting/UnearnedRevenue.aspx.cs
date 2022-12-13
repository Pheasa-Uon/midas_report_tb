using Microsoft.Reporting.WebForms;
using MySql.Data.MySqlClient;
using Report.Models;
using Report.Utils;
using System;
using System.Collections.Generic;
using System.Data;

namespace Report.Accounting
{
    public partial class UnearnedRevenue : System.Web.UI.Page
    {
        private DBConnect db = new DBConnect();
        public string format = "dd/MM/yyyy";
        public string dateFromError;
        public string sysDate = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DataHelper.checkLoginSession();
                dtpSystemDate.Text = DataHelper.getSystemDateTextbox();
                DataHelper.populateBranchDDL(ddBranchName, DataHelper.getUserId());
                populateOfficer();
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

        protected void btnView_Click(object sender, EventArgs e)
        {
            try
            {
                sysDate = DateTime.ParseExact(dtpSystemDate.Text.Trim(), format, null).ToString("yyyy-MM-dd");
            }
            catch (Exception)
            {
                dateFromError = "* Date wrong format";
                return;
            }

            var PS_UNEARN = "PS_UNEARN";
            var PS_UNEARN_ADJUST = "PS_UNEARN_ADJUST";

            List<Procedure> procedureList = new List<Procedure>();
            procedureList.Add(item: new Procedure() { field_name = "@pBranch", sql_db_type = MySqlDbType.VarChar, value_name = ddBranchName.SelectedItem.Value });
            procedureList.Add(item: new Procedure() { field_name = "@pSystem_Date", sql_db_type = MySqlDbType.Date, value_name = sysDate });
            procedureList.Add(item: new Procedure() { field_name = "@pOffice", sql_db_type = MySqlDbType.VarChar, value_name = ddOfficer.SelectedItem.Value == "0" ? null : ddOfficer.SelectedItem.Value });

            DataTable PS_UNEARN_DS = db.getProcedureDataTable(PS_UNEARN, procedureList);
            DataTable PS_UNEARN_ADJUST_DS = db.getProcedureDataTable(PS_UNEARN_ADJUST, procedureList);

            GenerateReport(PS_UNEARN_DS, PS_UNEARN_ADJUST_DS);
        }

        private void GenerateReport(DataTable PS_UNEARN_DT, DataTable PS_UNEARN_ADJUST_DT)
        {
            ReportParameterCollection reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("Branch", ddBranchName.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("SystemDate", DateTime.ParseExact(dtpSystemDate.Text, format, null).ToString("dd-MMM-yyyy")));
            reportParameters.Add(new ReportParameter("PawnOfficer", ddOfficer.SelectedItem.Text));

            var PS_UNEARN_DS = new ReportDataSource("UNEARNED_DS", PS_UNEARN_DT);
            var PS_UNEARN_ADJUST_DS = new ReportDataSource("UNEARNED_AJ_DS", PS_UNEARN_ADJUST_DT);

            DataHelper.generateAccountingReport(ReportViewer1, "UnearnedRevenue", reportParameters,
                PS_UNEARN_DS,
                PS_UNEARN_ADJUST_DS
                );
        }
    }
}