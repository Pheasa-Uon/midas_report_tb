using Microsoft.Reporting.WebForms;
using Report.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Report.Accounting
{
    public partial class TrialBalanceTotalByClass : System.Web.UI.Page
    {
        private DBConnect db = new DBConnect();
        public string fromDate, toDate;
        public string format = "dd/MM/yyyy";
        public string dateFromError = "", dateToError = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DataHelper.checkLoginSession();
                DataHelper.populateBranchDDL(ddBranchName, DataHelper.getUserId());
                DataHelper.populateCurrencyDDL(ddCurrency);
                var date = DataHelper.getSystemDateTextbox();
                dtpFromDate.Text = date;
                dtpToDate.Text = date;
            }
        }

        private void GenerateReport(DataTable trialBalanceTBCDT)
        {
            ReportParameterCollection reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("Branch", ddBranchName.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("FromDate", DateTime.ParseExact(dtpFromDate.Text, format, null).ToString("dd-MMM-yyyy")));
            reportParameters.Add(new ReportParameter("ToDate", DateTime.ParseExact(dtpToDate.Text, format, null).ToString("dd-MMM-yyyy")));

            var _trialBalance = new ReportDataSource("TrialBalanceTBCDS", trialBalanceTBCDT);
            DataHelper.generateAccountingReport(ReportViewer1, "TrialBalanceTotalByClass", reportParameters, _trialBalance);
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            try
            {
                fromDate = DateTime.ParseExact(dtpFromDate.Text.Trim(), format, null).ToString("yyyy-MM-dd");
            }
            catch (Exception)
            {
                dateFromError = "* Date wrong format";
                return;
            }
            try
            {
                toDate = DateTime.ParseExact(dtpToDate.Text.Trim(), format, null).ToString("yyyy-MM-dd");
            }
            catch (Exception)
            {
                dateToError = "* Date wrong format";
                return;
            }

            var sql = "SELECT acc.id,acc.acc_class_id, aac.class_name,CC.currency,IFNULL(acc.parent_gl,acc.gl) parent_gl,(SELECT gl_name FROM acc_chat_of_account WHERE gl=IFNULL(acc.parent_gl,acc.gl)) parent_gl_name,acc.gl,acc.gl_name,TRIM(acc.side) AS side, " +
                " CASE WHEN bal_his.balance IS NULL THEN 0 " +
                " WHEN TRIM(acc.side)= 'Debit' AND bal_his.balance < 0 THEN bal_his.balance * (-1) " +
                " WHEN TRIM(acc.side)= 'Credit' AND bal_his.balance < 0 THEN bal_his.balance * (-1) " +
                " ELSE 0 END AS O_DR, " +
                " CASE WHEN bal_his.balance IS NULL THEN 0 " +
                " WHEN TRIM(acc.side)= 'Credit' AND bal_his.balance >= 0 THEN bal_his.balance " +
                "  WHEN TRIM(acc.side) = 'Debit' AND bal_his.balance > 0 THEN bal_his.balance " +
                "    ELSE 0 END AS O_CR, " +
                " IFNULL(new_amt.debit_amount, 0) M_DR, " +
                " IFNULL(new_amt.credit_amount, 0) M_CR, " +
                " CASE WHEN bal_his.balance - IFNULL(new_amt.debit_amount, 0) + IFNULL(new_amt.credit_amount, 0) < 0 THEN " +
                " (bal_his.balance - IFNULL(new_amt.debit_amount, 0) + IFNULL(new_amt.credit_amount, 0)) * (-1) ELSE 0 END AS C_DR," +
                " CASE WHEN bal_his.balance - IFNULL(new_amt.debit_amount, 0) + IFNULL(new_amt.credit_amount, 0) > 0 THEN" +
                " (bal_his.balance - IFNULL(new_amt.debit_amount, 0) + IFNULL(new_amt.credit_amount, 0)) ELSE 0 END AS C_CR" +
                "  FROM acc_chat_of_account acc LEFT JOIN " +
                "  (SELECT gl_id, " +
                "  SUM(CASE WHEN balance_side = 1 THEN amount ELSE 0 END) AS debit_amount, " +
                "  SUM(CASE WHEN balance_side = 2 THEN amount ELSE 0 END) AS credit_amount " +
                " FROM acc_transaction WHERE DATE(sys_date) BETWEEN DATE('" + fromDate + "') AND DATE('" + toDate + "') AND b_status = 1 AND trx_status = 1 " +
                " GROUP BY gl_id " +
                " ) AS new_amt ON new_amt.gl_id = acc.id " +
                " LEFT JOIN " +
                " acc_gl_balance_hist AS bal_his ON acc.id = bal_his.chart_of_account_id AND bal_his.currency_id = " + ddCurrency.SelectedItem.Value +
                " AND DATE(bal_his.sys_date) = DATE(DATE_ADD('" + fromDate + "', INTERVAL - 1 DAY)) " +
                " LEFT JOIN " +
                " currency CC ON acc.currency_id = CC.id " +
                " LEFT JOIN acc_account_class aac on aac.id = acc.acc_class_id " +
                " WHERE acc.is_leaf = 1 AND acc.branch_id = " + ddBranchName.SelectedItem.Value + " AND acc.currency_id = " + ddCurrency.SelectedItem.Value +
                " ORDER BY acc.gl; ";

            DataTable trialBalanceTBCDT = db.getDataTable(sql);
            GenerateReport(trialBalanceTBCDT);
        }
    }
}