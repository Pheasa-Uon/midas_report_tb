using Microsoft.Reporting.WebForms;
using Report.Models;
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
    public partial class LateReport : System.Web.UI.Page
    {
        private DBConnect db = new DBConnect();
        public static string systemDateStr;
        public string format = "dd-MMM-yyyy";
        protected void Page_Load(object sender, EventArgs e)
        {
            DataHelper.checkLoginSession();
            if (!IsPostBack)
            {
                DataHelper.populateBranchDDL(ddBranchName, DataHelper.getUserId());
                systemDateStr = DataHelper.getSystemDateStr();
            }
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

        //GenerateReport Function
        private void GenerateReport(DataTable lateReportDT)
        {
            ReportParameterCollection reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("BranchParameter", ddBranchName.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("OfficerParameter", ddOfficer.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("SystemDateParameter", DateTime.ParseExact(systemDateStr, format, null).ToString("dd-MMM-yyyy")));
            var _lateReport = new ReportDataSource("LateReportDS", lateReportDT);
            DataHelper.generateOperationReport(ReportViewer1, "LateReport", reportParameters, _lateReport);
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            //Split System Date Time variable
            var systemDateSql = DateTime.ParseExact(systemDateStr, format, null);
            var dayFormat = systemDateSql.ToString("yyyy-MM-dd");

            var lateReportSql = "SELECT CUS.customer_name,P.lob_name,C.disbursement_date,OUS.principle_less,C.interest_rate,A.id,A.contract_id,A.contract_no,A.created_date, " +
                                "A.due_date,A.aging_late, " +
                                "CASE WHEN A.has_interest91 > 0 THEN A.interest130 + A.interest3160 + A.interest6190 + A.interest91 ELSE 0 END AS interest91, " +
                                "CASE WHEN A.has_interest6190 > 0 AND A.has_interest91 <= 0 THEN A.interest130 + A.interest3160 + A.interest6190 ELSE 0 END AS interest6190, " +
                                "CASE WHEN A.has_interest3160 > 0 AND A.has_interest91 <= 0 AND A.has_interest6190 <= 0 THEN A.interest130 + A.interest3160 ELSE 0 END AS interest3160, " +
                                "CASE WHEN A.has_interest130 > 0 AND A.has_interest91 <= 0 AND A.has_interest6190 <= 0 AND A.has_interest3160 <= 0 THEN A.interest130 ELSE 0 END AS interest130, " +
                                "CASE WHEN A.has_interest130 > 0 AND A.has_interest91 <= 0 AND A.has_interest6190 <= 0 AND A.has_interest3160 <= 0 THEN OUS.principle_less ELSE 0 END AS principle130, " +
                                "CASE WHEN A.has_interest3160 > 0 AND A.has_interest91 <= 0 AND A.has_interest6190 <= 0 THEN OUS.principle_less ELSE 0 END AS principle3160, " +
                                "CASE WHEN A.has_interest6190 > 0 AND A.has_interest91 <= 0 THEN OUS.principle_less ELSE 0 END AS principle6190, " +
                                "CASE WHEN A.has_interest91 > 0 THEN OUS.principle_less ELSE 0 END AS principle91,CUR.currency_code " +
                                "FROM " +
                                "(SELECT MIN(ST.id) id, ST.contract_id, ST.contract_no, ST.created_date, ST.due_date, MAX(DATEDIFF('" + dayFormat + "', DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END))) AS aging_late, " +
                                  "IFNULL(SUM(CASE WHEN DATEDIFF('" + dayFormat + "', DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 1 AND 30 THEN interest_less END), 0) AS interest130, " +
                                  "IFNULL(SUM(CASE WHEN DATEDIFF('" + dayFormat + "', DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 1 AND 30 THEN 1 END), 0) AS has_interest130, " +
                                  "IFNULL(SUM(CASE WHEN DATEDIFF('" + dayFormat + "', DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 31 AND 60 THEN interest_less END), 0) AS interest3160, " +
                                  "IFNULL(SUM(CASE WHEN DATEDIFF('" + dayFormat + "', DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 31 AND 60 THEN 1 END), 0) AS has_interest3160, " +
                                  "IFNULL(SUM(CASE WHEN DATEDIFF('" + dayFormat + "', DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 61 AND 90 THEN interest_less END), 0) AS interest6190, " +
                                  "IFNULL(SUM(CASE WHEN DATEDIFF('" + dayFormat + "', DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 61 AND 90 THEN 1 END), 0) AS has_interest6190, " +
                                  "IFNULL(SUM(CASE WHEN DATEDIFF('" + dayFormat + "', DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) >= 91 THEN interest_less END), 0) AS interest91, " +
                                  "IFNULL(SUM(CASE WHEN DATEDIFF('" + dayFormat + "', DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) >= 91 THEN 1 END), 0) AS has_interest91 " +
                                "FROM schedule_ticket ST LEFT JOIN contract IC ON ST.contract_id = IC.id WHERE ST.`ticket_status` != 'P' AND ST.`ticket_status` != 'FPP' AND ST.branch_id = " + ddBranchName.SelectedValue +
                                  " AND DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END) < '" + dayFormat + "' GROUP BY ST.contract_id ORDER BY ST.contract_id) A " +
                                "LEFT JOIN contract C ON A.contract_id = C.id " +
                                "LEFT JOIN customer CUS ON C.customer_id = CUS.id " +
                                "LEFT JOIN currency CUR ON C.currency_id = CUR.id " +
                                "LEFT JOIN(SELECT id, contract_id, collateral_type_id FROM collateral GROUP BY contract_id) COL ON C.id = COL.contract_id " +
                                "LEFT JOIN product P ON C.product_id = P.id " +
                                "LEFT JOIN(SELECT contract_id, SUM(principle_less) principle_less FROM schedule_ticket WHERE ticket_status != 'P' AND branch_id = " + ddBranchName.SelectedValue + " GROUP BY contract_id) OUS ON OUS.contract_id = C.id " +
                                "WHERE C.`b_status`= 1 AND C.contract_status = 4 AND C.pawn_officer_id = CASE WHEN " + ddOfficer.SelectedValue + " IS NULL THEN C.pawn_officer_id ELSE " + ddOfficer.SelectedValue + " END " +
                                "ORDER BY P.lob_name,CUR.currency_code,C.contract_no";
            
            DataTable lateReportDT = db.getDataTable(lateReportSql);
            GenerateReport(lateReportDT);
        }
    }
}