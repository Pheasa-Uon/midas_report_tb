using Microsoft.Reporting.WebForms;
using MySql.Data.MySqlClient;
using Report.Models;
using Report.Utils;
using System;
using System.Collections.Generic;
using System.Data;

namespace Report.Accounting
{
    public partial class FixedAssetListing : System.Web.UI.Page
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
                /*dtpSystemDate.Text = DataHelper.getSystemDateTextbox();*/
                DataHelper.populateBranchDDL(ddBranchName, DataHelper.getUserId());
            }
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            /*
            try
            {
                sysDate = DateTime.ParseExact(dtpSystemDate.Text.Trim(), format, null).ToString("yyyy-MM-dd");
            }
            catch (Exception)
            {
                dateFromError = "* Date wrong format";
                return;
            }
            */

            var PS_FA = "PS_FIXED_ASSET_LISTING";

            List<Procedure> procedureList = new List<Procedure>();
            procedureList.Add(item: new Procedure() { field_name = "@pBranch", sql_db_type = MySqlDbType.VarChar, value_name = ddBranchName.SelectedItem.Value });
           /* procedureList.Add(item: new Procedure() { field_name = "@pSystem_Date", sql_db_type = MySqlDbType.Date, value_name = sysDate }); */
            

            DataTable PS_FA_DS = db.getProcedureDataTable(PS_FA, procedureList);

            GenerateReport(PS_FA_DS);
        }

        private void GenerateReport(DataTable PS_FA_DT)
        {
            ReportParameterCollection reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("Branch", ddBranchName.SelectedItem.Text));
            /*reportParameters.Add(new ReportParameter("SystemDate", DateTime.ParseExact(dtpSystemDate.Text, format, null).ToString("dd-MMM-yyyy")));*/
           
            var PS_FA_DS = new ReportDataSource("FixedAssetListing", PS_FA_DT);

            DataHelper.generateAccountingReport(ReportViewer1, "FixedAssetListing", reportParameters, PS_FA_DS);
        }
    }
}