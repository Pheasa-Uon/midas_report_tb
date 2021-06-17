
using Microsoft.Reporting.WebForms;
using System;
using System.Data;
using Report.Utils;

namespace Report.Operation
{
    public partial class PawnlistDetail : System.Web.UI.Page
    {
        private DBConnect db = new DBConnect();
        public static string systemDateStr;
        public string format = "dd/MM/yyyy";
        protected void Page_Load(object sender, EventArgs e)
        {
            DataHelper.checkLoginSession();
            systemDateStr = DataHelper.getSystemDateStr();
            if (!IsPostBack)
            {
                DataHelper.populateBranchDDL(ddBranchName, DataHelper.getUserId());
                systemDateStr = DataHelper.getSystemDateStr();
            }
        }
        
        private void GenerateReport(DataTable pawnlistDT)
        {
            ReportParameterCollection reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("BranchParameter", ddBranchName.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("OfficerParameter", ddOfficer.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("SystemDateParameter", DateTime.ParseExact(systemDateStr, format, null).ToString("dd-MMM-yyyy")));

            var _pawnlist = new ReportDataSource("PawnListDetail", pawnlistDT);
            DataHelper.generateOperationReport(ReportViewer1, "PawnListDetail", reportParameters, _pawnlist);
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            var pawnlistSql = "SELECT CT.id,CUS.customer_no,CUS.customer_name,PT.lob_type,PD.lob_name," +
                            " CT.disbursement_date,CT.pawn_price_approved,CT.interest_rate," +
                            " OUS.principle_less as Outstanding,"+
                            " CT.num_of_install,CT.expired_date,PV.province,DT.district,CC.commune,VG.village,CUS.personal_phone,SI.name,CUR.currency_code,CT.ticket_type" +
                            " FROM contract CT" +
                            " LEFT JOIN(SELECT contract_id, SUM(principle_less) principle_less FROM schedule_ticket  WHERE ticket_status != 'P' AND branch_id = "+ ddBranchName.SelectedValue + 
                            " GROUP BY contract_id) OUS ON CT.id = OUS.contract_id" +
                            " LEFT JOIN customer CUS ON CT.customer_id = CUS.id" +
                            " LEFT JOIN product PD ON CT.product_id = PD.id" +
                            " LEFT JOIN product_type PT ON CT.product_type_id = PT.id" +
                            " LEFT JOIN staff_info SI ON CT.pawn_officer_id = SI.id" +
                            " LEFT JOIN province PV ON CUS.province_id = PV.id" +
                            " LEFT JOIN district DT ON CUS.district_id = DT.id" +
                            " LEFT JOIN commune CC ON CUS.commune_id = CC.id" +
                            " LEFT JOIN village VG ON CUS.village_id = VG.id" +
                            " LEFT JOIN currency CUR ON CT.currency_id = CUR.id" +
                            " WHERE contract_status = 4 AND CT.b_status = 1 AND CT.branch_id = " + ddBranchName.SelectedValue +
                            " AND CT.pawn_officer_id = CASE WHEN " + ddOfficer.SelectedValue +
                            " IS NULL THEN CT.pawn_officer_id ELSE " + ddOfficer.SelectedValue +" END";
            var pawnlistDT = db.getDataTable(pawnlistSql);
            GenerateReport(pawnlistDT);
        }

        protected void ddBranchName_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddBranchName.SelectedValue != "")
            {
                ddOfficer.Enabled = true;
                DataHelper.populateOfficerDDL(ddOfficer, Convert.ToInt32(ddBranchName.SelectedValue));
            }
            else
            {
                ddOfficer.Enabled = false;
                ddOfficer.SelectedItem.Text = "";
            }
        }
    }
}