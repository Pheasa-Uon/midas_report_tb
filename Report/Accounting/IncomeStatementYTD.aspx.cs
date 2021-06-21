using Report.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Report.Accounting
{
    public partial class IncomeStatementYTD : System.Web.UI.Page
    {
        private DBConnect db = new DBConnect();
        DateTime currentDate = DateTime.Today;
        private static string asOfDate;
        public string format = "dd/MM/yyyy";
        protected void Page_Load(object sender, EventArgs e)
        {
            DataHelper.checkLoginSession();
            //Convert Date Block
            asOfDate = dtpAsOfDate.Text;

            if (!IsPostBack)
            {
                DataHelper.checkLoginSession();
                dtpAsOfDate.Text = DataHelper.getSystemDateStr();
                DataHelper.populateBranchDDL(ddBranchName, DataHelper.getUserId());
            }
        }
    }
}