using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
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
            DataHelper.checkLoginSession();
            if (!IsPostBack)
            {
                DataHelper.populateBranchDDL(ddBranchName, DataHelper.getUserId());
                systemDateStr = DataHelper.getSystemDateStr();
            }
        }

        //GenerateReport Function
        private void GenerateReport(DataTable stockreportDT)
        {
            var reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("BranchParameter", ddBranchName.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("OfficerParameter", ddOfficer.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("SystemDateParameter", DateTime.ParseExact(systemDateStr, format, null).ToString("dd-MMM-yyyy")));
            var _stockreport = new ReportDataSource("StockDS", stockreportDT);
            DataHelper.generateOperationReport(ReportViewer1, "StockReport", reportParameters, _stockreport);
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            //Split System Date Time variable
            var systemDateSql = DateTime.ParseExact(systemDateStr, format, null);
            var dateFormat = systemDateSql.ToString("yyyy-MM-dd");
            var day = systemDateSql.ToString("dd");

            var stockreportSql = "SELECT  ST.id, ST.ticket_no, ST.ticket_type,ST.serial_number, ST.contract_no, CT.customer_name, CLT.coll_type_name"+
                                 ", CLC.coll_cate_code,CUR.currency_code, MKK.mark_name,ST.created_date,ST.due_date, CL.seal_no"+
                                 ", CL.year_of_model, CL.vh_plate_no, CL.vh_cylinder, CL.vh_frame, CL.vh_engine_no, CRT.cert_name"+
                                 ", CL.pr_issue_date, CL.pr_square_side, CL.pr_north_by, CL.pr_south_by, CL.pr_west_by, CL.pr_east_by"+
                                 ",CL.jr_weight, CL.jr_quality"+
                                 ", CL.remark,CL.el_model,C.interest_rate,OUS.principle_less as outstanding,"+
                                 "CASE WHEN DATE (ST.due_date) <"+
                                 day + " THEN 'Late' ELSE 'Active' END AS 'status',SI.`name` pawn_officer,"+
                                 "PD.lob_name "+
                                 "FROM schedule_ticket ST RIGHT JOIN "+ 
                                             "(SELECT MIN(id) AS id "+
                                             "FROM  schedule_ticket "+
                                             "WHERE (`ticket_status` = 'A' OR"+
                                                     "`ticket_status` = 'PAP' OR"+
                                                     "`ticket_status` = 'PPP' OR"+
                                                     "`ticket_status` = 'FPP') AND branch_id = " + ddBranchName.SelectedValue +
                                            " GROUP BY contract_id "+
                                            "ORDER BY contract_id) A ON ST.id = A.id "+
                                 "LEFT JOIN customer CT ON ST.customer_id = CT.id "+
                                 "LEFT JOIN contract C ON ST.contract_id = C.id "+
                                 "LEFT JOIN currency CUR ON C.currency_id = CUR.id "+
                                 "LEFT JOIN (SELECT* FROM collateral group by contract_id) CL ON C.id = CL.contract_id "+
                                 "LEFT JOIN collateral_category CLC ON CL.collateral_category_id = CLC.id "+
                                 "LEFT JOIN collateral_type CLT ON CL.collateral_type_id = CLT.id "+
                                 "LEFT JOIN mark MKK ON CL.mark_id = MKK.id "+
                                 "LEFT JOIN color COL ON CL.color_id = COL.id "+
                                 "LEFT JOIN certificate_type CRT ON CL.pr_certificate_type_id = CRT.id "+
                                 "LEFT JOIN staff_info SI ON C.pawn_officer_id = SI.id "+
                                 "LEFT JOIN collateral_history PL ON CL.id = PL.collateral_id AND PL.is_active = true "+
                                 "LEFT JOIN product PD ON PD.id = C.product_id "+
                                 "LEFT JOIN (SELECT contract_id, SUM(principle_less) principle_less FROM schedule_ticket WHERE ticket_status != 'P' AND branch_id = " + ddBranchName.SelectedValue +" GROUP BY contract_id) OUS ON ST.contract_id = OUS.contract_id "+
                                 "WHERE C.pawn_officer_id = CASE WHEN " + ddOfficer.SelectedValue + " IS NULL THEN C.pawn_officer_id ELSE " + ddOfficer.SelectedValue +" END";
            
            var stockreportDT = db.getDataTable(stockreportSql);
            GenerateReport(stockreportDT);
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
