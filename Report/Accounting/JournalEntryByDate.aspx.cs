using Microsoft.Reporting.WebForms;
using System;
using System.Data;
using Report.Utils;

namespace Report.Accounting
{
    public partial class JournalEntryByDate : System.Web.UI.Page
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
                DataHelper.populateTransactionTypeDDL(ddTransactionType);
                var date = DataHelper.getSystemDateTextbox();
                dtpFromDate.Text = date;
                dtpToDate.Text = date;
            }
        }

        private void GenerateReport(DataTable trialBalanceDT)
        {
            ReportParameterCollection reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("Branch", ddBranchName.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("FromDate", DateTime.ParseExact(dtpFromDate.Text, format, null).ToString("dd-MMM-yyyy")));
            reportParameters.Add(new ReportParameter("ToDate", DateTime.ParseExact(dtpToDate.Text, format, null).ToString("dd-MMM-yyyy")));
            reportParameters.Add(new ReportParameter("TransactionType", ddTransactionType.SelectedItem.Text));

            var _journalEntry = new ReportDataSource("JournalEntryByDateDS", trialBalanceDT);
            DataHelper.generateAccountingReport(ReportViewer1, "JournalEntryByDate", reportParameters, _journalEntry);
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


            var sql = "";
            if (ddBranchName.SelectedItem.Value == "ALL")
            {
                sql = "SELECT CASE WHEN act.balance_side = 1 THEN act.amount ELSE NULL END AS 'DR', " +
               " CASE WHEN act.balance_side = 2 THEN act.amount ELSE NULL END AS 'CR',jou.is_manual, " +
               " jou.trnx_id,act.trx_memo, act.gl_code,coa.gl_name,cur.currency,act.amount, " +
               " jou.trnx_ref,sysdate.system_date sys_date,jou.journal_desc,usr.username,pro.lob_name,con.contract_no, " +
               " cus.customer_name, act.last_updated " +
               " FROM acc_transaction act " +
               " LEFT JOIN acc_chat_of_account coa ON act.gl_id = coa.id " +
               " LEFT JOIN currency cur ON act.currency_id = cur.id " +
               " LEFT JOIN acc_journal jou ON act.jounal_id = jou.id " +
               " LEFT JOIN `user` usr ON jou.created_by_id = usr.id " +
               " LEFT JOIN contract con ON act.contract_id = con.id " +
               " LEFT JOIN product pro ON con.product_id = pro.id " +
               " LEFT JOIN customer cus ON con.customer_id = cus.id " +
               " LEFT JOIN system_date sysdate on act.system_date_id = sysdate.id" +
               " WHERE DATE(act.sys_date) BETWEEN DATE('" + fromDate + "') AND DATE('" + toDate + "') AND jou.trnx_status = 1 AND  act.trx_status IN(1, 2) ";
            }
            else
            {
                sql = "SELECT CASE WHEN act.balance_side = 1 THEN act.amount ELSE NULL END AS 'DR', " +
                " CASE WHEN act.balance_side = 2 THEN act.amount ELSE NULL END AS 'CR',jou.is_manual, " +
                " jou.trnx_id,act.trx_memo, act.gl_code,coa.gl_name,cur.currency,act.amount, " +
                " jou.trnx_ref,sysdate.system_date sys_date,jou.journal_desc,usr.username,pro.lob_name,con.contract_no, " +
                " cus.customer_name, act.last_updated " +
                " FROM acc_transaction act " +
                " LEFT JOIN acc_chat_of_account coa ON act.gl_id = coa.id " +
                " LEFT JOIN currency cur ON act.currency_id = cur.id " +
                " LEFT JOIN acc_journal jou ON act.jounal_id = jou.id " +
                " LEFT JOIN `user` usr ON jou.created_by_id = usr.id " +
                " LEFT JOIN contract con ON act.contract_id = con.id " +
                " LEFT JOIN product pro ON con.product_id = pro.id " +
                " LEFT JOIN customer cus ON con.customer_id = cus.id " +
                " LEFT JOIN system_date sysdate on act.system_date_id = sysdate.id" +
                " WHERE DATE(act.sys_date) BETWEEN DATE('" + fromDate + "') AND DATE('" + toDate + "') AND jou.trnx_status = 1 AND act.branch_id = " + ddBranchName.SelectedItem.Value + " AND act.trx_status IN(1, 2) ";
            }


            if (ddTransactionType.SelectedItem.Value != "0")
            {
                   sql += " AND jou.transaction_type_id = " + ddTransactionType.SelectedItem.Value;
            }
            sql += " ORDER BY jou.entry_no;";

            DataTable journalEntryDT = db.getDataTable(sql);
            GenerateReport(journalEntryDT);
        }
    }
}