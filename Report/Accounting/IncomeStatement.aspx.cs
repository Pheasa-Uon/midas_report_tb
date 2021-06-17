 using Microsoft.Reporting.WebForms;
using MySql.Data.MySqlClient;
using Report.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Report.Utils;

namespace Report.Accounting
{
    public partial class IncomeStatement : System.Web.UI.Page
    {
        private DBConnect db = new DBConnect();
        DateTime currentDate = DateTime.Today;
        private string fromDate, toDate;
        public string format = "dd/MM/yyyy";
        protected void Page_Load(object sender, EventArgs e)
        {
            DataHelper.checkLoginSession();
            //Convert Date Block
            fromDate = dtpFromDate.Text;
            toDate = dtpToDate.Text;

            //Adding Text and Value to Branch DropdownList block
            if (!IsPostBack)
            {
                DataHelper.populateBranchDDLAllowAll(ddBranchName, DataHelper.getUserId());
                dtpFromDate.Text = DataHelper.getSystemDateStr();
                dtpToDate.Text = DataHelper.getSystemDateStr();
            }
        }

        //GenerateReport Function
        private void GenerateReport(DataTable firstIncomeStatementDT, DataTable secondIncomeStatementDT)
        {
            //Generate Report Block
            ReportParameterCollection reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("FromDateParameter", DateTime.ParseExact(dtpFromDate.Text, format, null).ToString("dd-MMM-yyyy")));
            reportParameters.Add(new ReportParameter("ToDateParameter", DateTime.ParseExact(dtpToDate.Text, format, null).ToString("dd-MMM-yyyy")));
            var _firstIncomeStatement = new ReportDataSource("PS_Details", firstIncomeStatementDT);
            var _secondIncomeStatement = new ReportDataSource("PS_PL", secondIncomeStatementDT);
            DataHelper.generateAccountingReport(ReportViewer1, "IncomeStatement", reportParameters, _firstIncomeStatement, _secondIncomeStatement);
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            var firstIncomeStatementName = "PS_ISDETAIL";
            var secondIncomeStatementName = "PS_PL";
            List<Procedure> procedureList = new List<Procedure>();
            procedureList.Add(item: new Procedure() { field_name = "@pBranch", sql_db_type = MySqlDbType.VarChar, value_name = ddBranchName.SelectedItem.Value });
            procedureList.Add(item: new Procedure() { field_name = "@FR_Date", sql_db_type = MySqlDbType.Date, value_name = DateTime.ParseExact(dtpFromDate.Text, format, null).ToString("yyyy-MM-dd") });
            procedureList.Add(item: new Procedure() { field_name = "@TO_Date", sql_db_type = MySqlDbType.Date, value_name = DateTime.ParseExact(dtpToDate.Text, format, null).ToString("yyyy-MM-dd") });

            DataTable firstIncomeStatementDT = db.getProcedureDataTable(firstIncomeStatementName, procedureList);
            DataTable secondIncomeStatementDT = db.getProcedureDataTable(secondIncomeStatementName, procedureList);
            GenerateReport(firstIncomeStatementDT, secondIncomeStatementDT);
        }
    }
}