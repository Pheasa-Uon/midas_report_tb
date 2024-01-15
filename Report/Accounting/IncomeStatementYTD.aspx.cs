using Microsoft.Reporting.WebForms;
using MySql.Data.MySqlClient;
using Report.Models;
using Report.Utils;
using System;
using System.Collections.Generic;
using System.Data;

namespace Report.Accounting
{
    public partial class IncomeStatementYTD : System.Web.UI.Page
    {
        private DBConnect db = new DBConnect();
        private static string asOfDate;
        public string format = "dd/MM/yyyy";
        public string dateFromError = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var date = DataHelper.getSystemDateTextbox();
                DataHelper.checkLoginSession();
                DataHelper.populateBranchDDL(ddBranchName, DataHelper.getUserId());
                dtpAsOfDate.Text = DataHelper.getSystemDateTextbox();
            }
        }

        private void GenerateReport(DataTable dt)
        {
            ReportParameterCollection reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("Branch", ddBranchName.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("AsOfDate", DateTime.ParseExact(dtpAsOfDate.Text, format, null).ToString("dd-MMM-yyyy")));

            var ds = new ReportDataSource("IncomeStatementYTDDS", dt);

            DataHelper.generateAccountingReport(ReportViewer1, "IncomeStatementYTD", reportParameters, ds);
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            try
            {
                asOfDate = DateTime.ParseExact(dtpAsOfDate.Text.Trim(), format, null).ToString("yyyy-MM-dd");
            }
            catch (Exception)
            {
                dateFromError = "* Date wrong format";
                return;
            }

            var PS_INC_YTD = "PS_INC_YTD";

            List<Procedure> procedureList = new List<Procedure>();
            procedureList.Add(item: new Procedure() { field_name = "@pBranch", sql_db_type = MySqlDbType.VarChar, value_name = ddBranchName.SelectedItem.Value });
            procedureList.Add(item: new Procedure() { field_name = "@TO_Date", sql_db_type = MySqlDbType.Date, value_name = asOfDate });

            DataTable DT = db.getProcedureDataTable(PS_INC_YTD, procedureList);

            GenerateReport(DT);
        }
    }
}