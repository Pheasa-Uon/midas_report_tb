using Report.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Report.Accounting
{
    public partial class AccrualDetail : System.Web.UI.Page
    {
        private DBConnect db = new DBConnect();
        DateTime currentDate = DateTime.Today;
        private static string  systemDate;
        public string format = "dd/MM/yyyy";
        protected void Page_Load(object sender, EventArgs e)
        {
            DataHelper.checkLoginSession();
            //Convert Date Block
            systemDate = dtpSystemDate.Text;
        

            if (!IsPostBack)
            {
                txtContract.Text = "";
                dtpSystemDate.Text = DataHelper.getSystemDateStr();
           
            }
        }
    }
}