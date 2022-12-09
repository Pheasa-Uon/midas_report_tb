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
    public partial class COProductivity : System.Web.UI.Page
    {
        private DBConnect db = new DBConnect();
        static List<Currency> currencyList;
        public static string systemDateStr;
        public string format = "dd-MMM-yyyy";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DataHelper.checkLoginSession();
                DataHelper.populateBranchDDL(ddBranchName, DataHelper.getUserId());
                populateOfficer();
                currencyList = DataHelper.populateCurrencyDDL(ddCurrency);
                systemDateStr = DataHelper.getSystemDateStr();
            }
        }

        private void GenerateReport(DataTable coProductivityDT)
        {
            ReportParameterCollection reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("BranchParameter", ddBranchName.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("OfficerParameter", ddOfficer.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("SystemDateParameter", DateTime.ParseExact(systemDateStr, format, null).ToString("dd-MMM-yyyy")));
            reportParameters.Add(new ReportParameter("CurrencyParameter", ddCurrency.SelectedItem.Text));

            var _coProductivitylist = new ReportDataSource("COProductivityDS", coProductivityDT);
            DataHelper.generateOperationReport(ReportViewer1, "COProductivity", reportParameters, _coProductivitylist);
         }

        protected void btnView_Click(object sender, EventArgs e)
        {
            //Split System Date Time variable
            var systemDateSql = DateTime.ParseExact(systemDateStr, format, null);
            var dateFormat = systemDateSql.ToString("yyyy-MM-dd");
            var day = systemDateSql.ToString("dd");
            var month = systemDateSql.ToString("MM");
            var year = systemDateSql.ToString("yyyy");

            var coProductivitySql = "";
            if (ddBranchName.SelectedItem.Value == "ALL")
            {
                coProductivitySql = "SELECT PP.lob_name,MM.* FROM product PP LEFT JOIN " +
                                    "(SELECT P.id cotid, " +
                                    "SUM(CASE WHEN MONTH(C.disbursement_date) = " + month + " AND YEAR(C.disbursement_date) = " + year + " THEN 1 ELSE 0 END) AS 'new_num_client', " +
                                    "IFNULL(SUM(CASE WHEN MONTH(C.disbursement_date) = " + month + " AND YEAR(C.disbursement_date) = " + year + " THEN C.pawn_price_approved END), 0) AS 'new_amt', " +
                                    "SUM(CASE WHEN MONTH(C.redeem_date) = " + month + " AND YEAR(C.redeem_date) = " + year + " THEN 1 ELSE 0 END) AS 'redeem_num_client', " +
                                    "IFNULL(SUM(CASE WHEN MONTH(C.redeem_date) = " + month + " AND YEAR(C.redeem_date) = " + year + " THEN C.pawn_price_approved END), 0) AS 'redeem_amt', " +
                                    "IFNULL(SUM(CASE WHEN C.contract_status = 4 THEN 1 ELSE 0 END), 0) AS 'current_num_client', " +
                                    "IFNULL(SUM(PL.principle_less), 0) AS 'current_amt', " +
                                    "SUM(CASE WHEN A.id > 0 THEN PL.principle_less ELSE 0 END) AS late_principle_amt, " +
                                    "IFNULL(COUNT(A.late_num_client), 0) AS late_num_client, " +
                                    "SUM(CASE WHEN A.has_interest130 > 0 AND A.has_interest91 <= 0 AND A.has_interest6190 <= 0 AND A.has_interest3160 <= 0 THEN A.interest130 ELSE 0 END) AS interest130, " +
                                    "SUM(CASE WHEN A.has_interest3160 > 0 AND A.has_interest91 <= 0 AND A.has_interest6190 <= 0 THEN A.interest130 + A.interest3160 ELSE 0 END) AS interest3160, " +
                                    "SUM(CASE WHEN A.has_interest6190 > 0 AND A.has_interest91 <= 0 THEN A.interest130 + A.interest3160 + A.interest6190 ELSE 0 END) AS interest6190, " +
                                    "SUM(CASE WHEN A.has_interest91 > 0 THEN A.interest130 + A.interest3160 + A.interest6190 + A.interest91 ELSE 0 END) AS interest91, " +
                                    "SUM(A.principle130) AS principle130, " +
                                    "SUM(A.principle3160) AS principle3160, " +
                                    "SUM(A.principle6190) AS principle6190, " +
                                    "SUM(A.principle91) AS principle91, " +
                                    "CUR.currency " +
                                    "FROM " +
                                    "contract C LEFT JOIN " +
                                    "(SELECT B.id, B.contract_id, B.contract_no, B.late_principle_amt, B.late_num_client, B.interest130, B.interest3160, B.interest6190, B.interest91, " +
                                    "B.has_interest130, B.has_interest3160, B.has_interest6190, B.has_interest91, " +
                                        "CASE WHEN B.principle91 > 0 THEN B.principle91 ELSE 0 END AS principle91, " +
                                        "CASE WHEN B.principle6190 > 0 AND B.principle91 <= 0 THEN B.principle6190 ELSE 0 END AS principle6190, " +
                                        "CASE WHEN B.principle3160 > 0 AND B.principle91 <= 0 AND B.principle6190 <= 0 THEN B.principle3160 ELSE 0 END AS principle3160, " +
                                        "CASE WHEN B.principle130 > 0 AND B.principle91 <= 0 AND B.principle6190 <= 0 AND B.principle3160 <= 0 THEN B.principle130 ELSE 0 END AS principle130 " +
                                    "FROM(SELECT MIN(ST.id) id, ST.contract_id, ST.contract_no, " +
                                            "IFNULL(SUM(CASE WHEN DATEDIFF('" + dateFormat + "', DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) >= 1 THEN ST.principle_less END), 0) AS late_principle_amt, " +
                                            "IFNULL(CASE WHEN DATEDIFF('" + dateFormat + "', DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) >= 1 THEN 1 END, 0) AS late_num_client, " +
                                            "IFNULL(MAX(CASE WHEN DATEDIFF('" + dateFormat + "', DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 1 AND 30 THEN IOUS.principle_less END), 0) AS principle130, " +
                                            "IFNULL(MAX(CASE WHEN DATEDIFF('" + dateFormat + "', DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 31 AND 60 THEN IOUS.principle_less END), 0) AS principle3160, " +
                                            "IFNULL(MAX(CASE WHEN DATEDIFF('" + dateFormat + "', DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 61 AND 90 THEN IOUS.principle_less END), 0) AS principle6190, " +
                                            "IFNULL(MAX(CASE WHEN DATEDIFF('" + dateFormat + "', DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) >= 91 THEN IOUS.principle_less END), 0) AS principle91, " +
                                            "IFNULL(SUM(CASE WHEN DATEDIFF('" + dateFormat + "', DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 1 AND 30 THEN ST.interest_less END), 0) AS interest130, " +
                                            "IFNULL(SUM(CASE WHEN DATEDIFF('" + dateFormat + "', DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 1 AND 30 THEN 1 END), 0) AS has_interest130, " +
                                            "IFNULL(SUM(CASE WHEN DATEDIFF('" + dateFormat + "', DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 31 AND 60 THEN ST.interest_less END), 0) AS interest3160, " +
                                            "IFNULL(SUM(CASE WHEN DATEDIFF('" + dateFormat + "', DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 31 AND 60 THEN 1 END), 0) AS has_interest3160, " +
                                            "IFNULL(SUM(CASE WHEN DATEDIFF('" + dateFormat + "', DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 61 AND 90 THEN ST.interest_less END), 0) AS interest6190, " +
                                            "IFNULL(SUM(CASE WHEN DATEDIFF('" + dateFormat + "', DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 61 AND 90 THEN 1 END), 0) AS has_interest6190, " +
                                            "IFNULL(SUM(CASE WHEN DATEDIFF('" + dateFormat + "', DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) >= 91 THEN ST.interest_less END), 0) AS interest91, " +
                                            "IFNULL(SUM(CASE WHEN DATEDIFF('" + dateFormat + "', DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) >= 91 THEN 1 END), 0) AS has_interest91 " +
                                        "FROM schedule_ticket ST LEFT JOIN contract IC ON ST.contract_id = IC.id " +
                                        "LEFT JOIN(SELECT contract_id, SUM(principle_less) principle_less FROM schedule_ticket WHERE ticket_status != 'P'  GROUP BY contract_id) IOUS ON ST.contract_id = IOUS.contract_id " +
                                        "WHERE ST.`ticket_status` != 'P' AND ST.`ticket_status` != 'FPP'  AND IC.currency_id = " + ddCurrency.SelectedValue +
                                        " AND DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END) < '" + dateFormat + "' GROUP BY ST.contract_id " +
                                        "ORDER BY ST.contract_id) B " +
                                    ") A  ON A.contract_id = C.id " +
                                    "LEFT JOIN currency CUR ON C.currency_id = CUR.id " +
                                    "LEFT JOIN collateral COL ON C.id = COL.contract_id " +
                                    "LEFT JOIN(SELECT contract_id, SUM(principle_less) principle_less FROM schedule_ticket WHERE ticket_status != 'P'  GROUP BY contract_id) PL ON C.id = PL.contract_id " +
                                    "RIGHT JOIN product P ON C.product_id = P.id " +
                                    "WHERE C.pawn_officer_id = " + ddOfficer.SelectedValue + " AND C.currency_id = " + ddCurrency.SelectedValue + " AND C.`b_status`= 1 AND(C.contract_status = 4 OR C.contract_status = 6 OR C.contract_status = 8) " +
                                    "GROUP BY P.lob_name) MM ON PP.id = MM.cotid";
            }
            else
            {
                coProductivitySql = "SELECT PP.lob_name,MM.* FROM product PP LEFT JOIN " +
                                    "(SELECT P.id cotid, " +
                                    "SUM(CASE WHEN MONTH(C.disbursement_date) = " + month + " AND YEAR(C.disbursement_date) = " + year + " THEN 1 ELSE 0 END) AS 'new_num_client', " +
                                    "IFNULL(SUM(CASE WHEN MONTH(C.disbursement_date) = " + month + " AND YEAR(C.disbursement_date) = " + year + " THEN C.pawn_price_approved END), 0) AS 'new_amt', " +
                                    "SUM(CASE WHEN MONTH(C.redeem_date) = " + month + " AND YEAR(C.redeem_date) = " + year + " THEN 1 ELSE 0 END) AS 'redeem_num_client', " +
                                    "IFNULL(SUM(CASE WHEN MONTH(C.redeem_date) = " + month + " AND YEAR(C.redeem_date) = " + year + " THEN C.pawn_price_approved END), 0) AS 'redeem_amt', " +
                                    "IFNULL(SUM(CASE WHEN C.contract_status = 4 THEN 1 ELSE 0 END), 0) AS 'current_num_client', " +
                                    "IFNULL(SUM(PL.principle_less), 0) AS 'current_amt', " +
                                    "SUM(CASE WHEN A.id > 0 THEN PL.principle_less ELSE 0 END) AS late_principle_amt, " +
                                    "IFNULL(COUNT(A.late_num_client), 0) AS late_num_client, " +
                                    "SUM(CASE WHEN A.has_interest130 > 0 AND A.has_interest91 <= 0 AND A.has_interest6190 <= 0 AND A.has_interest3160 <= 0 THEN A.interest130 ELSE 0 END) AS interest130, " +
                                    "SUM(CASE WHEN A.has_interest3160 > 0 AND A.has_interest91 <= 0 AND A.has_interest6190 <= 0 THEN A.interest130 + A.interest3160 ELSE 0 END) AS interest3160, " +
                                    "SUM(CASE WHEN A.has_interest6190 > 0 AND A.has_interest91 <= 0 THEN A.interest130 + A.interest3160 + A.interest6190 ELSE 0 END) AS interest6190, " +
                                    "SUM(CASE WHEN A.has_interest91 > 0 THEN A.interest130 + A.interest3160 + A.interest6190 + A.interest91 ELSE 0 END) AS interest91, " +
                                    "SUM(A.principle130) AS principle130, " +
                                    "SUM(A.principle3160) AS principle3160, " +
                                    "SUM(A.principle6190) AS principle6190, " +
                                    "SUM(A.principle91) AS principle91, " +
                                    "CUR.currency " +
                                    "FROM " +
                                    "contract C LEFT JOIN " +
                                    "(SELECT B.id, B.contract_id, B.contract_no, B.late_principle_amt, B.late_num_client, B.interest130, B.interest3160, B.interest6190, B.interest91, " +
                                    "B.has_interest130, B.has_interest3160, B.has_interest6190, B.has_interest91, " +
                                        "CASE WHEN B.principle91 > 0 THEN B.principle91 ELSE 0 END AS principle91, " +
                                        "CASE WHEN B.principle6190 > 0 AND B.principle91 <= 0 THEN B.principle6190 ELSE 0 END AS principle6190, " +
                                        "CASE WHEN B.principle3160 > 0 AND B.principle91 <= 0 AND B.principle6190 <= 0 THEN B.principle3160 ELSE 0 END AS principle3160, " +
                                        "CASE WHEN B.principle130 > 0 AND B.principle91 <= 0 AND B.principle6190 <= 0 AND B.principle3160 <= 0 THEN B.principle130 ELSE 0 END AS principle130 " +
                                    "FROM(SELECT MIN(ST.id) id, ST.contract_id, ST.contract_no, " +
                                            "IFNULL(SUM(CASE WHEN DATEDIFF('" + dateFormat + "', DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) >= 1 THEN ST.principle_less END), 0) AS late_principle_amt, " +
                                            "IFNULL(CASE WHEN DATEDIFF('" + dateFormat + "', DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) >= 1 THEN 1 END, 0) AS late_num_client, " +
                                            "IFNULL(MAX(CASE WHEN DATEDIFF('" + dateFormat + "', DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 1 AND 30 THEN IOUS.principle_less END), 0) AS principle130, " +
                                            "IFNULL(MAX(CASE WHEN DATEDIFF('" + dateFormat + "', DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 31 AND 60 THEN IOUS.principle_less END), 0) AS principle3160, " +
                                            "IFNULL(MAX(CASE WHEN DATEDIFF('" + dateFormat + "', DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 61 AND 90 THEN IOUS.principle_less END), 0) AS principle6190, " +
                                            "IFNULL(MAX(CASE WHEN DATEDIFF('" + dateFormat + "', DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) >= 91 THEN IOUS.principle_less END), 0) AS principle91, " +
                                            "IFNULL(SUM(CASE WHEN DATEDIFF('" + dateFormat + "', DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 1 AND 30 THEN ST.interest_less END), 0) AS interest130, " +
                                            "IFNULL(SUM(CASE WHEN DATEDIFF('" + dateFormat + "', DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 1 AND 30 THEN 1 END), 0) AS has_interest130, " +
                                            "IFNULL(SUM(CASE WHEN DATEDIFF('" + dateFormat + "', DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 31 AND 60 THEN ST.interest_less END), 0) AS interest3160, " +
                                            "IFNULL(SUM(CASE WHEN DATEDIFF('" + dateFormat + "', DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 31 AND 60 THEN 1 END), 0) AS has_interest3160, " +
                                            "IFNULL(SUM(CASE WHEN DATEDIFF('" + dateFormat + "', DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 61 AND 90 THEN ST.interest_less END), 0) AS interest6190, " +
                                            "IFNULL(SUM(CASE WHEN DATEDIFF('" + dateFormat + "', DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) BETWEEN 61 AND 90 THEN 1 END), 0) AS has_interest6190, " +
                                            "IFNULL(SUM(CASE WHEN DATEDIFF('" + dateFormat + "', DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) >= 91 THEN ST.interest_less END), 0) AS interest91, " +
                                            "IFNULL(SUM(CASE WHEN DATEDIFF('" + dateFormat + "', DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END)) >= 91 THEN 1 END), 0) AS has_interest91 " +
                                        "FROM schedule_ticket ST LEFT JOIN contract IC ON ST.contract_id = IC.id " +
                                        "LEFT JOIN(SELECT contract_id, SUM(principle_less) principle_less FROM schedule_ticket WHERE ticket_status != 'P' AND branch_id = " + ddBranchName.SelectedValue + " GROUP BY contract_id) IOUS ON ST.contract_id = IOUS.contract_id " +
                                        "WHERE ST.`ticket_status` != 'P' AND ST.`ticket_status` != 'FPP' AND ST.branch_id = " + ddBranchName.SelectedValue + " AND IC.currency_id = " + ddCurrency.SelectedValue +
                                        " AND DATE(CASE WHEN IC.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END) < '" + dateFormat + "' GROUP BY ST.contract_id " +
                                        "ORDER BY ST.contract_id) B " +
                                    ") A  ON A.contract_id = C.id " +
                                    "LEFT JOIN currency CUR ON C.currency_id = CUR.id " +
                                    "LEFT JOIN collateral COL ON C.id = COL.contract_id " +
                                    "LEFT JOIN(SELECT contract_id, SUM(principle_less) principle_less FROM schedule_ticket WHERE ticket_status != 'P' AND branch_id = " + ddBranchName.SelectedValue + " GROUP BY contract_id) PL ON C.id = PL.contract_id " +
                                    "RIGHT JOIN product P ON C.product_id = P.id " +
                                    "WHERE C.pawn_officer_id = " + ddOfficer.SelectedValue + " AND C.currency_id = " + ddCurrency.SelectedValue + " AND C.`b_status`= 1 AND(C.contract_status = 4 OR C.contract_status = 6 OR C.contract_status = 8) " +
                                    "GROUP BY P.lob_name) MM ON PP.id = MM.cotid";
            }
            var coProductivityDT = db.getDataTable(coProductivitySql);
            GenerateReport(coProductivityDT);
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
    }
}