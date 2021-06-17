using Microsoft.Reporting.WebForms;
using MySql.Data.MySqlClient;
using Report.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Report.Utils;

namespace Report.BranchProductivity
{
    public partial class BranchProductivity : System.Web.UI.Page
    {
        private DBConnect db = new DBConnect();
        string urlPath = HttpContext.Current.Request.Url.AbsoluteUri;

        static List<Currency> currencyList;
        public static string systemDateStr;
        public string format = "dd/MM/yyyy";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DataHelper.checkLoginSession();
                DataHelper.populateBranchDDL(ddBranchName, DataHelper.getUserId());
                currencyList = DataHelper.populateCurrencyDDL(ddCurrency);
                systemDateStr = DataHelper.getSystemDateStr();
            }
        }

        //GenerateReport Function
        private void GenerateReport(DataTable branchProductivityDT)
        {
            var reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("BranchParameter", ddBranchName.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("CurrencyParameter", ddCurrency.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("SystemDateParameter", DateTime.ParseExact(systemDateStr, format, null).ToString("dd-MMM-yyyy")));

            var _branchProductivitylist = new ReportDataSource("BranchProductivityDS", branchProductivityDT);
            DataHelper.generateOperationReport(ReportViewer1, "BranchProductivity", reportParameters, _branchProductivitylist);
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            //Split System Date Time variable
            var systemDateSql = DateTime.ParseExact(systemDateStr, format, null);

            var branchProductivityProcedure = "PS_BRANCH_PROD";
            List<Procedure> procedureList = new List<Procedure>();
            procedureList.Add(item: new Procedure() { field_name = "@pBrnach", sql_db_type = MySqlDbType.VarChar, value_name = ddBranchName.SelectedItem.Value });
            procedureList.Add(item: new Procedure() { field_name = "@pSystem_date", sql_db_type = MySqlDbType.Date, value_name = DateTime.ParseExact(systemDateStr, format, null).ToString("yyyy-MM-dd") });
            procedureList.Add(item: new Procedure() { field_name = "@pCurrency", sql_db_type = MySqlDbType.VarChar, value_name = ddCurrency.SelectedItem.Value });

            DataTable branchProductivityPSDT = db.getProcedureDataTable(branchProductivityProcedure, procedureList);
            GenerateReport(branchProductivityPSDT);
        }
    }
}