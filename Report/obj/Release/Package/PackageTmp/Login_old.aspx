<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login_old.aspx.cs" Inherits="Report.LogInForm" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Log in - Pawn Shop - CUU</title>
    <webopt:BundleReference runat="server" Path="~/Content/css" />
    <link href="~/favicon.ico" rel="shortcut icon" type="image/x-icon" />
    <webopt:BundleReference runat="server" Path="~/Content/override.css" />
</head>
<body>
    <div class="container">
        <div class="login-form">
            <form id="form1" runat="server">
                <asp:Label ID="lbl_action" runat="server" Visible="false"></asp:Label>
                <asp:ScriptManager runat="server">
                    <Scripts>
                        <%--To learn more about bundling scripts in ScriptManager see https://go.microsoft.com/fwlink/?LinkID=301884 --%>
                        <%--Framework Scripts--%>
                        <asp:ScriptReference Name="jquery" />
                        <asp:ScriptReference Name="bootstrap" />
                        <asp:ScriptReference Name="WebForms.js" Assembly="System.Web" Path="~/Scripts/WebForms/WebForms.js" />
                        <asp:ScriptReference Name="WebUIValidation.js" Assembly="System.Web" Path="~/Scripts/WebForms/WebUIValidation.js" />
                        <%--Site Scripts--%>
                    </Scripts>
                </asp:ScriptManager>
                <h4 class="text-center" style="margin-bottom: 32px; font-weight: bold;">Pawn Shop Reporting System</h4>

                <div class="form-group">
                    <asp:TextBox ID="tfUsername" runat="server" placeholder="Username"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="UsernameRequiredFieldValidator" runat="server" CssClass="validateUsername"
                        ErrorMessage="Username field is required!" ControlToValidate="tfUsername">
                    </asp:RequiredFieldValidator>
                </div>
                <div class="form-group">
                    <asp:TextBox ID="tfPassword" runat="server" placeholder="Password" TextMode="Password"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="PasswordRequiredFieldValidator" runat="server" CssClass="validatePassword"
                        ErrorMessage="Password field is required!" ControlToValidate="tfPassword">
                    </asp:RequiredFieldValidator>
                </div>
                <div class="form-group">
                    <asp:Button ID="btnSubmit" CssClass="btn btn-primary btn-block" runat="server" OnClick="btnSubmit_Click" Text="Log in" />
                </div>
                <div class="clearfix">
                    <div class="pull-left checkbox-inline">
                        <asp:CheckBox ID="chkRememberMe" runat="server" AutoPostBack="true" />
                        <asp:Panel runat="server" ID="panel1">Remember me</asp:Panel>
                    </div>
                </div>
                <!--------------------------------------->
                <%if (IsPostBack)
                    {
                        if (lbl_action.Text == "incorrect")
                        {%>
                <div class="alert alert-danger alert-custom alert-dismissible" role="alert">
                    <button type="button" class="close" data-dismiss="alert"><span aria-hidden="true">×</span><span class="sr-only">Close</span></button>
                    <i class="fa fa-times-circle m-right-xs"></i><span>Invalid username or password!</span>			   
                </div>
                <%}
                    }%>
            </form>
        </div>
    </div>
    <script type="text/javascript">
        $(document).ready(function () {
            $(".alert").delay(5000).hide(300);
        })
    </script>
</body>
</html>
