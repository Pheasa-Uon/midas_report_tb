using Microsoft.Reporting.WebForms;
using System;
using System.Data;
using Report.Utils;
using Report.Models;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace Report.Operation
{
    public partial class AllSourceOfFundAccountList : System.Web.UI.Page
    {
        private DBConnect db = new DBConnect();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DataHelper.checkLoginSession();
                DataHelper.populateBranchDDL(ddBranchName, DataHelper.getUserId());
            }
        }
        private void GenerateReport(DataTable AllSourceOfFundAccountListDT)
        {
            ReportParameterCollection reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("Branch", ddBranchName.SelectedItem.Text));

            var AllSourceOfFundAccountListDS = new ReportDataSource("AllSourceOfFundAccountListDS", AllSourceOfFundAccountListDT);

            DataHelper.generateOperationReport(ReportViewer1, "AllSourceOfFundAccountList", reportParameters, AllSourceOfFundAccountListDS);

        }

        protected void btnView_Click(object sender, EventArgs e)
        {

            var AllSourceOfFundAccountListSQL = "PS_SOFLIST";

            List<Procedure> parameters = new List<Procedure>();
            parameters.Add(item: new Procedure() { field_name = "@pBranch", sql_db_type = MySqlDbType.VarChar, value_name = ddBranchName.SelectedItem.Value });

            DataTable AllSourceOfFundAccountListDT = db.getProcedureDataTable(AllSourceOfFundAccountListSQL, parameters);
            GenerateReport(AllSourceOfFundAccountListDT);
        }
    }
}