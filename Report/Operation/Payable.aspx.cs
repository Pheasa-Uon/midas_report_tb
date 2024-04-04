using Microsoft.Reporting.WebForms;
using MySql.Data.MySqlClient;
using Report.Models;
using Report.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Report.Operation
{
    public partial class Payable : System.Web.UI.Page
    {
        private DBConnect db = new DBConnect();
        public string dateFromError = "", dateToError = "";
        private static string fromDate, toDate;
        public string format = "dd/MM/yyyy";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {

                DataHelper.checkLoginSession();

                DataHelper.populateBranchDDL(ddBranchName, DataHelper.getUserId());
                var date = DataHelper.getSystemDateTextbox();
                dtpFromDate.Text = date;
                dtpToDate.Text = date;
            }
        }
        private void GenerateReport(DataTable payableDT)
        {

            ReportParameterCollection reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("Branch", ddBranchName.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("FromDate", DateTime.ParseExact(dtpFromDate.Text, format, null).ToString("dd-MMM-yyyy")));
            reportParameters.Add(new ReportParameter("ToDate", DateTime.ParseExact(dtpToDate.Text, format, null).ToString("dd-MMM-yyyy")));

            var _payable = new ReportDataSource("payableDS", payableDT);
            DataHelper.generateOperationReport(ReportViewer1, "Payable", reportParameters, _payable);
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            try
            {
                fromDate = DateTime.ParseExact(dtpFromDate.Text.Trim(), format, null).ToString("yyyy-MM-dd");
            }
            catch (Exception)
            {
                dateFromError = "* Date wrong format";
                return;
            }
            try
            {
                toDate = DateTime.ParseExact(dtpToDate.Text.Trim(), format, null).ToString("yyyy-MM-dd");
            }
            catch (Exception)
            {
                dateToError = "* Date wrong format";
                return;
            }

            var reyable = "PS_AccountPayableReport";

            List<Procedure> parameters = new List<Procedure>();
            parameters.Add(item: new Procedure() { field_name = "@pBranch", sql_db_type = MySqlDbType.VarChar, value_name = ddBranchName.SelectedItem.Value });
            parameters.Add(item: new Procedure() { field_name = "@pFRDT", sql_db_type = MySqlDbType.VarChar, value_name = fromDate });
            parameters.Add(item: new Procedure() { field_name = "@pTODT", sql_db_type = MySqlDbType.VarChar, value_name = toDate });



            DataTable Payable = db.getProcedureDataTable(reyable, parameters);
            GenerateReport(Payable);
        }
    }
}