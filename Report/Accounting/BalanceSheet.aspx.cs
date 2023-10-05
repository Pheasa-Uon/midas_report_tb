using Microsoft.Reporting.WebForms;
using MySql.Data.MySqlClient;
using Report.Models;
using Report.Utils;
using System;
using System.Collections.Generic;
using System.Data;

namespace Report.Accounting
{
    public partial class BalanceSheet : System.Web.UI.Page
    {
        private DBConnect db = new DBConnect();
        public string format = "dd/MM/yyyy";
        public string dateError = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            /*
            if (!IsPostBack)
            {
                DataHelper.checkLoginSession();
                if (Convert.ToBoolean(Session["isSuperAdmin"]))
                {
                    DataHelper.populateBranchDDLAllowAll(ddBranchName, DataHelper.getUserId());
                }
                else
                {
                    DataHelper.populateBranchDDL(ddBranchName, DataHelper.getUserId());
                }
                
                dtpSystemDate.Text = DataHelper.getSystemDate().ToString(format);
            } */
            if (!IsPostBack)
            {
                DataHelper.checkLoginSession();
                DataHelper.populateBranchDDL(ddBranchName, DataHelper.getUserId());
                var d = DataHelper.getSystemDate().ToString(format);
                dtpSystemDate.Text = d;
            }

        }
        
        private void GenerateReport(DataTable firstBalanceSheetDT, DataTable secondBalanceSheetDT)
        {
            ReportParameterCollection reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("Branch", ddBranchName.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("SystemDate", DateTime.ParseExact(dtpSystemDate.Text, format, null).ToString("dd-MMM-yyyy")));

            var _firstBalanceSheet = new ReportDataSource("DataSet2", firstBalanceSheetDT);
            var _secondBalanceSheet = new ReportDataSource("DataSet3", secondBalanceSheetDT);
            DataHelper.generateAccountingReport(ReportViewer1, "BalanceSheet", reportParameters, _firstBalanceSheet, _secondBalanceSheet);
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            var dateSearch = "";
            try
            {
                dateSearch = DateTime.ParseExact(dtpSystemDate.Text, format, null).ToString("yyyy-MM-dd");
            }
            catch(Exception)
            {
                dateError = "* Date wrong format";
                return;
            }
            var firstBalanceSheetName = "PS_BSDETAIL";
            var secondBalanceSheetName = "PS_BSLC";
            List<Procedure> procedureList = new List<Procedure>();
            procedureList.Add(item: new Procedure() { field_name = "@pBranch", sql_db_type = MySqlDbType.VarChar, value_name = ddBranchName.SelectedItem.Value });
            procedureList.Add(item: new Procedure() { field_name = "@pDate", sql_db_type = MySqlDbType.Date, value_name = dateSearch});

            DataTable firstBalanceSheetDT = db.getProcedureDataTable(firstBalanceSheetName, procedureList);
            DataTable secondBalanceSheetDT = db.getProcedureDataTable(secondBalanceSheetName, procedureList);

            GenerateReport(firstBalanceSheetDT, secondBalanceSheetDT);
        }
    }
}