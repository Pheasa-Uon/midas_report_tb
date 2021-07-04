using Microsoft.Reporting.WebForms;
using MySql.Data.MySqlClient;
using Report.Models;
using Report.Utils;
using System;
using System.Collections.Generic;
using System.Data;

namespace Report.Accounting
{
    public partial class DailyTillPettyCash : System.Web.UI.Page
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
                dtpFromDate.Text = DataHelper.getSystemDateTextbox();
                dtpToDate.Text = DataHelper.getSystemDateTextbox();
                DataHelper.populateBranchDDL(ddBranchName, DataHelper.getUserId());
            }
        }
        private void GenerateReport(DataTable cashFlowStatementDT)
        {
            ReportParameterCollection reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("Branch", ddBranchName.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("FromDate", DateTime.ParseExact(dtpFromDate.Text, format, null).ToString("dd-MMM-yyyy")));
            reportParameters.Add(new ReportParameter("ToDate", DateTime.ParseExact(dtpToDate.Text, format, null).ToString("dd-MMM-yyyy")));

            var ds = new ReportDataSource("DailyTillPettyCashDS", cashFlowStatementDT);
            DataHelper.generateAccountingReport(ReportViewer1, "DailyTillPettyCash", reportParameters, ds);
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

            var stp = "PS_TILL_PETTY";
            List<Procedure> procedureList = new List<Procedure>();
            procedureList.Add(item: new Procedure() { field_name = "@branchId", sql_db_type = MySqlDbType.VarChar, value_name = ddBranchName.SelectedItem.Value });
            procedureList.Add(item: new Procedure() { field_name = "@pFromDate", sql_db_type = MySqlDbType.Date, value_name = fromDate });
            procedureList.Add(item: new Procedure() { field_name = "@pToDate", sql_db_type = MySqlDbType.Date, value_name = toDate });

            DataTable dt = db.getProcedureDataTable(stp, procedureList);
            GenerateReport(dt);
        }
    }
}