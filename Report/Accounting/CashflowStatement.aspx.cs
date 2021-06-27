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
        public string format = "dd/MM/yyyy";
        public string fromDate, toDate;
        public string dateFromError = "", dateToError = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DataHelper.checkLoginSession();
                DataHelper.populateBranchDDL(ddBranchName, DataHelper.getUserId());
                DataHelper.populateCurrencyDDL(ddCurrency);
                var d = DataHelper.getSystemDate().ToString(format);
                dtpFromDate.Text = d;
                dtpToDate.Text = d;
            }
        }
        
        private void GenerateReport(DataTable cashFlowStatementDT)
        {
            ReportParameterCollection reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("Branch", ddBranchName.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("Currency",ddCurrency.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("FromDate", DateTime.ParseExact(dtpFromDate.Text, format, null).ToString("dd-MMM-yyyy")));
            reportParameters.Add(new ReportParameter("ToDate", DateTime.ParseExact(dtpToDate.Text, format, null).ToString("dd-MMM-yyyy")));

            var _cashFlowStatement = new ReportDataSource("CashFlowDS", cashFlowStatementDT);
            DataHelper.generateAccountingReport(ReportViewer1, "CashflowStatement", reportParameters, _cashFlowStatement);
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            try
            {
                fromDate = DateTime.ParseExact(dtpFromDate.Text.Trim(), format, null).ToString("yyyy-MM-dd");
            }
            catch (Exception)
            {
                dateFromError = "* Date wrong format";
                return;
            }
            try
            {
                toDate = DateTime.ParseExact(dtpToDate.Text.Trim(), format, null).ToString("yyyy-MM-dd");
            }
            catch (Exception)
            {
                dateToError = "* Date wrong format";
                return;
            }
            
            var cashFlowProcedureName = "CSHFL";
            List<Procedure> procedureList = new List<Procedure>();
            procedureList.Add(item: new Procedure() { field_name = "@pBranch", sql_db_type = MySqlDbType.VarChar, value_name = ddBranchName.SelectedItem.Value });
            procedureList.Add(item: new Procedure() { field_name = "@pCCRCD", sql_db_type = MySqlDbType.VarChar , value_name = ddCurrency.SelectedItem.Value });
            procedureList.Add(item: new Procedure() { field_name = "@FR_Date", sql_db_type = MySqlDbType.Date , value_name = fromDate });
            procedureList.Add(item: new Procedure() { field_name = "@TO_Date", sql_db_type = MySqlDbType.Date , value_name = toDate });

            DataTable chartOfAccountDT = db.getProcedureDataTable(cashFlowProcedureName, procedureList);
            GenerateReport(chartOfAccountDT);
        }
    }
}