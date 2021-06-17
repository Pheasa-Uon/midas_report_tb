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

namespace Report.Operation
{
    public partial class ProductivityConsolidateByLOB : System.Web.UI.Page
    {
        private DBConnect db = new DBConnect();
        static List<Currency> currencyList;
        public static string systemDateStr;
        public string format = "dd/MM/yyyy";
        protected void Page_Load(object sender, EventArgs e)
        {
            DataHelper.checkLoginSession();
            if (!IsPostBack)
            {
                currencyList = DataHelper.populateCurrencyDDL(ddCurrency);
                systemDateStr = DataHelper.getSystemDateStr();
            }
        }

        //GenerateReport Function
        private void GenerateReport(DataTable productivityByLOBDT)
        {
            var reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("CurrencyParameter", ddCurrency.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("SystemDateParameter", DateTime.ParseExact(systemDateStr, format, null).ToString("dd-MMM-yyyy")));

            var _productivityByLOBlist = new ReportDataSource("ProductivityByLOBDS", productivityByLOBDT);
            DataHelper.generateOperationReport(ReportViewer1, "ProductivityConsolidateByLOB", reportParameters, _productivityByLOBlist);
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            //Split System Date Time variable
            var systemDateSql = DateTime.ParseExact(systemDateStr, format, null);

            var productivityByLOBProcedure = "PS_PROD_BY_LOB";
            List<Procedure> procedureList = new List<Procedure>();
            procedureList.Add(item: new Procedure() { field_name = "@pSystem_date", sql_db_type = MySqlDbType.Date, value_name = DateTime.ParseExact(systemDateStr, format, null).ToString("yyyy-MM-dd") });
            procedureList.Add(item: new Procedure() { field_name = "@pCurrency", sql_db_type = MySqlDbType.VarChar, value_name = ddCurrency.SelectedItem.Value });

            DataTable productivityByLOBDT = db.getProcedureDataTable(productivityByLOBProcedure, procedureList);
            GenerateReport(productivityByLOBDT);
        }
    }
}