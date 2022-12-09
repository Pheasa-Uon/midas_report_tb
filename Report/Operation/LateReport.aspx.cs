using Microsoft.Reporting.WebForms;
using System;
using System.Data;
using Report.Utils;
using Report.Models;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace Report.Operation
{
    public partial class LateReport : System.Web.UI.Page
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
                }
                else
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

        private void GenerateReport(DataTable lateReportDT, DataTable summaryDT)
        {
            ReportParameterCollection reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("Branch", ddBranchName.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("PawnOfficer", ddOfficer.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("SystemDate", DataHelper.getSystemDateStr()));
            var lateDS = new ReportDataSource("LateReportDS", lateReportDT);
            var summaryDS = new ReportDataSource("SummaryDS", summaryDT);
            DataHelper.generateOperationReport(ReportViewer1, "LateReport", reportParameters, lateDS, summaryDS);
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            var dayFormat = DataHelper.getSystemDate().ToString("yyyy-MM-dd");

            var lateReportSql = "";
            if (ddBranchName.SelectedItem.Value == "ALL")
            {
                lateReportSql = "SELECT CUS.customer_name,P.lob_name,C.disbursement_date,OUS.principle_less,C.interest_rate,A.id,A.contract_id,A.contract_no,A.ticket_no,A.created_date, " +
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
                " (SELECT MIN(ST.id) id, ST.contract_id, ST.contract_no, ST.ticket_no, ST.created_date, ST.due_date, MAX(DATEDIFF(DATE('" + dayFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END))) AS aging_late, ST.penalty_less, " +
                "   IFNULL(SUM(CASE WHEN DATEDIFF(DATE('" + dayFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 1 AND 30 THEN interest_less END), 0) AS interest130, " +
                "   IFNULL(SUM(CASE WHEN DATEDIFF(DATE('" + dayFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 1 AND 30 THEN 1 END), 0) AS has_interest130, " +
                "   IFNULL(SUM(CASE WHEN DATEDIFF(DATE('" + dayFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 31 AND 60 THEN interest_less END), 0) AS interest3160, " +
                "   IFNULL(SUM(CASE WHEN DATEDIFF(DATE('" + dayFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 31 AND 60 THEN 1 END), 0) AS has_interest3160, " +
                "   IFNULL(SUM(CASE WHEN DATEDIFF(DATE('" + dayFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 61 AND 90 THEN interest_less END), 0) AS interest6190, " +
                "   IFNULL(SUM(CASE WHEN DATEDIFF(DATE('" + dayFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 61 AND 90 THEN 1 END), 0) AS has_interest6190, " +
                "   IFNULL(SUM(CASE WHEN DATEDIFF(DATE('" + dayFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) >= 91 THEN interest_less END), 0) AS interest91, " +
                "   IFNULL(SUM(CASE WHEN DATEDIFF(DATE('" + dayFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) >= 91 THEN 1 END), 0) AS has_interest91 " +
                " FROM schedule_ticket ST LEFT JOIN contract IC ON ST.contract_id = IC.id WHERE ST.`ticket_status` != 'P' AND ST.`ticket_status` != 'FPP' " +
                "   AND DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END) < DATE('" + dayFormat + "') GROUP BY ST.contract_id ORDER BY ST.contract_id) A " +
                " LEFT JOIN contract C ON A.contract_id = C.id " +
                " LEFT JOIN customer CUS ON C.customer_id = CUS.id " +
                " LEFT JOIN currency CUR ON C.currency_id = CUR.id " +
                " LEFT JOIN(SELECT id, contract_id, collateral_type_id FROM collateral GROUP BY contract_id) COL ON C.id = COL.contract_id " +
                " LEFT JOIN product P ON C.product_id = P.id " +
                " LEFT JOIN(SELECT contract_id, SUM(principle_less) principle_less FROM schedule_ticket WHERE ticket_status != 'P'  GROUP BY contract_id) OUS ON OUS.contract_id = C.id " +
                " LEFT JOIN(SELECT SDT.contract_id, MAX(SDT.Serial_number) Serial_number FROM schedule_ticket SDT LEFT JOIN contract CC ON SDT.contract_id = CC.id " +
                " WHERE CC.contract_status >= 4 AND CC.b_status = TRUE " +
                " GROUP BY SDT.contract_id) SR ON A.contract_id = SR.contract_id " +
                " WHERE C.`b_status`= 1 AND C.contract_status IN(4,7) ";
            }
            else
            {
                lateReportSql = "SELECT CUS.customer_name,P.lob_name,C.disbursement_date,OUS.principle_less,C.interest_rate,A.id,A.contract_id,A.contract_no,A.ticket_no,A.created_date, " +
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
                " (SELECT MIN(ST.id) id, ST.contract_id, ST.contract_no, ST.ticket_no, ST.created_date, ST.due_date, MAX(DATEDIFF(DATE('" + dayFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END))) AS aging_late, ST.penalty_less, " +
                "   IFNULL(SUM(CASE WHEN DATEDIFF(DATE('" + dayFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 1 AND 30 THEN interest_less END), 0) AS interest130, " +
                "   IFNULL(SUM(CASE WHEN DATEDIFF(DATE('" + dayFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 1 AND 30 THEN 1 END), 0) AS has_interest130, " +
                "   IFNULL(SUM(CASE WHEN DATEDIFF(DATE('" + dayFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 31 AND 60 THEN interest_less END), 0) AS interest3160, " +
                "   IFNULL(SUM(CASE WHEN DATEDIFF(DATE('" + dayFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 31 AND 60 THEN 1 END), 0) AS has_interest3160, " +
                "   IFNULL(SUM(CASE WHEN DATEDIFF(DATE('" + dayFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 61 AND 90 THEN interest_less END), 0) AS interest6190, " +
                "   IFNULL(SUM(CASE WHEN DATEDIFF(DATE('" + dayFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 61 AND 90 THEN 1 END), 0) AS has_interest6190, " +
                "   IFNULL(SUM(CASE WHEN DATEDIFF(DATE('" + dayFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) >= 91 THEN interest_less END), 0) AS interest91, " +
                "   IFNULL(SUM(CASE WHEN DATEDIFF(DATE('" + dayFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) >= 91 THEN 1 END), 0) AS has_interest91 " +
                " FROM schedule_ticket ST LEFT JOIN contract IC ON ST.contract_id = IC.id WHERE ST.`ticket_status` != 'P' AND ST.`ticket_status` != 'FPP' AND ST.branch_id = " + ddBranchName.SelectedItem.Value + " " +
                "   AND DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END) < DATE('" + dayFormat + "') GROUP BY ST.contract_id ORDER BY ST.contract_id) A " +
                " LEFT JOIN contract C ON A.contract_id = C.id " +
                " LEFT JOIN customer CUS ON C.customer_id = CUS.id " +
                " LEFT JOIN currency CUR ON C.currency_id = CUR.id " +
                " LEFT JOIN(SELECT id, contract_id, collateral_type_id FROM collateral GROUP BY contract_id) COL ON C.id = COL.contract_id " +
                " LEFT JOIN product P ON C.product_id = P.id " +
                " LEFT JOIN(SELECT contract_id, SUM(principle_less) principle_less FROM schedule_ticket WHERE ticket_status != 'P' AND branch_id = " + ddBranchName.SelectedItem.Value + " GROUP BY contract_id) OUS ON OUS.contract_id = C.id " +
                " LEFT JOIN(SELECT SDT.contract_id, MAX(SDT.Serial_number) Serial_number FROM schedule_ticket SDT LEFT JOIN contract CC ON SDT.contract_id = CC.id " +
                " WHERE CC.contract_status >= 4 AND CC.b_status = TRUE AND SDT.branch_id = " + ddBranchName.SelectedItem.Value +
                " GROUP BY SDT.contract_id) SR ON A.contract_id = SR.contract_id " +
                " WHERE C.`b_status`= 1 AND C.contract_status IN(4,7) ";
            }
            if (ddOfficer.SelectedItem.Value != "0")
            {
                lateReportSql += " AND C.pawn_officer_id = " + ddOfficer.SelectedItem.Value;
            }

            lateReportSql += " ORDER BY P.lob_name,CUR.currency_code,C.contract_no;";

            var spd = "PS_BRANCH_PROD_TEST";
            List<Procedure> parameters = new List<Procedure>();
            parameters.Add(item: new Procedure() { field_name = "@pBranch", sql_db_type = MySqlDbType.VarChar, value_name = ddBranchName.SelectedItem.Value });
            parameters.Add(item: new Procedure() { field_name = "@pSystem_date", sql_db_type = MySqlDbType.VarChar, value_name = dayFormat });
            parameters.Add(item: new Procedure() { field_name = "@pCurrency", sql_db_type = MySqlDbType.VarChar, value_name = "2" });

            DataTable summaryDT = db.getProcedureDataTable(spd, parameters);
            DataTable lateReportDT = db.getDataTable(lateReportSql);
            GenerateReport(lateReportDT, summaryDT);
        }
    }
}