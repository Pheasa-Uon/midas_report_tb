using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Report.Models;
using Report.Utils;

namespace Report
{
    public partial class _Default : Page
    {
        public static string appName = System.Configuration.ConfigurationManager.AppSettings["appName"];
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DataHelper.checkLoginSession();
                //rp = DataHelper.getMenuItem();
            }
        }
    }
}