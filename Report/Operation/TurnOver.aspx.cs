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
    public partial class TurnOver : System.Web.UI.Page
    {
        private DBConnect db = new DBConnect();
        DateTime currentDate = DateTime.Today;
        static List<Currency> currencyList;
        public static string systemDateStr;
        public string format = "dd/MM/yyyy";

        protected void Page_Load(object sender, EventArgs e)
        {
            DataHelper.checkLoginSession();
            systemDateStr = DataHelper.getSystemDateStr();

            if (!IsPostBack)
            { 
                DataHelper.populateBranchDDL(ddBranchName, DataHelper.getUserId());
                currencyList = DataHelper.populateCurrencyDDL(ddCurrency);
                dtpSystemDate.Text = systemDateStr;
            }
        }

        //GenerateReport Function
        private void GenerateReport(DataTable turnOverDT)
        {
            var reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("BranchParameter", ddBranchName.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("CurrencyParameter", ddCurrency.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("SystemDateParameter", DateTime.ParseExact(dtpSystemDate.Text, format, null).ToString("dd-MMM-yyyy")));
            reportParameters.Add(new ReportParameter("CurrencyLabelParameter", currencyList.Find(x => x.id == Convert.ToInt32(ddCurrency.SelectedItem.Value)).currency_label));
            var _turnOverlist = new ReportDataSource("TurnoverDS", turnOverDT);
            DataHelper.generateOperationReport(ReportViewer1, "TurnOver", reportParameters, _turnOverlist);
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            //Split System Date Time variable
            var systemDateSql = DateTime.ParseExact(dtpSystemDate.Text, format, null);
            var day = systemDateSql.ToString("dd");
            var month = systemDateSql.ToString("MM");
            var year = systemDateSql.ToString("yyyy");

            var turnOverSql = "SELECT CASE WHEN ST.ticket_status = 'P' THEN ST.paid_date WHEN ST.ticket_status = 'FPP' THEN ST.prepaid_date END as trnx_date," +
                               "C.customer_name,ST.ticket_no,P.lob_name,0 as newgrant,ST.principle_due as outs" +
                               ",CASE WHEN IC.product_type_id = 1 THEN STT.due_date ELSE ST.due_date END due_date,ST.serial_number " +
                               "FROM schedule_ticket ST LEFT JOIN contract IC ON ST.contract_id = IC.id " +
                               "LEFT JOIN schedule_ticket STT ON ST.id + 1 = STT.id " +
                               "LEFT JOIN product P ON IC.product_id = P.id " +
                               "LEFT JOIN customer C ON IC.customer_id = C.id " +
                               "WHERE(ST.ticket_status = 'P' OR ST.ticket_status = 'FPP') " +
                               "AND CASE WHEN ST.ticket_status = 'P' THEN MONTH(ST.paid_date) = " + month + " AND YEAR(ST.paid_date) = " + year +
                               " WHEN ST.ticket_status = 'FPP' THEN MONTH(ST.prepaid_date) = " + month + " AND YEAR(ST.prepaid_date) = " + year +
                               " END " +
                               "AND ST.order_no > 1 AND ST.last_schedule = FALSE AND IC.contract_status = 4 AND IC.b_status = 1 " +
                               "AND ST.branch_id = " + ddBranchName.SelectedValue + " AND IC.currency_id = " + ddCurrency.SelectedValue +
                               " UNION ALL " +
                               "SELECT C.disbursement_date trnx_date, CUS.customer_name,ST.ticket_no,PD.lob_name,C.pawn_price_approved newgrant,0 as outs,ST.due_date,ST.serial_number " +
                               "FROM contract C " +
                               "LEFT JOIN schedule_ticket ST  ON ST.contract_id = C.id AND ST.order_no = 1 " +
                               "LEFT JOIN customer CUS ON C.customer_id = CUS.id " +
                               "LEFT JOIN product PD ON PD.id = C.product_id " +
                               "WHERE C.branch_id = " + ddBranchName.SelectedValue + " AND C.contract_status >= 4 AND C.b_status = 1 AND C.currency_id = " + ddCurrency.SelectedValue +
                               " AND MONTH(C.disbursement_date) = " + month + " AND YEAR(C.disbursement_date) = " + year;

            DataTable turnOverDT = db.getDataTable(turnOverSql);
            GenerateReport(turnOverDT);
        }
    }
}