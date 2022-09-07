using Microsoft.Reporting.WebForms;
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
    public partial class Customer : System.Web.UI.Page
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

        protected void btnView_Click(object sender, EventArgs e)
        {
            var sql = "SELECT CUS.id, customer_name as name, customer_name_kh as name_kh, SX.`sex` as gender, " +
                           "CUS.dob " +
                           "FROM customer CUS LEFT JOIN identify_type IDT ON CUS.`identify_type_id` = IDT.`id` " +
                           "LEFT JOIN sex SX ON CUS.`sex_id` = SX.`id` " +
                           "LEFT JOIN occupation OCP ON CUS.`occupation_id` = OCP.`id` " +
                           "WHERE CUS.`b_status`= TRUE AND branch_id = " + ddBranchName.SelectedItem.Value + ";";

            DataTable dt = db.getDataTable(sql);

            DataTable dt1 = db.getDataTable(sql);
            GenerateReport(dt, dt1);
        }

        private void GenerateReport(DataTable dt, DataTable dt1)
        {
            var reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("BranchName", ddBranchName.SelectedItem.Text));

            var ds = new ReportDataSource("CustomerDS", dt);
            var ds1 = new ReportDataSource("CustomerHistoryDS", dt1);
            DataHelper.generateOperationReport(ReportViewer1, "Customer", reportParameters, ds, ds1);
        }
    }
}