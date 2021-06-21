using Report.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Report.Accounting
{
    public partial class UnearnedRevenue : System.Web.UI.Page
    {

        private DBConnect db = new DBConnect();
        DateTime currentDate = DateTime.Today;
        private static string systemDate;
        public string format = "dd/MM/yyyy";
        protected void Page_Load(object sender, EventArgs e)
        {
            DataHelper.checkLoginSession();
            //Convert Date Block
            systemDate = dtpSystemDate.Text;

            if (!IsPostBack)
            {
                DataHelper.checkLoginSession();
                dtpSystemDate.Text = DataHelper.getSystemDateStr();
                DataHelper.populateBranchDDL(ddBranchName, DataHelper.getUserId());
            }
        }

        protected void ddBranchName_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddBranchName.SelectedItem.Value == "")
            {
                ddOfficer.Items.Clear();
            }
            else
            {
                DataHelper.populateOfficerDDL(ddOfficer, Convert.ToInt32(ddBranchName.SelectedItem.Value));
            }
        }
    }
}