using Microsoft.Reporting.WebForms;
using Report.Utils;
using System;
using System.Data;
using System.Web;

namespace Report.Operation
{
    public partial class CustomerList : System.Web.UI.Page
    {
        private DBConnect db = new DBConnect();
        string urlPath = HttpContext.Current.Request.Url.AbsoluteUri;
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
            var sql = "";
            if (ddBranchName.SelectedItem.Value == "ALL")
            {
                sql = "SELECT CUS.customer_no, customer_name, customer_name_kh, SX.`sex`, " +
                "CUS.dob, IDT.`identify_type`, CUS.`identify`, CUS.`personal_phone`, " +
                "OCP.`occupation`, CUS.`home_street`, CUS.`address`, CUS.`remark` " +
                "FROM customer CUS LEFT JOIN identify_type IDT ON CUS.`identify_type_id` = IDT.`id` " +
                "LEFT JOIN sex SX ON CUS.`sex_id` = SX.`id` " +
                "LEFT JOIN occupation OCP ON CUS.`occupation_id` = OCP.`id` " +
                "WHERE CUS.`b_status`= TRUE;";
            }
            else
            {
                sql = "SELECT CUS.customer_no, customer_name, customer_name_kh, SX.`sex`, " +
                "CUS.dob, IDT.`identify_type`, CUS.`identify`, CUS.`personal_phone`, " +
                "OCP.`occupation`, CUS.`home_street`, CUS.`address`, CUS.`remark` " +
                "FROM customer CUS LEFT JOIN identify_type IDT ON CUS.`identify_type_id` = IDT.`id` " +
                "LEFT JOIN sex SX ON CUS.`sex_id` = SX.`id` " +
                "LEFT JOIN occupation OCP ON CUS.`occupation_id` = OCP.`id` " +
                "WHERE CUS.`b_status`= TRUE AND branch_id = " + ddBranchName.SelectedItem.Value + ";";
            }
            DataTable dt = db.getDataTable(sql);
            GenerateReport(dt);
        }

        private void GenerateReport(DataTable dt)
        {
            var reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("Branch", ddBranchName.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("SystemDate", DataHelper.getSystemDateStr()));
           
            var ds = new ReportDataSource("CustomerList", dt);
            DataHelper.generateOperationReport(ReportViewer1, "CustomerList", reportParameters, ds);
        }
    }
}