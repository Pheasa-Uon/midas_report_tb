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
    public partial class LateReportV21 : System.Web.UI.Page
    {
        private DBConnect db = new DBConnect();
        DateTime currentDate = DateTime.Today;
        public string format = "dd-MMM-yyyy";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DataHelper.populateBranchDDL(ddBranchName, DataHelper.getUserId());
            }
        }

        private void GenerateReport(DataTable dt, DataTable summaryDT)
        {
            ReportParameterCollection reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("Branch", ddBranchName.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("PawnOfficer", ddOfficer.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("SystemDate", DataHelper.getSystemDateStr()));

            var ds = new ReportDataSource("LateReportDS", dt);
            var summaryDs = new ReportDataSource("SummaryDS", summaryDT);
            DataHelper.generateOperationReport(ReportViewer1, "LateReportV2", reportParameters, ds, summaryDs);
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            var systemDateSql = DateTime.ParseExact(DataHelper.getSystemDateStr(), format, null);
            var sysDate = systemDateSql.ToString("yyyy-MM-dd");

            var sql = "SELECT CUS.customer_name,P.lob_name,C.disbursement_date,OUS.principle_less,C.interest_rate,A.id,A.contract_id,A.contract_no,A.ticket_no,A.created_date, " +
                " A.due_date,A.aging_late,A.penalty_less, " +
                " CASE WHEN A.has_interest91 > 0 THEN A.interest130 + A.interest3160 + A.interest6190 + A.interest91 ELSE 0 END AS interest91, " +
                " CASE WHEN A.has_interest6190 > 0 AND A.has_interest91 <= 0 THEN A.interest130 + A.interest3160 + A.interest6190 ELSE 0 END AS interest6190, " +
                " CASE WHEN A.has_interest3160 > 0 AND A.has_interest91 <= 0 AND A.has_interest6190 <= 0 THEN A.interest130 + A.interest3160 ELSE 0 END AS interest3160, " +
                " CASE WHEN A.has_interest130 > 0 AND A.has_interest91 <= 0 AND A.has_interest6190 <= 0 AND A.has_interest3160 <= 0 THEN A.interest130 ELSE 0 END AS interest130, " +
                " CASE WHEN A.has_interest130 > 0 AND A.has_interest91 <= 0 AND A.has_interest6190 <= 0 AND A.has_interest3160 <= 0 THEN OUS.principle_less ELSE 0 END AS principle130, " +
                " CASE WHEN A.has_interest3160 > 0 AND A.has_interest91 <= 0 AND A.has_interest6190 <= 0 THEN OUS.principle_less ELSE 0 END AS principle3160, " +
                " CASE WHEN A.has_interest6190 > 0 AND A.has_interest91 <= 0 THEN OUS.principle_less ELSE 0 END AS principle6190, " +
                " CASE WHEN A.has_interest91 > 0 THEN OUS.principle_less ELSE 0 END AS principle91,CUR.currency_code,C.ticket_type,SR.Serial_number,C.contract_status " +
                " FROM " +
                " (SELECT MIN(ST.id) id, ST.contract_id, ST.contract_no, ST.ticket_no, ST.created_date, ST.due_date, MAX(DATEDIFF(DATE(?), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END))) AS aging_late, ST.penalty_less, " +
                "   IFNULL(SUM(CASE WHEN DATEDIFF(DATE(?), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 1 AND 30 THEN interest_less END), 0) AS interest130, " +
                "   IFNULL(SUM(CASE WHEN DATEDIFF(DATE(?), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 1 AND 30 THEN 1 END), 0) AS has_interest130, " +
                "   IFNULL(SUM(CASE WHEN DATEDIFF(DATE(?), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 31 AND 60 THEN interest_less END), 0) AS interest3160, " +
                "   IFNULL(SUM(CASE WHEN DATEDIFF(DATE(?), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 31 AND 60 THEN 1 END), 0) AS has_interest3160, " +
                "   IFNULL(SUM(CASE WHEN DATEDIFF(DATE(?), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 61 AND 90 THEN interest_less END), 0) AS interest6190, " +
                "   IFNULL(SUM(CASE WHEN DATEDIFF(DATE(?), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 61 AND 90 THEN 1 END), 0) AS has_interest6190, " +
                "   IFNULL(SUM(CASE WHEN DATEDIFF(DATE(?), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) >= 91 THEN interest_less END), 0) AS interest91, " +
                "   IFNULL(SUM(CASE WHEN DATEDIFF(DATE(?), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) >= 91 THEN 1 END), 0) AS has_interest91 " +
                " FROM schedule_ticket ST LEFT JOIN contract IC ON ST.contract_id = IC.id WHERE ST.`ticket_status` != 'P' AND ST.`ticket_status` != 'FPP' AND ST.branch_id = " + ddBranchName.SelectedItem.Value +
                "   AND DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END) < DATE(Select system_date from system_date where is_active=1 limit 1) GROUP BY ST.contract_id ORDER BY ST.contract_id) A " +
                " LEFT JOIN contract C ON A.contract_id = C.id " +
                " LEFT JOIN customer CUS ON C.customer_id = CUS.id " +
                " LEFT JOIN currency CUR ON C.currency_id = CUR.id " +
                " LEFT JOIN(SELECT id, contract_id, collateral_type_id FROM collateral GROUP BY contract_id) COL ON C.id = COL.contract_id " +
                " LEFT JOIN product P ON C.product_id = P.id " +
                " LEFT JOIN(SELECT contract_id, SUM(principle_less) principle_less FROM schedule_ticket WHERE ticket_status != 'P' AND branch_id = ? GROUP BY contract_id) OUS ON OUS.contract_id = C.id " +
                " LEFT JOIN(SELECT SDT.contract_id, MAX(SDT.Serial_number) Serial_number FROM schedule_ticket SDT LEFT JOIN contract CC ON SDT.contract_id = CC.id " +
                " WHERE CC.contract_status >= 4 AND CC.b_status = TRUE AND SDT.branch_id = " + ddBranchName.SelectedItem.Value +
                " GROUP BY SDT.contract_id) SR ON A.contract_id = SR.contract_id " +
                " WHERE C.`b_status`= 1 AND C.contract_status IN(4,7) ";
            if (ddOfficer.SelectedItem.Value != "0")
            {
                sql += " AND C.pawn_officer_id=" + ddOfficer.SelectedItem.Value;
            }

            sql += " ORDER BY P.lob_name,CUR.currency_code,C.contract_no;";

            var lateDS = db.getDataTable(sql);
            
            var spd = "PS_BRANCH_PROD_TEST";
            List<Procedure> parameters = new List<Procedure>();
            parameters.Add(item: new Procedure() { field_name = "@pBranch", sql_db_type = MySqlDbType.VarChar, value_name = ddBranchName.SelectedItem.Value });
            parameters.Add(item: new Procedure() { field_name = "@pSystem_date", sql_db_type = MySqlDbType.Date, value_name = DataHelper.getSystemDateStr() });
            parameters.Add(item: new Procedure() { field_name = "@pCurrency", sql_db_type = MySqlDbType.VarChar, value_name = "2" });
            DataTable summaryDS = db.getProcedureDataTable(spd, parameters);
           
            GenerateReport(lateDS, summaryDS);
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