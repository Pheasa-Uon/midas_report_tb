using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using Report.Utils;
using System.Web.UI.WebControls;

namespace Report.Accounting
{
    public partial class JournalEntryByDate : System.Web.UI.Page
    {
        private DBConnect db = new DBConnect();
        DateTime currentDate = DateTime.Today;
        private string fromDateStr, toDateStr;
        public string format = "dd/MM/yyyy";
        private string branchDDL;
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
                DataHelper.populateTransactionTypeDDL(ddTransactionType);
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
            reportParameters.Add(new ReportParameter("TransactionTypeParameter", ddTransactionType.SelectedItem.Text));

            var _journalEntry = new ReportDataSource("JournalEntryByDateDataset", trialBalanceDT);
            DataHelper.generateAccountingReport(ReportViewer1, "JournalEntryByDate", reportParameters, _journalEntry);
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            //Split System Date Time variable

            var fromDateSql = DateTime.ParseExact(dtpFromDate.Text, format, null);
            var fromDateFull = fromDateSql.ToString("yyyy-MM-dd");

            var toDateSql = DateTime.ParseExact(dtpToDate.Text, format, null);
            var toDateFull = toDateSql.ToString("yyyy-MM-dd");

            var journalEntrySql = "SELECT CASE WHEN act.balance_side = 1 THEN act.amount ELSE 0 END as 'DR', " +
                                "CASE WHEN act.balance_side = 2 THEN act.amount ELSE 0 END as 'CR',jou.is_manual, " +
                                "jou.trnx_id,act.gl_code,coa.gl_name,cur.currency,jou.trnx_ref,act.sys_date,jou.journal_desc,usr.username,act.trx_memo,pro.lob_name " +
                                "FROM acc_transaction act " +
                                "LEFT JOIN acc_chat_of_account coa ON act.gl_id = coa.id " +
                                "LEFT JOIN currency cur ON act.currency_id = cur.id " +
                                "LEFT JOIN acc_journal jou ON act.jounal_id = jou.id " +
                                "LEFT JOIN `user` usr ON jou.created_by_id = usr.id " +
                                "LEFT JOIN contract con ON act.contract_id = con.id " +
                                "LEFT JOIN product pro ON con.product_id = pro.id " +
                                "WHERE DATE(act.sys_date) BETWEEN '" + fromDateFull + "' AND '" + toDateFull + "' AND jou.trnx_status = 1 AND act.branch_id = " + ddBranchName.SelectedValue +
                                " AND jou.transaction_type_id = CASE WHEN " + ddTransactionType.SelectedValue + " = 0 THEN jou.transaction_type_id ELSE " + ddTransactionType.SelectedValue + " END " +
                                "ORDER BY act.sys_date; ";

            DataTable journalEntryDT = db.getDataTable(journalEntrySql);
            GenerateReport(journalEntryDT);
        }
    }
}