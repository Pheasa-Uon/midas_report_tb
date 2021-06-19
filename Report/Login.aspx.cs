using System;
using System.Web;
using LMSReport.Utils;
using Report.Utils;

namespace Report
{
    public partial class LogInForm : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (HttpContext.Current.Session["userID"] != null && HttpContext.Current.Session["isSuperAdmin"] != null)
                {
                    Response.Redirect("~/Dashboard");
                }
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            var pEncrypt = AESUtil.EncryptString(txtPassword.Text);
         
            var user = DataHelper.login(txtUsername.Text, "BorhjiY6JTfWc/HRSOkbOQ==");
            if (user.id != 0)
            {
                HttpContext.Current.Session["userID"] = user.id;
                HttpContext.Current.Session["isSuperAdmin"] = user.isSuperAdmin;
                HttpContext.Current.Session["name"] = user.name;
                HttpContext.Current.Session["username"] = user.username;
                Response.Redirect("~/Dashboard");
            }
            else
            {
                lblLogin.Text = "notMatch";
            }
        }
    }
}