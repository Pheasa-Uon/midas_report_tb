using Microsoft.Reporting.WebForms;
using Report.Utils;
using System;
using System.Data;

namespace Report.Accounting
{
    public partial class FixedAssetListing : System.Web.UI.Page
    {
        private DBConnect db = new DBConnect();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DataHelper.checkLoginSession();
                DataHelper.populateBranchDDL(ddBranchName, DataHelper.getUserId());
            }
        }

        private void GenerateReport(DataTable dt)
        {
            ReportParameterCollection reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("Branch", ddBranchName.SelectedItem.Text));

            var ds = new ReportDataSource("FAListing", dt);
            DataHelper.generateAccountingReport(ReportViewer1, "FixedAssetListing", reportParameters, ds);
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            var sql = "SELECT * FROM acc_fix_asset fa INNER JOIN acc_fa_product p " +
                    "ON fa.fa_product_id = p.id INNER JOIN acc_depreciation_method dm " +
                    "ON fa.`depreciation_method_id`= dm.id " +
                    "where fa.branch_id=" + ddBranchName.SelectedItem.Value + " ;";
            GenerateReport(db.getDataTable(sql));
        }
    }
}