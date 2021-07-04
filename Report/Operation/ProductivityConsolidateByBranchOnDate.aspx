<%@ Page Title="Productivity Consolidate By Branch On Date" MasterPageFile="~/Site.Master" Language="C#" AutoEventWireup="true" CodeBehind="ProductivityConsolidateByBranchOnDate.aspx.cs" Inherits="Report.Operation.ProductivityConsolidateByBranchOnDate" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="panel panel-warning no-margin">
        <div class="panel-body">
            <div class="row">
                <div class="col-sm-2 form-group">
                    <label>Currency:</label>
                    <asp:DropDownList ID="ddCurrency" runat="server" CssClass="form-control input-sm">
                    </asp:DropDownList>
                </div>
                <div class="form-group ml16">
                     <div>
                        <label>&nbsp;</label>
                    </div>
                    <asp:Button ID="btnView" runat="server" Text="View Report" OnClick="btnView_Click" CssClass="btn btn-sm btn-primary" />
                </div>
            </div>
        </div>
    </div>

    <div class="row">
        <div class="col-md-12">
            <center>
                 <rsweb:ReportViewer ID="ReportViewer1" runat="server" nt-Names="Verdana" Font-Size="8pt" WaitMessageFont-Names="Verdana"  
                WaitMessageFont-Size="14pt" ShowPrintButton="true" ShowBackButton="true" BackColor="#999999" CssClass="printer"  
                PageCountMode="Actual" ShowZoomControl="False" BorderStyle="None"></rsweb:ReportViewer>
            </center>
        </div>
    </div>
</asp:Content>
