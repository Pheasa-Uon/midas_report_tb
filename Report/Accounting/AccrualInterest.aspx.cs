using Microsoft.Reporting.WebForms;
using MySql.Data.MySqlClient;
using Report.Models;
using Report.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Report.Accounting
{
    public partial class AccrualInterest : System.Web.UI.Page
    {
        private DBConnect db = new DBConnect();
        public string format = "dd/MM/yyyy";
        public string dateError = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DataHelper.checkLoginSession();
                dtpSystemDate.Text = DataHelper.getSystemDate().ToString(format);
                DataHelper.populateBranchDDL(ddBranchName, DataHelper.getUserId());
                popualateOfficer();
            }
        }

        protected void ddBranchName_SelectedIndexChanged(object sender, EventArgs e)
        {
            popualateOfficer();
        }

        private void popualateOfficer()
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
            var dateSearch = "";
            try
            {
                dateSearch = DateTime.ParseExact(dtpSystemDate.Text.Trim(), format, null).ToString("yyyy-MM-dd");
            }
            catch (Exception)
            {
                dateError = "* Date wrong format";
                return;
            }
            string officer = null;
            if (ddOfficer.SelectedItem.Value != "0")
            {
                officer = ddOfficer.SelectedItem.Value;
            }

            var spd = "PS_AIR6";
            List<Procedure> parameters = new List<Procedure>();
            parameters.Add(item: new Procedure() { field_name = "@pBranch", sql_db_type = MySqlDbType.VarChar, value_name = ddBranchName.SelectedItem.Value });
            parameters.Add(item: new Procedure() { field_name = "@pSystem_Date", sql_db_type = MySqlDbType.VarChar, value_name = dateSearch});
            parameters.Add(item: new Procedure() { field_name = "@pOffice", sql_db_type = MySqlDbType.VarChar, value_name = officer });

            DataTable dt = db.getProcedureDataTable(spd, parameters);
            GenerateReport(dt);
        }

        private void GenerateReport(DataTable dt)
        {
            var reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("Branch", ddBranchName.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("SystemDate", DateTime.ParseExact(dtpSystemDate.Text.Trim(), format, null).ToString("dd-MMM-yyyy")));
            reportParameters.Add(new ReportParameter("PawnOfficer", ddOfficer.SelectedItem.Text));

            var ds = new ReportDataSource("AIR_DS", dt);
            DataHelper.generateAccountingReport(ReportViewer1, "AccrualInterest", reportParameters, ds);
        }
    }
}