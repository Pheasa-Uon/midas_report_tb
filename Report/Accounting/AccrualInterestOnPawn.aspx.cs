using Microsoft.Reporting.WebForms;
using MySql.Data.MySqlClient;
using Report.Models;
using Report.Utils;
using System;
using System.Collections.Generic;
using System.Data;

namespace Report.Accounting
{
    public partial class AccrualInterestOnPawn : System.Web.UI.Page
    {
        private DBConnect db = new DBConnect();
        public static string systemDateStr;
        public static string maxDateStr;
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
                dtpSystemDate.Text = systemDateStr;
            }
            
        }

        //GenerateReport Function
        private void GenerateReport(DataTable airPSDT)
        {
            //Generate Report Block
            ReportParameterCollection reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("BranchParameter", ddBranchName.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("OfficerParameter", ddOfficer.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("SystemDateParameter", DateTime.ParseExact(dtpSystemDate.Text, format, null).ToString("dd-MMM-yyyy")));

            var _accrualInterest = new ReportDataSource("PawnDetailDS", airPSDT);
            DataHelper.generateAccountingReport(ReportViewer1, "AccrualInterestOnPawn", reportParameters, _accrualInterest);
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            //Split System Date Time variable
            var systemDateSql = DateTime.ParseExact(dtpSystemDate.Text, format, null);
            maxDateStr = db.GetMaxDate(dtpSystemDate.Text, ddBranchName.SelectedItem.Value);

            var officerStr = ddOfficer.SelectedItem.Value;
            if(officerStr != "null")
            {
                officerStr = ddOfficer.SelectedItem.Value;
            }
            else
            {
                officerStr = null;
            }

            var airProcedure = "PS_AIR";
            List<Procedure> procedureList = new List<Procedure>();
            procedureList.Add(item: new Procedure() { field_name = "@pBranch", sql_db_type = MySqlDbType.VarChar, value_name = ddBranchName.SelectedItem.Value });
            procedureList.Add(item: new Procedure() { field_name = "@pSystem_Date", sql_db_type = MySqlDbType.Date, value_name = DateTime.ParseExact(dtpSystemDate.Text, format, null).ToString("yyyy-MM-dd") });
            procedureList.Add(item: new Procedure() { field_name = "@pMax_Date", sql_db_type = MySqlDbType.VarChar, value_name = DateTime.ParseExact(maxDateStr, format, null).ToString("yyyy-MM-dd") });
            procedureList.Add(item: new Procedure() { field_name = "@pOffice", sql_db_type = MySqlDbType.VarChar, value_name = officerStr });

            DataTable airPSDT = db.getProcedureDataTable(airProcedure, procedureList);
            GenerateReport(airPSDT);
        }

        protected void ddBranchName_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(ddBranchName.SelectedValue != "")
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