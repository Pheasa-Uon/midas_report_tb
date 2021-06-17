<%@ Page Title="Summary Report" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="SummaryReport.aspx.cs" Inherits="Report.Operation.SummaryReport" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="panel panel-warning no-margin">
        <div class="panel-body">
            <div class="form-inline">
                <div class="form-group">
                    <label>Branch</label>
                    <asp:DropDownList ID="ddBranchName" runat="server" CssClass="form-control cnt-min-width">
                    </asp:DropDownList>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="ddBranchName"
                        ErrorMessage="* Please select branch" ForeColor="Red" Font-Names="Tahoma" Display="Dynamic">
                    </asp:RequiredFieldValidator>
                </div>
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
                Page--CountMode="Actual" ShowZoomControl="False"></rsweb:ReportViewer>
            </center>
        </div>
    </div>
</asp:Content>
