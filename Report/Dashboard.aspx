<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="Report._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div style="margin:0 auto; padding: 0 15px">
        <h3 class="dashboard-title">PAWN SHOP REPORTING (<%=appName %>)</h3>
         <div class="row">
        <% 
            foreach (var item in rp)
            {
                var str = "";%>
        <div class="col-md-6">
             <h4 style="color: #ffb536;text-decoration:underline;font-weight:bold;"><% Response.Write(item.title);%></h4>
            <div class="list-group">
                <% int i = 0;
                    foreach (var subItem in item.items)
                    {
                        i++;
                        str += "<a class=\"col-md-6 list-group-item list-group-item-action\" runat=\"server\" href=" + subItem.routing + ">" + i + ". " + subItem.title + "</a>";
                    }
                    Response.Write(str);%>
            </div>
        </div>
        <%} %>
    </div>
    </div>
   
</asp:Content>
