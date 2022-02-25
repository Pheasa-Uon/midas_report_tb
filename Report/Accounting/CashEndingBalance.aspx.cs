using Microsoft.Reporting.WebForms;
using MySql.Data.MySqlClient;
using Report.Models;
using Report.Utils;
using System;
using System.Collections.Generic;
using System.Data;

namespace Report.Accounting
{
    public partial class CashEndingBalance : System.Web.UI.Page
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
            
            List<Procedure> parameters = new List<Procedure>();
            parameters.Add(item: new Procedure() { field_name = "@pSystemDate", sql_db_type = MySqlDbType.VarChar, value_name = dateSearch });
            GenerateReport(db.getProcedureDataTable("PS_CASH_ENDING", parameters), db.getProcedureDataTable("PS_CASH_ENDING_SUMMARY", parameters));
        }

        private void GenerateReport(DataTable dt, DataTable dtSummary)
        {
            var reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("SystemDate", DateTime.ParseExact(dtpSystemDate.Text.Trim(), format, null).ToString("dd-MMM-yyyy")));
            
            DataHelper.generateAccountingReport(ReportViewer1, "CashEndingBalance", reportParameters, new ReportDataSource("CashEndingBalanceDS", dt), new ReportDataSource("CashEndingSummaryDS", dtSummary));
        }
    }
}