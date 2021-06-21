<%@ Page Title="Payment Detail" MasterPageFile="~/Site.Master" Language="C#" AutoEventWireup="true" CodeBehind="PaymentDetailV2.aspx.cs" Inherits="Report.Operation.PaymentDetailV2" %>
<%@ Register Assembly="Microsoft.ReportViewer.WebForms" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="panel panel-warning no-margin ">
        <div class="panel-body">
            <div class="form-inline">
                <div class="form-group">
                    <label>BRANCH</label>
                    <asp:DropDownList ID="ddBranchName" runat="server" AutoPostBack="true" CssClass="form-control cnt-min-width"></asp:DropDownList>
                      <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="ddBranchName"
                        ErrorMessage="* Please select branch" ForeColor="Red" Font-Names="Tahoma" Display="Dynamic">
                    </asp:RequiredFieldValidator>
                </div>
                 <div class="form-group ml16">
                    <label>From Date:</label>
                    <asp:TextBox ID="dtpFromDate" runat="server" class="form-control cnt-min-width datepick"></asp:TextBox>
                </div>
                <div class="form-group ml16">
                    <label>ToDate:</label>
                    <asp:TextBox ID="dtpToDate" runat="server" class="form-control cnt-min-width datepick"></asp:TextBox>
                </div>
                  <div class="form-group ml16">
                            <label>Pawn Officer:</label>
                            <asp:DropDownList ID="DropDownList2" runat="server" CssClass="form-control cnt-min-width" Enabled="true">
                            </asp:DropDownList>
                </div>
                <div class="form-group ml16">
                    <asp:Button ID="btnView" runat="server" Text="View"  CssClass="btn btn-info" />
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