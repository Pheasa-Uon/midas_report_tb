using Microsoft.Reporting.WebForms;
using MySql.Data.MySqlClient;
using Report.Models;
using System;
using System.Collections.Generic;
using System.Data;
using Report.Utils;

namespace Report.Accounting
{
    public partial class IncomeStatementByDate : System.Web.UI.Page
    {
        private DBConnect db = new DBConnect();
        public string fromDate, toDate;
        public string format = "dd/MM/yyyy";
        public string dateFromError = "", dateToError = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var date = DataHelper.getSystemDateTextbox();
                DataHelper.checkLoginSession();
                DataHelper.populateBranchDDL(ddBranchName, DataHelper.getUserId());
                dtpFromDate.Text = date;
                dtpToDate.Text = date;
            }
        }

        private void GenerateReport(DataTable PS_INCTRAN_DT, DataTable PS_GOSTRAN_DT, DataTable PS_TTGOSTRAN_DT, DataTable PS_EXPTRAN_DT, DataTable PS_TTNDITRAN_DT, DataTable PS_OTHRETRAN_DT, DataTable PS_INCEXPTRAN_DT, DataTable PS_PLBINCTRAN_DT, DataTable PS_INCTAXTTRAN_DT, DataTable PS_PLTRAN_DT)
        {
            ReportParameterCollection reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("Branch", ddBranchName.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("FromDate", DateTime.ParseExact(dtpFromDate.Text, format, null).ToString("dd-MMM-yyyy")));
            reportParameters.Add(new ReportParameter("ToDate", DateTime.ParseExact(dtpToDate.Text, format, null).ToString("dd-MMM-yyyy")));

            var PS_INCTRAN_DS = new ReportDataSource("PS_INCTRAN", PS_INCTRAN_DT);
            var PS_GOSTRAN_DS = new ReportDataSource("PS_GOSTRAN", PS_GOSTRAN_DT);
            var PS_TTGOSTRAN_DS = new ReportDataSource("PS_TTGOSTRAN", PS_TTGOSTRAN_DT);
            var PS_EXPTRAN_DS = new ReportDataSource("PS_EXPTRAN", PS_EXPTRAN_DT);
            var PS_TTNDITRAN_DS = new ReportDataSource("PS_TTNDITRAN", PS_TTNDITRAN_DT);
            var PS_OTHRETRAN_DS = new ReportDataSource("PS_OTHRETRAN", PS_OTHRETRAN_DT);
            var PS_INCEXPTRAN_DS = new ReportDataSource("PS_INCEXPTRAN", PS_INCEXPTRAN_DT);
            var PS_PLBINCTRAN_DS = new ReportDataSource("PS_PLBINCTRAN", PS_PLBINCTRAN_DT);
            var PS_INCTAXTTRAN_DS = new ReportDataSource("PS_INCTAXTTRAN", PS_INCTAXTTRAN_DT);
            var PS_PLTRAN_DS = new ReportDataSource("PS_PLTRAN", PS_PLTRAN_DT);

            DataHelper.generateAccountingReport(ReportViewer1, "IncomeStatementByDate", reportParameters,
                PS_INCTRAN_DS,
                PS_GOSTRAN_DS,
                PS_TTGOSTRAN_DS,
                PS_EXPTRAN_DS,
                PS_TTNDITRAN_DS,
                PS_OTHRETRAN_DS,
                PS_INCEXPTRAN_DS,
                PS_PLBINCTRAN_DS,
                PS_INCTAXTTRAN_DS,
                PS_PLTRAN_DS
                );
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

            var PS_INCTRAN = "PS_INCTRAN";
            var PS_GOSTRAN = "PS_GOSTRAN";
            var PS_TTGOSTRAN = "PS_TTGOSTRAN";
            var PS_EXPTRAN = "PS_EXPTRAN";
            var PS_TTNDITRAN = "PS_TTNDITRAN";
            var PS_OTHRETRAN = "PS_OTHRETRAN";
            var PS_INCEXPTRAN = "PS_INCEXPTRAN";
            var PS_PLBINCTRAN = "PS_PLBINCTRAN";
            var PS_INCTAXTTRAN = "PS_INCTAXTTRAN";
            var PS_PLTRAN = "PS_PLTRAN";

            List<Procedure> procedureList = new List<Procedure>();
            procedureList.Add(item: new Procedure() { field_name = "@pBranch", sql_db_type = MySqlDbType.VarChar, value_name = ddBranchName.SelectedItem.Value });
            procedureList.Add(item: new Procedure() { field_name = "@FR_Date", sql_db_type = MySqlDbType.Date, value_name = fromDate });
            procedureList.Add(item: new Procedure() { field_name = "@TO_Date", sql_db_type = MySqlDbType.Date, value_name = toDate });

            DataTable PS_INCTRAN_DS = db.getProcedureDataTable(PS_INCTRAN, procedureList);
            DataTable PS_GOSTRAN_DS = db.getProcedureDataTable(PS_GOSTRAN, procedureList);
            DataTable PS_TTGOSTRAN_DS = db.getProcedureDataTable(PS_TTGOSTRAN, procedureList);
            DataTable PS_EXPTRAN_DS = db.getProcedureDataTable(PS_EXPTRAN, procedureList);
            DataTable PS_TTNDITRAN_DS = db.getProcedureDataTable(PS_TTNDITRAN, procedureList);
            DataTable PS_OTHRETRAN_DS = db.getProcedureDataTable(PS_OTHRETRAN, procedureList);
            DataTable PS_INCEXPTRAN_DS = db.getProcedureDataTable(PS_INCEXPTRAN, procedureList);
            DataTable PS_PLBINCTRAN_DS = db.getProcedureDataTable(PS_PLBINCTRAN, procedureList);
            DataTable PS_INCTAXTTRAN_DS = db.getProcedureDataTable(PS_INCTAXTTRAN, procedureList);
            DataTable PS_PLTRAN_DS = db.getProcedureDataTable(PS_PLTRAN, procedureList);

            GenerateReport(PS_INCTRAN_DS, PS_GOSTRAN_DS, PS_TTGOSTRAN_DS, PS_EXPTRAN_DS, PS_TTNDITRAN_DS, PS_OTHRETRAN_DS, PS_INCEXPTRAN_DS, PS_PLBINCTRAN_DS, PS_INCTAXTTRAN_DS, PS_PLTRAN_DS);
        }
    }
}