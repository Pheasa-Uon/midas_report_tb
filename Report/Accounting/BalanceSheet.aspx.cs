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
        DateTime currentDate = DateTime.Today;
        public string dateStr;
        public string format = "dd/MM/yyyy";

        protected void Page_Load(object sender, EventArgs e)
        {
            DataHelper.checkLoginSession();
            //Convert Date Block
            dateStr = dtpDate.Text;

            //Adding Text and Value to Branch DropdownList block
            if (!IsPostBack)
            {
                DataHelper.populateBranchDDLAllowAll(ddBranchName, DataHelper.getUserId());
                dtpDate.Text = currentDate.ToString(format);
            }
        }

        //GenerateReport Function
        private void GenerateReport(DataTable firstBalanceSheetDT, DataTable secondBalanceSheetDT)
        {
            //Generate Report Block
            ReportParameterCollection reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("BranchParameter", ddBranchName.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("DateParameter", DateTime.ParseExact(dtpDate.Text, format, null).ToString("dd-MMM-yyyy")));

            var _firstBalanceSheet = new ReportDataSource("DataSet2", firstBalanceSheetDT);
            var _secondBalanceSheet = new ReportDataSource("DataSet3", secondBalanceSheetDT);
            DataHelper.generateAccountingReport(ReportViewer1, "BalanceSheet", reportParameters, _firstBalanceSheet, _secondBalanceSheet);
        }

        protected void btnView_Click(object sender, EventArgs e)
        {   
            var firstBalanceSheetName = "PS_BSDETAIL";
            var secondBalanceSheetName = "PS_BSLC";
            List<Procedure> procedureList = new List<Procedure>();
            procedureList.Add(item: new Procedure() { field_name = "@pBranch", sql_db_type = MySqlDbType.VarChar, value_name = ddBranchName.SelectedItem.Value });
            procedureList.Add(item: new Procedure() { field_name = "@pDate", sql_db_type = MySqlDbType.Date, value_name = DateTime.ParseExact(dtpDate.Text, format, null).ToString("yyyy-MM-dd") });

            DataTable firstBalanceSheetDT = db.getProcedureDataTable(firstBalanceSheetName, procedureList);
            DataTable secondBalanceSheetDT = db.getProcedureDataTable(secondBalanceSheetName, procedureList);

            GenerateReport(firstBalanceSheetDT, secondBalanceSheetDT);
        }
    }
}