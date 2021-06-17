using Microsoft.Reporting.WebForms;
using Report.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using Report.Utils;

namespace Report.Accounting
{
    public partial class CashflowStatement : System.Web.UI.Page
    {
        private DBConnect db = new DBConnect();
        DateTime currentDate = DateTime.Today;
        public static string systemDateStr;
        public string format = "dd/MM/yyyy";
        public string fromDate, toDate;
        protected void Page_Load(object sender, EventArgs e)
        {
            DataHelper.checkLoginSession();
            //Convert Date Block
            fromDate = dtpFromDate.Text;
            toDate = dtpToDate.Text;

            //Adding Text and Value to Branch DropdownList block
            if (!IsPostBack)
            {
                DataHelper.populateBranchDDL(ddBranchName, DataHelper.getUserId());
                DataHelper.populateCurrencyDDL(ddCurrency);
                systemDateStr = DataHelper.getSystemDateStr();
                dtpFromDate.Text = DataHelper.getSystemDateStr();
                dtpToDate.Text = DataHelper.getSystemDateStr();
            }
        }

        //GenerateReport Function
        private void GenerateReport(DataTable cashFlowStatementDT)
        {
            //Generate Report Block
            ReportParameterCollection reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("BranchParameter", ddBranchName.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("CurrencyParameter",ddCurrency.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("FromDateParameter", DateTime.ParseExact(dtpFromDate.Text, format, null).ToString("dd-MMM-yyyy")));
            reportParameters.Add(new ReportParameter("ToDateParameter", DateTime.ParseExact(dtpToDate.Text, format, null).ToString("dd-MMM-yyyy")));

            var _cashFlowStatement = new ReportDataSource("CashFlowDS", cashFlowStatementDT);
            DataHelper.generateAccountingReport(ReportViewer1, "CashflowStatement", reportParameters, _cashFlowStatement);
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            var cashFlowProcedureName = "CSHFL";
            List<Procedure> procedureList = new List<Procedure>();
            procedureList.Add(item: new Procedure() { field_name = "@pBranch", sql_db_type = MySqlDbType.VarChar, value_name = ddBranchName.SelectedItem.Value });
            procedureList.Add(item: new Procedure() { field_name = "@pCCRCD", sql_db_type = MySqlDbType.VarChar , value_name = ddCurrency.SelectedItem.Value });
            procedureList.Add(item: new Procedure() { field_name = "@FR_Date", sql_db_type = MySqlDbType.Date , value_name = DateTime.ParseExact(dtpFromDate.Text, format, null).ToString("yyyy-MM-dd") });
            procedureList.Add(item: new Procedure() { field_name = "@TO_Date", sql_db_type = MySqlDbType.Date , value_name = DateTime.ParseExact(dtpToDate.Text, format, null).ToString("yyyy-MM-dd") });

            DataTable chartOfAccountDT = db.getProcedureDataTable(cashFlowProcedureName, procedureList);
            GenerateReport(chartOfAccountDT);
        }
    }
}