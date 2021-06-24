using Report.Utils;
using System;

namespace Report.Accounting
{
    public partial class AccrualDetail : System.Web.UI.Page
    {
        private DBConnect db = new DBConnect();
        DateTime currentDate = DateTime.Today;
        public string format = "dd/MM/yyyy";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DataHelper.checkLoginSession();
                txtContract.Text = "";
                dtpSystemDate.Text = DataHelper.getSystemDate().ToString(format);
            }
        }
    }
}