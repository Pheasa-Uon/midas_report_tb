using Report.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Report.Accounting
{
    public partial class IncomeStatementByMonth : System.Web.UI.Page
    {
        private DBConnect db = new DBConnect();
        DateTime currentDate = DateTime.Today;
        private string fromDateStr, toDateStr;
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
                DataHelper.populateBranchDDLAllowAll(ddBranchName, DataHelper.getUserId());
                dtpFromDate.Text = DataHelper.getSystemDateStr();
                dtpToDate.Text = DataHelper.getSystemDateStr();
            }
        }
    }
}