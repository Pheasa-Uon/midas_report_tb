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
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DataHelper.checkLoginSession();
                currencyList = DataHelper.populateCurrencyDDL(ddCurrency);
            }
        }

        private void GenerateReport(DataTable productivityByBranchDT)
        {
            var reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("Currency", ddCurrency.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("SystemDate", DataHelper.getSystemDateStr()));

            var _productivityByBranchlist = new ReportDataSource("ProductivityConsolidateByLOBDS", productivityByBranchDT);
            DataHelper.generateOperationReport(ReportViewer1, "ProductivityConsolidateByLOB", reportParameters, _productivityByBranchlist);
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            var stp = "PS_PROD_BY_LOB_V1";
            List<Procedure> procedureList = new List<Procedure>();
            procedureList.Add(item: new Procedure() { field_name = "@pCurrency", sql_db_type = MySqlDbType.VarChar, value_name = ddCurrency.SelectedItem.Value });

            DataTable dt = db.getProcedureDataTable(stp, procedureList);
            GenerateReport(dt);
        }
    }
}