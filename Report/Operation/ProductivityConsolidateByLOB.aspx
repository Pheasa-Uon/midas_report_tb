<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ProductivityConsolidateByLOB.aspx.cs" Inherits="Report.Operation.ProductivityConsolidateByLOB" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="panel panel-warning no-margin">
        <div class="panel-body">
            <div class="form-inline">
                <div class="form-group ml16">
                    <label>Currency:</label>
                    <asp:DropDownList ID="ddCurrency" runat="server" CssClass="form-control cnt-min-width">
                    </asp:DropDownList>
                </div>
                <div class="form-group ml16">
                    <asp:Button ID="btnView" runat="server" Text="View" OnClick="btnView_Click" CssClass="btn btn-info" />
                </div>
            </div>
        </div>
    </div>

    <div class="row">
        <div class="col-md-12">
            <center>
                <rsweb:ReportViewer ID="ReportViewer1" runat="server" nt-Names="Verdana" Font-Size="8pt" WaitMessageFont-Names="Verdana"  
                WaitMessageFont-Size="14pt" ShowPrintButton="true" ShowBackButton="true" BackColor="#999999" CssClass="printer"  
                PageCountMode="Actual" ShowZoomControl="False"></rsweb:ReportViewer>
            </center>
        </div>
    </div>
</asp:Content>
