using Report.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Report.Accounting
{
    public partial class DailyTillCashOperation : System.Web.UI.Page
    {
        private DBConnect db = new DBConnect();
        DateTime currentDate = DateTime.Today;
        private static string fromDate, toDate;
        public string format = "dd/MM/yyyy";
        protected void Page_Load(object sender, EventArgs e)
        {
            DataHelper.checkLoginSession();
            //Convert Date Block
            fromDate = dtpfromDate.Text;
            toDate = dtpToDate.Text;

            if (!IsPostBack)
            {
                DataHelper.checkLoginSession();
                dtpfromDate.Text = DataHelper.getSystemDateStr();
                dtpToDate.Text = DataHelper.getSystemDateStr();
                DataHelper.populateBranchDDL(ddBranchName, DataHelper.getUserId());
            }
        }
    }
}