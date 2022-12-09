using Microsoft.Reporting.WebForms;
using Report.Models;
using System;
using System.Collections.Generic;
using System.Data;
using Report.Utils;

namespace Report.Operation
{
    public partial class SummaryReport : System.Web.UI.Page
    {
        private DBConnect db = new DBConnect();
        static List<Currency> currencyList;
        public string format = "dd/MM/yyyy";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DataHelper.checkLoginSession();
                DataHelper.populateBranchDDL(ddBranchName, DataHelper.getUserId());
                currencyList = DataHelper.populateCurrencyDDL(ddCurrency);
            }
        }
        
        private void GenerateReport(DataTable summaryReportDT)
        {
            var reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("Branch", ddBranchName.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("Currency", ddCurrency.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("SystemDate", DataHelper.getSystemDate().ToString("dd-MMM-yyyy")));
            var _summaryReportlist = new ReportDataSource("SummaryDS", summaryReportDT);
            DataHelper.generateOperationReport(ReportViewer1, "SummaryReport", reportParameters, _summaryReportlist);
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            var systemDateSql = DataHelper.getSystemDate();
            var dateFormat = systemDateSql.ToString("yyyy-MM-dd");
            var day = systemDateSql.ToString("dd");
            var month = systemDateSql.ToString("MM");
            var year = systemDateSql.ToString("yyyy");

            var summaryReportSql = "";
            if (ddBranchName.SelectedItem.Value == "ALL")
            {
                summaryReportSql = "SELECT SIO.`name`,MM.* FROM (SELECT * FROM staff_info WHERE b_status = true ) SIO LEFT JOIN " +
                        " (SELECT SI.id siid, " +
                        " SUM(CASE WHEN MONTH(C.disbursement_date) = " + month + " AND YEAR(C.disbursement_date) = " + year + " THEN 1 ELSE 0 END) AS 'new_num_client', " +
                        " IFNULL(SUM(CASE WHEN MONTH(C.disbursement_date) = " + month + " AND YEAR(C.disbursement_date) = " + year + " THEN C.pawn_price_approved END), 0) AS 'new_amt', " +
                        " SUM(CASE WHEN MONTH(C.redeem_date) = " + month + " AND YEAR(C.redeem_date) = " + year + " THEN 1 ELSE 0 END) AS 'redeem_num_client', " +
                        " IFNULL(SUM(CASE WHEN MONTH(C.redeem_date) = " + month + " AND YEAR(C.redeem_date) = " + year + " THEN C.pawn_price_approved END), 0) AS 'redeem_amt', " +
                        " MAX(RN.renew_num) AS 'renew_client', " +
                        " MAX(RN.renew_amt) AS 'renew_amt', " +
                        " IFNULL(SUM(CASE WHEN C.contract_status IN(4, 7) THEN 1 ELSE 0 END), 0) AS 'current_num_client', " +
                        " IFNULL(SUM(CASE WHEN C.contract_status IN(4, 7) THEN OUS.principle_less END), 0) AS 'current_amt', " +
                        " (SUM(CASE WHEN A.id > 0 THEN OUS.principle_less ELSE 0 END)) AS late_principle_amt, IFNULL(COUNT(A.late_num_client), 0) AS late_num_client, " +
                        " SUM(CASE WHEN A.has_interest130 > 0 AND A.has_interest91 <= 0 AND A.has_interest6190 <= 0 AND A.has_interest3160 <= 0 THEN A.interest130 ELSE 0 END) AS interest130, " +
                        " SUM(CASE WHEN A.has_interest3160 > 0 AND A.has_interest91 <= 0 AND A.has_interest6190 <= 0 THEN A.interest130 + A.interest3160 ELSE 0 END) AS interest3160, " +
                        " SUM(CASE WHEN A.has_interest6190 > 0 AND A.has_interest91 <= 0 THEN A.interest130 + A.interest3160 + A.interest6190 ELSE 0 END) AS interest6190, " +
                        " SUM(CASE WHEN A.has_interest91 > 0 THEN A.interest130 + A.interest3160 + A.interest6190 + A.interest91 ELSE 0 END) AS interest91, " +
                        " SUM(A.principle130) AS principle130, " +
                        " SUM(A.principle3160) AS principle3160, " +
                        " SUM(A.principle6190) AS principle6190, " +
                        " SUM(A.principle91) AS principle91, " +
                        " CUR.currency " +
                        " FROM " +
                        " contract C LEFT JOIN " +
                        " (SELECT B.id, B.contract_id, B.contract_no, B.late_principle_amt, B.late_num_client, B.interest130, B.interest3160, B.interest6190, B.interest91, " +
                        " B.has_interest130, B.has_interest3160, B.has_interest6190, B.has_interest91, " +
                        " CASE WHEN B.principle91 > 0 THEN B.principle91 ELSE 0 END AS principle91, " +
                        " CASE WHEN B.principle6190 > 0 AND B.principle91 <= 0 THEN B.principle6190 ELSE 0 END AS principle6190, " +
                        " CASE WHEN B.principle3160 > 0 AND B.principle91 <= 0 AND B.principle6190 <= 0 THEN B.principle3160 ELSE 0 END AS principle3160, " +
                        " CASE WHEN B.principle130 > 0 AND B.principle91 <= 0 AND B.principle6190 <= 0 AND B.principle3160 <= 0 THEN B.principle130 ELSE 0 END AS principle130 " +
                        " FROM(SELECT MIN(ST.id) id, ST.contract_id, ST.contract_no, " +
                        " IFNULL(SUM(CASE WHEN DATEDIFF(DATE('" + dateFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) >= 1 THEN ST.principle_less END), 0) AS late_principle_amt, " +
                        " IFNULL(CASE WHEN DATEDIFF(DATE('" + dateFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) >= 1 THEN 1 END, 0) AS late_num_client, " +
                        " IFNULL(MAX(CASE WHEN DATEDIFF(DATE('" + dateFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 1 AND 30 THEN IOUS.principle_less END), 0) AS principle130, " +
                        " IFNULL(MAX(CASE WHEN DATEDIFF(DATE('" + dateFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 31 AND 60 THEN IOUS.principle_less END), 0) AS principle3160, " +
                        " IFNULL(MAX(CASE WHEN DATEDIFF(DATE('" + dateFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 61 AND 90 THEN IOUS.principle_less END), 0) AS principle6190, " +
                        " IFNULL(MAX(CASE WHEN DATEDIFF(DATE('" + dateFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) >= 91 THEN IOUS.principle_less END), 0) AS principle91, " +
                        " IFNULL(SUM(CASE WHEN DATEDIFF(DATE('" + dateFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 1 AND 30 THEN ST.interest_less END), 0) AS interest130, " +
                        " IFNULL(SUM(CASE WHEN DATEDIFF(DATE('" + dateFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 1 AND 30 THEN 1 END), 0) AS has_interest130, " +
                        " IFNULL(SUM(CASE WHEN DATEDIFF(DATE('" + dateFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 31 AND 60 THEN ST.interest_less END), 0) AS interest3160, " +
                        " IFNULL(SUM(CASE WHEN DATEDIFF(DATE('" + dateFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 31 AND 60 THEN 1 END), 0) AS has_interest3160, " +
                        " IFNULL(SUM(CASE WHEN DATEDIFF(DATE('" + dateFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 61 AND 90 THEN ST.interest_less END), 0) AS interest6190, " +
                        " IFNULL(SUM(CASE WHEN DATEDIFF(DATE('" + dateFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 61 AND 90 THEN 1 END), 0) AS has_interest6190, " +
                        " IFNULL(SUM(CASE WHEN DATEDIFF(DATE('" + dateFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) >= 91 THEN ST.interest_less END), 0) AS interest91, " +
                        " IFNULL(SUM(CASE WHEN DATEDIFF(DATE('" + dateFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) >= 91 THEN 1 END), 0) AS has_interest91 " +
                        " FROM schedule_ticket ST LEFT JOIN contract IC ON ST.contract_id = IC.id " +
                        " LEFT JOIN(SELECT contract_id, SUM(principle_less) principle_less FROM schedule_ticket WHERE ticket_status != 'P'  GROUP BY contract_id) IOUS ON ST.contract_id = IOUS.contract_id " +
                        " WHERE ST.`ticket_status` != 'P' AND ST.`ticket_status` != 'FPP' AND  IC.currency_id = " + ddCurrency.SelectedItem.Value +
                        " AND DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END) < DATE('" + dateFormat + "') GROUP BY ST.contract_id " +
                        " ORDER BY ST.contract_id) B " +
                        " ) A ON A.contract_id = C.id " +
                        " LEFT JOIN(SELECT contract_id, SUM(principle_less) principle_less FROM schedule_ticket WHERE ticket_status != 'P'  GROUP BY contract_id) OUS ON C.id = OUS.contract_id " +
                        " LEFT JOIN currency CUR ON C.currency_id = CUR.id " +
                        " LEFT JOIN staff_info SI ON C.pawn_officer_id = SI.id " +
                        " LEFT JOIN( " +
                        "     SELECT ISI.id, COUNT(IRN.siid) renew_num, SUM(IRN.principle_due) renew_amt FROM " +
                        "         (SELECT I.id siid, ST.principle_due FROM schedule_ticket ST LEFT JOIN contract IC ON ST.contract_id = IC.id " +
                        "         LEFT JOIN staff_info I ON IC.pawn_officer_id = I.id " +
                        "         WHERE(ST.`ticket_status` = 'P' OR ST.`ticket_status` = 'FPP') " +
                        "         AND CASE WHEN ST.`ticket_status` = 'P' THEN MONTH(ST.paid_date) = " + month + " AND YEAR(ST.paid_date) = " + year +
                        "         WHEN ST.`ticket_status` = 'FPP' THEN MONTH(ST.prepaid_date) = " + month + " AND YEAR(ST.prepaid_date) = " + year + " END " +
                        "         AND CASE WHEN DATE(IC.disbursement_date) = DATE(CASE WHEN ST.`ticket_status` = 'P' THEN ST.paid_date WHEN ST.`ticket_status` = 'FPP' THEN ST.prepaid_date END) THEN ST.order_no > 1 ELSE TRUE END " +
                        "         AND ST.last_schedule = FALSE AND IC.contract_status IN(4, 7) AND IC.`b_status`= 1 " +
                        "         AND IC.currency_id = " + ddCurrency.SelectedItem.Value +
                        "         ) IRN LEFT JOIN staff_info ISI ON IRN.siid = ISI.id " +
                        "     GROUP BY ISI.id) RN ON SI.id = RN.id " +
                        " WHERE C.currency_id =" + ddCurrency.SelectedItem.Value + " AND C.`b_status`= 1 AND C.contract_status >= 4 " +
                        " GROUP BY SI.`name`) MM ON SIO.id = MM.siid";
            }
            else
            {
                summaryReportSql = "SELECT SIO.`name`,MM.* FROM (SELECT * FROM staff_info WHERE b_status = true AND branch_id = " + ddBranchName.SelectedItem.Value + ") SIO LEFT JOIN " +
                        " (SELECT SI.id siid, " +
                        " SUM(CASE WHEN MONTH(C.disbursement_date) = " + month + " AND YEAR(C.disbursement_date) = " + year + " THEN 1 ELSE 0 END) AS 'new_num_client', " +
                        " IFNULL(SUM(CASE WHEN MONTH(C.disbursement_date) = " + month + " AND YEAR(C.disbursement_date) = " + year + " THEN C.pawn_price_approved END), 0) AS 'new_amt', " +
                        " SUM(CASE WHEN MONTH(C.redeem_date) = " + month + " AND YEAR(C.redeem_date) = " + year + " THEN 1 ELSE 0 END) AS 'redeem_num_client', " +
                        " IFNULL(SUM(CASE WHEN MONTH(C.redeem_date) = " + month + " AND YEAR(C.redeem_date) = " + year + " THEN C.pawn_price_approved END), 0) AS 'redeem_amt', " +
                        " MAX(RN.renew_num) AS 'renew_client', " +
                        " MAX(RN.renew_amt) AS 'renew_amt', " +
                        " IFNULL(SUM(CASE WHEN C.contract_status IN(4, 7) THEN 1 ELSE 0 END), 0) AS 'current_num_client', " +
                        " IFNULL(SUM(CASE WHEN C.contract_status IN(4, 7) THEN OUS.principle_less END), 0) AS 'current_amt', " +
                        " (SUM(CASE WHEN A.id > 0 THEN OUS.principle_less ELSE 0 END)) AS late_principle_amt, IFNULL(COUNT(A.late_num_client), 0) AS late_num_client, " +
                        " SUM(CASE WHEN A.has_interest130 > 0 AND A.has_interest91 <= 0 AND A.has_interest6190 <= 0 AND A.has_interest3160 <= 0 THEN A.interest130 ELSE 0 END) AS interest130, " +
                        " SUM(CASE WHEN A.has_interest3160 > 0 AND A.has_interest91 <= 0 AND A.has_interest6190 <= 0 THEN A.interest130 + A.interest3160 ELSE 0 END) AS interest3160, " +
                        " SUM(CASE WHEN A.has_interest6190 > 0 AND A.has_interest91 <= 0 THEN A.interest130 + A.interest3160 + A.interest6190 ELSE 0 END) AS interest6190, " +
                        " SUM(CASE WHEN A.has_interest91 > 0 THEN A.interest130 + A.interest3160 + A.interest6190 + A.interest91 ELSE 0 END) AS interest91, " +
                        " SUM(A.principle130) AS principle130, " +
                        " SUM(A.principle3160) AS principle3160, " +
                        " SUM(A.principle6190) AS principle6190, " +
                        " SUM(A.principle91) AS principle91, " +
                        " CUR.currency " +
                        " FROM " +
                        " contract C LEFT JOIN " +
                        " (SELECT B.id, B.contract_id, B.contract_no, B.late_principle_amt, B.late_num_client, B.interest130, B.interest3160, B.interest6190, B.interest91, " +
                        " B.has_interest130, B.has_interest3160, B.has_interest6190, B.has_interest91, " +
                        " CASE WHEN B.principle91 > 0 THEN B.principle91 ELSE 0 END AS principle91, " +
                        " CASE WHEN B.principle6190 > 0 AND B.principle91 <= 0 THEN B.principle6190 ELSE 0 END AS principle6190, " +
                        " CASE WHEN B.principle3160 > 0 AND B.principle91 <= 0 AND B.principle6190 <= 0 THEN B.principle3160 ELSE 0 END AS principle3160, " +
                        " CASE WHEN B.principle130 > 0 AND B.principle91 <= 0 AND B.principle6190 <= 0 AND B.principle3160 <= 0 THEN B.principle130 ELSE 0 END AS principle130 " +
                        " FROM(SELECT MIN(ST.id) id, ST.contract_id, ST.contract_no, " +
                        " IFNULL(SUM(CASE WHEN DATEDIFF(DATE('" + dateFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) >= 1 THEN ST.principle_less END), 0) AS late_principle_amt, " +
                        " IFNULL(CASE WHEN DATEDIFF(DATE('" + dateFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) >= 1 THEN 1 END, 0) AS late_num_client, " +
                        " IFNULL(MAX(CASE WHEN DATEDIFF(DATE('" + dateFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 1 AND 30 THEN IOUS.principle_less END), 0) AS principle130, " +
                        " IFNULL(MAX(CASE WHEN DATEDIFF(DATE('" + dateFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 31 AND 60 THEN IOUS.principle_less END), 0) AS principle3160, " +
                        " IFNULL(MAX(CASE WHEN DATEDIFF(DATE('" + dateFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 61 AND 90 THEN IOUS.principle_less END), 0) AS principle6190, " +
                        " IFNULL(MAX(CASE WHEN DATEDIFF(DATE('" + dateFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) >= 91 THEN IOUS.principle_less END), 0) AS principle91, " +
                        " IFNULL(SUM(CASE WHEN DATEDIFF(DATE('" + dateFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 1 AND 30 THEN ST.interest_less END), 0) AS interest130, " +
                        " IFNULL(SUM(CASE WHEN DATEDIFF(DATE('" + dateFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 1 AND 30 THEN 1 END), 0) AS has_interest130, " +
                        " IFNULL(SUM(CASE WHEN DATEDIFF(DATE('" + dateFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 31 AND 60 THEN ST.interest_less END), 0) AS interest3160, " +
                        " IFNULL(SUM(CASE WHEN DATEDIFF(DATE('" + dateFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 31 AND 60 THEN 1 END), 0) AS has_interest3160, " +
                        " IFNULL(SUM(CASE WHEN DATEDIFF(DATE('" + dateFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 61 AND 90 THEN ST.interest_less END), 0) AS interest6190, " +
                        " IFNULL(SUM(CASE WHEN DATEDIFF(DATE('" + dateFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 61 AND 90 THEN 1 END), 0) AS has_interest6190, " +
                        " IFNULL(SUM(CASE WHEN DATEDIFF(DATE('" + dateFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) >= 91 THEN ST.interest_less END), 0) AS interest91, " +
                        " IFNULL(SUM(CASE WHEN DATEDIFF(DATE('" + dateFormat + "'), DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) >= 91 THEN 1 END), 0) AS has_interest91 " +
                        " FROM schedule_ticket ST LEFT JOIN contract IC ON ST.contract_id = IC.id " +
                        " LEFT JOIN(SELECT contract_id, SUM(principle_less) principle_less FROM schedule_ticket WHERE ticket_status != 'P' AND branch_id = " + ddBranchName.SelectedItem.Value + " GROUP BY contract_id) IOUS ON ST.contract_id = IOUS.contract_id " +
                        " WHERE ST.`ticket_status` != 'P' AND ST.`ticket_status` != 'FPP' AND ST.branch_id = " + ddBranchName.SelectedItem.Value + " AND IC.currency_id = " + ddCurrency.SelectedItem.Value +
                        " AND DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END) < DATE('" + dateFormat + "') GROUP BY ST.contract_id " +
                        " ORDER BY ST.contract_id) B " +
                        " ) A ON A.contract_id = C.id " +
                        " LEFT JOIN(SELECT contract_id, SUM(principle_less) principle_less FROM schedule_ticket WHERE ticket_status != 'P' AND branch_id = " + ddBranchName.SelectedItem.Value + " GROUP BY contract_id) OUS ON C.id = OUS.contract_id " +
                        " LEFT JOIN currency CUR ON C.currency_id = CUR.id " +
                        " LEFT JOIN staff_info SI ON C.pawn_officer_id = SI.id " +
                        " LEFT JOIN( " +
                        "     SELECT ISI.id, COUNT(IRN.siid) renew_num, SUM(IRN.principle_due) renew_amt FROM " +
                        "         (SELECT I.id siid, ST.principle_due FROM schedule_ticket ST LEFT JOIN contract IC ON ST.contract_id = IC.id " +
                        "         LEFT JOIN staff_info I ON IC.pawn_officer_id = I.id " +
                        "         WHERE(ST.`ticket_status` = 'P' OR ST.`ticket_status` = 'FPP') " +
                        "         AND CASE WHEN ST.`ticket_status` = 'P' THEN MONTH(ST.paid_date) = " + month + " AND YEAR(ST.paid_date) = " + year +
                        "         WHEN ST.`ticket_status` = 'FPP' THEN MONTH(ST.prepaid_date) = " + month + " AND YEAR(ST.prepaid_date) = " + year + " END " +
                        "         AND CASE WHEN DATE(IC.disbursement_date) = DATE(CASE WHEN ST.`ticket_status` = 'P' THEN ST.paid_date WHEN ST.`ticket_status` = 'FPP' THEN ST.prepaid_date END) THEN ST.order_no > 1 ELSE TRUE END " +
                        "         AND ST.last_schedule = FALSE AND IC.contract_status IN(4, 7) AND IC.`b_status`= 1 " +
                        "         AND ST.branch_id = " + ddBranchName.SelectedItem.Value + " AND IC.currency_id = " + ddCurrency.SelectedItem.Value +
                        "         ) IRN LEFT JOIN staff_info ISI ON IRN.siid = ISI.id " +
                        "     GROUP BY ISI.id) RN ON SI.id = RN.id " +
                        " WHERE C.branch_id = " + ddBranchName.SelectedItem.Value + " AND C.currency_id =" + ddCurrency.SelectedItem.Value + " AND C.`b_status`= 1 AND C.contract_status >= 4 " +
                        " GROUP BY SI.`name`) MM ON SIO.id = MM.siid";
            }
            DataTable summaryReportDT = db.getDataTable(summaryReportSql);
            GenerateReport(summaryReportDT);
        }
        
    }
}