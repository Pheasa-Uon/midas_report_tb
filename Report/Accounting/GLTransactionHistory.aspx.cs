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
    public partial class GLTransactionHistory : System.Web.UI.Page
    {
        private DBConnect db = new DBConnect();
        DateTime currentDate = DateTime.Today;
        private static string systemDateStr;
        public string format = "dd/MM/yyyy";
        public static string fromDate, toDate;
        private string glSelect;
        String[] glCode;

        protected void Page_Load(object sender, EventArgs e)
        {
            DataHelper.checkLoginSession();
            //Convert Date Block
            fromDate = dtpFromDate.Text;
            toDate = dtpToDate.Text;

            //Adding Text and Value to Branch DropdownList block
            if (!IsPostBack)
            {
                DataHelper.populateBranchDDL(ddBranchName, DataHelper.getUserId());
                DataHelper.populateCurrencyDDL(ddCurrency);
                systemDateStr = DataHelper.getSystemDateStr();
                dtpFromDate.Text = DataHelper.getSystemDateStr();
                dtpToDate.Text = DataHelper.getSystemDateStr();
            }
        }

        //GenerateReport Function
        private void GenerateReport(DataTable glTransactionDT, DataTable openingBalanceDT)
        {
            //Generate Report Block
            ReportParameterCollection reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("BranchParameter", ddBranchName.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("CurrencyParameter", ddCurrency.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("FromDateParameter", DateTime.ParseExact(dtpFromDate.Text, format, null).ToString("dd-MMM-yyyy")));
            reportParameters.Add(new ReportParameter("ToDateParameter", DateTime.ParseExact(dtpToDate.Text, format, null).ToString("dd-MMM-yyyy")));
            reportParameters.Add(new ReportParameter("GLCodeParameter", glCode[0]));
            reportParameters.Add(new ReportParameter("GLNameParameter", glCode[1]));

            var _firstGLTransaction = new ReportDataSource("GLHistDataset", glTransactionDT);
            var _secondGLTransaction = new ReportDataSource("GL", glTransactionDT);
            var _thirdGLTransaction = new ReportDataSource("OpenBalance", openingBalanceDT);
            var _fourdGLTransaction = new ReportDataSource("CloseBalance", glTransactionDT);
            DataHelper.generateAccountingReport(ReportViewer1, "GLTransactionHistory", reportParameters, _firstGLTransaction, _secondGLTransaction, _thirdGLTransaction, _fourdGLTransaction);
        }

        protected void ddBranchName_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddBranchName.SelectedValue != "")
            {
                ddGL.Enabled = true;
                DataHelper.populateGLDDL(ddGL, Convert.ToInt32(ddBranchName.SelectedValue));
            }
            else
            {
                ddGL.Enabled = false;
                ddGL.SelectedItem.Text = "";
            }
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            //Split System Date Time variable
            var fromDateSql = DateTime.ParseExact(dtpFromDate.Text, format, null);
            var fromDateFormat = fromDateSql.ToString("yyyy-MM-dd");

            var toDateSql = DateTime.ParseExact(dtpToDate.Text, format, null);
            var toDateFormat = toDateSql.ToString("yyyy-MM-dd");

            var systemDateSql = DateTime.ParseExact(systemDateStr, format, null);
            var systemDateFormat = toDateSql.ToString("yyyy-MM-dd");

            var branchSelect = ddBranchName.SelectedItem.Value;

            glSelect = ddGL.SelectedItem.Text;
            glCode = glSelect.Split('-');

            var glTransactionSql = "SELECT CASE WHEN act.balance_side = 1 THEN act.amount ELSE 0 END AS 'DR', "+
                                   "CASE WHEN act.balance_side = 2 THEN act.amount ELSE 0 END AS 'CR',jou.is_manual, "+
                                   "CASE " +
                                     "WHEN aac.id = 1 OR aac.id = 5 THEN " +
                                       "CASE WHEN act.balance_side = 1 THEN - act.amount ELSE act.amount END " +
                                       "ELSE CASE WHEN act.balance_side = 2 THEN act.amount ELSE -act.amount END " +
                                   "END AS amount, " +
                                   "jou.trnx_id,act.trx_memo,act.gl_code,coa.gl_name,cur.currency,jou.trnx_ref, " +
                                   "act.sys_date,jou.journal_desc,usr.username,aac.id AS class_type,avd.vendor_name,cus.customer_name,act.balance_side " +
                                   "FROM acc_transaction act " +
                                   "LEFT JOIN acc_chat_of_account coa ON act.gl_id = coa.id " +
                                   "LEFT JOIN acc_account_class aac ON coa.acc_class_id = aac.id "+
                                   "LEFT JOIN currency cur ON act.currency_id = cur.id "+
                                   "LEFT JOIN acc_journal jou ON act.jounal_id = jou.id "+
                                   "LEFT JOIN acc_bill acb ON jou.id = acb.journal_id "+
                                   "LEFT JOIN acc_vendor avd ON acb.vendor_id = avd.id "+
                                   "LEFT JOIN contract con ON act.contract_id = con.id "+
                                   "LEFT JOIN customer cus ON con.customer_id = cus.id "+
                                   "LEFT JOIN user usr ON jou.created_by_id = usr.id "+
                                   "WHERE DATE(act.sys_date) BETWEEN '" + fromDateFormat + "' AND '" + toDateFormat + "' AND jou.trnx_status = 1 AND act.branch_id = " + branchSelect +
                                   " AND act.gl_code = '" + glCode[0] + "' ORDER BY act.sys_date,act.date_created";

            var openingBalanceSql = "SELECT ACA.acc_class_id,IFNULL(ABH.balance,0) as open_balance FROM acc_gl_balance_hist ABH LEFT JOIN acc_chat_of_account ACA ON ABH.gl = ACA.gl "+
                                    "WHERE ABH.branch_id = " + branchSelect + " AND DATE(ABH.sys_date) = DATE_ADD('" + systemDateFormat + "', INTERVAL - 1 DAY) AND ABH.gl = '" + glCode[0] + "' LIMIT 1 ";

            DataTable glTransactionDT = db.getDataTable(glTransactionSql);
            DataTable openingBalanceDT = db.getDataTable(openingBalanceSql);
            GenerateReport(glTransactionDT, openingBalanceDT);
        }
    }
}