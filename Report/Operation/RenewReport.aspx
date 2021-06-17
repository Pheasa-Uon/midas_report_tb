<%@ Page Title="Renew" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="RenewReport.aspx.cs" Inherits="Report.Operation.Renew" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <link href="../Content/jquery-ui.css" rel="Stylesheet" type="text/css" />
    <script src="../Scripts/jquery-ui.js" type="text/javascript"></script>
    <script src="../Scripts/datetimepicker.js" type="text/javascript"></script>

    <div class="panel panel-warning no-margin">
        <div class="panel-body">
                <div class="form-inline">
                    <div class="form-group">
                        <label>Branch:</label>
                        <asp:DropDownList ID="ddBranchName" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddBranchName_SelectedIndexChanged" CssClass="form-control cnt-min-width">
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="ddBranchName"
                          ErrorMessage="* Please select branch" ForeColor="Red" Font-Names="Tahoma" Display="Dynamic">
                        </asp:RequiredFieldValidator>
                    </div>
                    <div class="form-group ml16">
                        <label>From Date:</label>
                        <asp:TextBox ID="dtpFromDate" runat="server" class="form-control cnt-min-width datepick"></asp:TextBox>
                        <asp:CheckBox ID="chkFromDate" runat="server" Text="Null" AutoPostBack="true" TextAlign="Right" OnCheckedChanged="chkFromDate_CheckedChanged"/>
                    </div>
                    <div class="form-group ml16">
                        <label>System Date:</label>
                        <asp:TextBox ID="dtpSystemDate" runat="server" class="form-control cnt-min-width datepick"></asp:TextBox>
                    </div>
                    <div class="form-group ml16">
                        <label>Currency:</label>
                        <asp:DropDownList ID="ddCurrency" runat="server" CssClass="form-control cnt-min-width">
                        </asp:DropDownList>
                    </div>
                    <div class="form-group ml16">
                    <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
                            <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="ddBranchName" />
                            </Triggers>
                            <ContentTemplate>
                                <label>Pawn Officer:</label>
                                <asp:DropDownList ID="ddOfficer" runat="server" CssClass="form-control cnt-min-width" Enabled="false">
                                </asp:DropDownList>
                            </ContentTemplate>
                        </asp:UpdatePanel>
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
