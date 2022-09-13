using Microsoft.Reporting.WebForms;
using System;
using System.Data;
using Report.Utils;

namespace Report.Operation
{
    public partial class StockReport : System.Web.UI.Page
    {
        private DBConnect db = new DBConnect();
        public static string systemDateStr;
        public string format = "dd/MM/yyyy";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DataHelper.checkLoginSession();
                DataHelper.populateBranchDDL(ddBranchName, DataHelper.getUserId());
                populateOfficer();
                systemDateStr = DataHelper.getSystemDateStr();
            }
        }

        private void GenerateReport(DataTable stockreportDT)
        {
            var reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("Branch", ddBranchName.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("PawnOfficer", ddOfficer.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("SystemDate", DataHelper.getSystemDate().ToString("dd-MMM-yyyy")));
            var _stockreport = new ReportDataSource("StockDS", stockreportDT);
            DataHelper.generateOperationReport(ReportViewer1, "StockReport", reportParameters, _stockreport);
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            var dateFormat = DataHelper.getSystemDate().ToString("yyyy-MM-dd");

            var sql = "SELECT  ST.id, ST.ticket_no, ST1.ticket_no as pre_ticket_no, ST.ticket_type,ST.serial_number,ST1.serial_number pre_serial_number, ST.contract_no, CT.customer_name, CLT.coll_type_name " +
                " , CLC.coll_cate_code,CUR.currency_code, MKK.mark_name,ST.created_date,ST.due_date, CL.seal_no " +
                " , CL.year_of_model, CL.vh_plate_no, CL.vh_cylinder, CL.vh_frame, CL.vh_engine_no, CRT.cert_name " +
                " , CL.pr_issue_date, CL.pr_square_side, CL.pr_north_by, CL.pr_south_by, CL.pr_west_by, CL.pr_east_by " +
                " ,CL.jr_weight, CL.jr_quality, c.market_price, ct.address, c.hot_price,cl.vh_first_issue_date,C.disbursement_date " +
                " ,case when CL.is_ownership = 1 then 'Yes' else 'No' end is_ownership, cl.certificate_check  " +
                " , CL.remark,CL.el_model,C.interest_rate,OUS.principle_less AS outstanding,CL.vh_mark,CL.vh_type,CL.vh_color, " +
                " CASE WHEN DATE(CASE WHEN C.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END) < " +
                " DATE('" + dateFormat + "') THEN 'Late' ELSE 'Active' END AS 'status',SI.`name` pawn_officer,PD.lob_name " +
                " FROM  schedule_ticket ST RIGHT JOIN " +
                " (SELECT MIN(id) AS id " +
                " FROM  schedule_ticket " +
                " WHERE (`ticket_status` = 'A' OR " +
                " `ticket_status` = 'PAP' OR " +
                " `ticket_status` = 'PPP' OR " +
                " `ticket_status` = 'FPP') AND branch_id = " + ddBranchName.SelectedItem.Value +
                " GROUP BY contract_id " +
                " ORDER BY contract_id) A ON ST.id = A.id " +
                "     LEFT JOIN customer CT ON ST.customer_id = CT.id " +
                "     LEFT JOIN contract C ON ST.contract_id = C.id " +
                "     LEFT JOIN currency CUR ON C.currency_id = CUR.id " +
                "     LEFT JOIN(SELECT* FROM collateral WHERE b_status=1 GROUP BY contract_id) CL ON C.id = CL.contract_id " +
                "     LEFT JOIN collateral_category CLC ON CL.collateral_category_id = CLC.id " +
                "     LEFT JOIN collateral_type CLT ON CL.collateral_type_id = CLT.id " +
                "     LEFT JOIN mark MKK ON CL.mark_id = MKK.id " +
                "     LEFT JOIN color COL ON CL.color_id = COL.id " +
                "     LEFT JOIN certificate_type CRT ON CL.pr_certificate_type_id = CRT.id " +
                "     LEFT JOIN staff_info SI ON C.pawn_officer_id = SI.id " +
                "     LEFT JOIN collateral_history PL ON CL.id = PL.collateral_id AND PL.is_active = TRUE " +
                "     LEFT JOIN product PD ON PD.id = C.product_id " +
                " LEFT JOIN schedule_ticket ST1 ON ST.contract_id = ST1.contract_id AND ST.id - 1 = ST1.id " +
                " LEFT JOIN(SELECT contract_id, SUM(principle_less) principle_less FROM schedule_ticket WHERE ticket_status != 'P' AND branch_id = "+ddBranchName.SelectedItem.Value+" GROUP BY contract_id) OUS ON ST.contract_id = OUS.contract_id " +
                " WHERE C.contract_status IN(4,7) ";
            if (ddOfficer.SelectedItem.Value != "0")
            {
                sql += " AND C.pawn_officer_id = " + ddOfficer.SelectedItem.Value;
            }
            sql += " ORDER BY ST.serial_number, ST1.serial_number, CT.customer_name;";

            var stockreportDT = db.getDataTable(sql);
            GenerateReport(stockreportDT);
        }

        protected void ddBranchName_SelectedIndexChanged(object sender, EventArgs e)
        {
            populateOfficer();
        }
        private void populateOfficer()
        {
            if (ddBranchName.SelectedValue != "")
            {
                ddOfficer.Enabled = true;
                DataHelper.populateOfficerDDL(ddOfficer, Convert.ToInt32(ddBranchName.SelectedValue));
            }
            else
            {
                ddOfficer.Enabled = false;
                ddOfficer.Items.Clear();
            }
        }
    }
}
