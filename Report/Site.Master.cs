using Report.Models;
using Report.Utils;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;

namespace Report
{
    public partial class SiteMaster : MasterPage
    {
        public static List<ReportItem> rp = new List<ReportItem>();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["name"].ToString() == "")
                {
                    Label1.Text = "Welcome " + Session["username"];
                }
                else
                {
                    Label1.Text = "Welcome " + Session["name"];
                }
                
                rp = DataHelper.getMenuItem();
            }
        }

        protected void btnLogOut_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Response.Redirect("~/Login");
        }
    }
}