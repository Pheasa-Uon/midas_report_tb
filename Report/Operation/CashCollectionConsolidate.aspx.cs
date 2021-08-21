using Microsoft.Reporting.WebForms;
using MySql.Data.MySqlClient;
using Report.Models;
using Report.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Report.Operation
{
    public partial class CashCollectionConsolidate : System.Web.UI.Page
    {
        private DBConnect db = new DBConnect();
        string urlPath = HttpContext.Current.Request.Url.AbsoluteUri;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DataHelper.checkLoginSession();
                DataHelper.populateCurrencyDDL(ddCurrency);
            }
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            var spd = "PS_CASH_COLLECTION_CONSOLIDATE";
            List<Procedure> parameters = new List<Procedure>();
            parameters.Add(item: new Procedure() { field_name = "@pCurrency", sql_db_type = MySqlDbType.VarChar, value_name = ddCurrency.SelectedItem.Value });
            DataTable dt = db.getProcedureDataTable(spd, parameters);
            GenerateReport(dt);
        }

        private void GenerateReport(DataTable dt)
        {
            var reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("SystemDate", DataHelper.getSystemDateStr()));
            reportParameters.Add(new ReportParameter("Currency", ddCurrency.SelectedItem.Text));

            var ds = new ReportDataSource("CCConsolidateDS", dt);
            DataHelper.generateOperationReport(ReportViewer1, "CashCollectionConsolidate", reportParameters, ds);
        }
    }
}