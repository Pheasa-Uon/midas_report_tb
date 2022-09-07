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
    public partial class BranchProductivityOnDate : System.Web.UI.Page
    {
        private DBConnect db = new DBConnect();
        string urlPath = HttpContext.Current.Request.Url.AbsoluteUri;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DataHelper.checkLoginSession();
                DataHelper.populateBranchDDL(ddBranchName, DataHelper.getUserId());
                DataHelper.populateCurrencyDDL(ddCurrency);
            }
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            var spd = "PS_BRANCH_PROD_ON_DATE";

            List<Procedure> parameters = new List<Procedure>();
            parameters.Add(item: new Procedure() { field_name = "@pBranch", sql_db_type = MySqlDbType.VarChar, value_name = ddBranchName.SelectedItem.Value });
            parameters.Add(item: new Procedure() { field_name = "@pCurrency", sql_db_type = MySqlDbType.VarChar, value_name = ddCurrency.SelectedItem.Value });

            DataTable dt = db.getProcedureDataTable("PS_BRANCH_PROD_ON_DATE", parameters);
            GenerateReport(dt);
        }

        private void GenerateReport(DataTable dt)
        {
            var reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("Branch", ddBranchName.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("SystemDate", DataHelper.getSystemDateStr()));
            reportParameters.Add(new ReportParameter("Currency", ddCurrency.SelectedItem.Text));

            var ds = new ReportDataSource("BranchProductivityOnDateDS", dt);
            DataHelper.generateOperationReport(ReportViewer1, "BranchProductivityOnDate", reportParameters, ds);
        }
    }
}