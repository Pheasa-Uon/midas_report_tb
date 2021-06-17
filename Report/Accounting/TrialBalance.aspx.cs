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

namespace Report.Accounting
{
    public partial class TrialBalance : System.Web.UI.Page
    {
        private DBConnect db = new DBConnect();
        DateTime currentDate = DateTime.Today;
        private static string systemDateStr;
        public string fromDateStr, toDateStr;
        public string format = "dd/MM/yyyy";

        protected void Page_Load(object sender, EventArgs e)
        {
            DataHelper.checkLoginSession();
            //Convert Date Block
            fromDateStr = dtpFromDate.Text;
            toDateStr = dtpToDate.Text;

            //Adding Text and Value to Branch DropdownList block
            if (!IsPostBack)
            {
                DataHelper.populateBranchDDL(ddBranchName, DataHelper.getUserId());
                systemDateStr = DataHelper.getSystemDateStr();
                dtpFromDate.Text = DataHelper.getSystemDateStr();
                dtpToDate.Text = DataHelper.getSystemDateStr(); 
            }
        }

        //GenerateReport Function
        private void GenerateReport(DataTable trialBalanceDT)
        {
            //Generate Report Block
            ReportParameterCollection reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("BranchParameter", ddBranchName.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("FromDateParameter", DateTime.ParseExact(dtpFromDate.Text, format, null).ToString("dd-MMM-yyyy")));
            reportParameters.Add(new ReportParameter("ToDateParameter", DateTime.ParseExact(dtpToDate.Text, format, null).ToString("dd-MMM-yyyy")));

            var _trialBalance = new ReportDataSource("TrialBalanceDataset", trialBalanceDT);
            DataHelper.generateAccountingReport(ReportViewer1, "TrialBalance", reportParameters, _trialBalance);
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            var systemDateSql = DateTime.ParseExact(systemDateStr, format, null);
            var systemDateDay = systemDateSql.ToString("yyyy-MM-dd");

            var fromDateSql = DateTime.ParseExact(dtpFromDate.Text, format, null);
            var fromDateDay = fromDateSql.ToString("yyyy-MM-dd");

            var toDateSql = DateTime.ParseExact(dtpToDate.Text, format, null);
            var toDateDay = toDateSql.ToString("yyyy-MM-dd");


            var trialBalanceSql = "SELECT CC.currency,acc.gl,acc.gl_name,acc.side,acc.id,IFNULL(bal_his.balance, 0) AS open_balance, IFNULL(new_amt.debit_amount, 0) debit_amount,IFNULL(new_amt.credit_amount, 0) credit_amount, " +
                                  "IFNULL(bal_his.balance, 0) + IFNULL(new_amt.debit_amount, 0) + IFNULL(new_amt.credit_amount, 0) AS 'closing_balance' " +
                                  "FROM " +
                                  "acc_chat_of_account acc LEFT JOIN " +
                                  "(SELECT gl_id, " +
                                  "-SUM(CASE WHEN balance_side = 1 THEN amount ELSE 0 END) AS debit_amount, " +
                                  "SUM(CASE WHEN balance_side = 2 THEN amount ELSE 0 END) AS credit_amount " +
                                  "FROM acc_transaction WHERE DATE(sys_date) BETWEEN '" + fromDateDay + "' AND '" + toDateDay +
                                  "' GROUP BY gl_id) new_amt ON new_amt.gl_id = acc.id " +
                                  "LEFT JOIN acc_gl_balance_hist bal_his ON acc.id = bal_his.chart_of_account_id AND DATE(bal_his.sys_date) = DATE(DATE_ADD('" + systemDateDay + "', INTERVAL - 1 DAY)) " +
                                  "LEFT JOIN currency CC ON acc.currency_id = CC.id " +
                                  "WHERE acc.is_leaf = 1 AND acc.branch_id = " + ddBranchName.SelectedValue;

            DataTable trialBalanceDT = db.getDataTable(trialBalanceSql);
            GenerateReport(trialBalanceDT);
        }
    }
}