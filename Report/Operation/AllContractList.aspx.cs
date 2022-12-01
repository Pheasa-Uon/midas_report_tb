using Microsoft.Reporting.WebForms;
using Report.Utils;
using System;
using System.Data;

namespace Report.Operation
{
    public partial class AllContractList1 : System.Web.UI.Page
    {
        private DBConnect db = new DBConnect();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DataHelper.checkLoginSession();
                DataHelper.populateBranchDDL(ddBranchName, DataHelper.getUserId());
                populateOfficer();
            }
        }

        protected void ddBranchName_SelectedIndexChanged(object sender, EventArgs e)
        {
            populateOfficer();
        }

        private void populateOfficer()
        {
            if (ddBranchName.SelectedItem.Value != "")
            {
                if (ddBranchName.SelectedItem.Value == "ALL")
                {
                    ddOfficer.Enabled = true;
                    DataHelper.populateOfficerDDLAll(ddOfficer);
                } else
                {
                    ddOfficer.Enabled = true;
                    DataHelper.populateOfficerDDL(ddOfficer, Convert.ToInt32(ddBranchName.SelectedItem.Value));
                }
               
            }
            else
            {
                ddOfficer.Enabled = false;
                ddOfficer.Items.Clear();
            }
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            var sql = "";
            if (ddBranchName.SelectedItem.Value == "ALL")
            {
                sql = "SELECT CT.id,CUS.`customer_no`,CUS.`customer_name`,CUS.`customer_name_kh`,CUS.`dob`,CUS.`identify`, " +
                "CUS.`personal_phone`,CUS.`address`,CUS.`remark`,BN.`branch_name`,PT.lob_type,PD.lob_name,IDT.identify_type, " +
                "SX.sex,OCP.occupation,CUS.`home_street`, CT.disbursement_date,CT.pawn_price_approved,CT.interest_rate, " +
                "OUS.principle_less as Outstanding, CT.num_of_install,CT.expired_date,PV.province,DT.district,CC.commune, " +
                "VG.village,SI.`name`,CUR.currency_code,CT.ticket_type, CT.contract_type,CT.contract_no,CT.come_through, " +
                "CT.`cycle_num`,CT1.`disbursement_date` AS first_disbursement_date, CT.`contract_status` " +
                "FROM contract CT LEFT JOIN(SELECT contract_id, SUM(principle_less) principle_less FROM schedule_ticket " +
                "WHERE ticket_status != 'P' GROUP BY contract_id) OUS " +
                "ON CT.id = OUS.contract_id " +
                "LEFT JOIN customer CUS ON CT.customer_id = CUS.id " +
                "LEFT JOIN product PD ON CT.product_id = PD.id " +
                "LEFT JOIN product_type PT ON CT.product_type_id = PT.id " +
                "LEFT JOIN staff_info SI ON CT.pawn_officer_id = SI.id " +
                "LEFT JOIN province PV ON CUS.province_id = PV.id " +
                "LEFT JOIN district DT ON CUS.district_id = DT.id " +
                "LEFT JOIN commune CC ON CUS.commune_id = CC.id " +
                "LEFT JOIN village VG ON CUS.village_id = VG.id " +
                "LEFT JOIN currency CUR ON CT.currency_id = CUR.id " +
                "LEFT JOIN contract CT1 ON CT.`main_parent_id` = CT1.id " +
                "LEFT JOIN identify_type IDT ON CUS.`identify_type_id` = IDT.`id` " +
                "LEFT JOIN sex SX ON CUS.`sex_id` = SX.`id` " +
                "LEFT JOIN occupation OCP ON CUS.`occupation_id`= OCP.`id` " +
                "LEFT JOIN branch BN ON CT.`branch_id` = BN.`id` " +
                "WHERE CT.`b_status`= 1 ";
            }
            else
            {
                sql = "SELECT CT.id,CUS.`customer_no`,CUS.`customer_name`,CUS.`customer_name_kh`,CUS.`dob`,CUS.`identify`, " +
                "CUS.`personal_phone`,CUS.`address`,CUS.`remark`,BN.`branch_name`,PT.lob_type,PD.lob_name,IDT.identify_type, " +
                "SX.sex,OCP.occupation,CUS.`home_street`, CT.disbursement_date,CT.pawn_price_approved,CT.interest_rate, " +
                "OUS.principle_less as Outstanding, CT.num_of_install,CT.expired_date,PV.province,DT.district,CC.commune, " +
                "VG.village,SI.`name`,CUR.currency_code,CT.ticket_type, CT.contract_type,CT.contract_no,CT.come_through, " +
                "CT.`cycle_num`,CT1.`disbursement_date` AS first_disbursement_date, CT.`contract_status` " +
                "FROM contract CT LEFT JOIN(SELECT contract_id, SUM(principle_less) principle_less FROM schedule_ticket " +
                "WHERE ticket_status != 'P' AND branch_id = " + ddBranchName.SelectedItem.Value + " GROUP BY contract_id) OUS " +
                "ON CT.id = OUS.contract_id " +
                "LEFT JOIN customer CUS ON CT.customer_id = CUS.id " +
                "LEFT JOIN product PD ON CT.product_id = PD.id " +
                "LEFT JOIN product_type PT ON CT.product_type_id = PT.id " +
                "LEFT JOIN staff_info SI ON CT.pawn_officer_id = SI.id " +
                "LEFT JOIN province PV ON CUS.province_id = PV.id " +
                "LEFT JOIN district DT ON CUS.district_id = DT.id " +
                "LEFT JOIN commune CC ON CUS.commune_id = CC.id " +
                "LEFT JOIN village VG ON CUS.village_id = VG.id " +
                "LEFT JOIN currency CUR ON CT.currency_id = CUR.id " +
                "LEFT JOIN contract CT1 ON CT.`main_parent_id` = CT1.id " +
                "LEFT JOIN identify_type IDT ON CUS.`identify_type_id` = IDT.`id` " +
                "LEFT JOIN sex SX ON CUS.`sex_id` = SX.`id` " +
                "LEFT JOIN occupation OCP ON CUS.`occupation_id`= OCP.`id` " +
                "LEFT JOIN branch BN ON CT.`branch_id` = BN.`id` " +
                "WHERE CT.`b_status`= 1 AND CT.branch_id = " + ddBranchName.SelectedItem.Value;
            }

            if (ddContractStatus.SelectedItem.Value == "0")
            {
                sql += " AND CT.contract_status IN(4,7,8,6,9,10) ";
            }
            else
            {
                sql += " AND CT.contract_status = " + ddContractStatus.SelectedItem.Value;
            }

            if (ddOfficer.SelectedItem.Value != "0")
            {
                sql += " AND CT.pawn_officer_id = " + ddOfficer.SelectedItem.Value;
            }
            DataTable dt = db.getDataTable(sql + ";");
            GenerateReport(dt);
        }

        private void GenerateReport(DataTable dt)
        {
            var reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("Branch", ddBranchName.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("Officer", ddOfficer.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("SystemDate", DataHelper.getSystemDateStr()));
            reportParameters.Add(new ReportParameter("ContractStatus", ddContractStatus.SelectedItem.Text));

            var ds = new ReportDataSource("AllContractList", dt);
            DataHelper.generateOperationReport(ReportViewer1, "AllContractList", reportParameters, ds);
        }
    }
}