using Microsoft.Reporting.WebForms;
using System;
using System.Data;
using Report.Utils;

namespace Report.Operation
{
    public partial class CollectionSheet : System.Web.UI.Page
    {
        private DBConnect db = new DBConnect();
        DateTime currentDate = DateTime.Today;
        private static string fromDate, toDate, systemDateStr;
        public string format = "dd/MM/yyyy";
        protected void Page_Load(object sender, EventArgs e)
        {
            //Convert Date Block
            fromDate = dtpFromDate.Text;
            toDate = dtpToDate.Text;

            if (!IsPostBack)
            {
                DataHelper.populateBranchDDL(ddBranchName, DataHelper.getUserId());
                dtpFromDate.Text = DataHelper.getSystemDateStr();
                dtpToDate.Text = DataHelper.getSystemDateStr();
            }
        }

        private void GenerateReport(DataTable collectionSheetDT)
        {
            ReportParameterCollection reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("Branch", ddBranchName.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("PawnOfficer", ddOfficer.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("FromDate", DateTime.ParseExact(dtpFromDate.Text, format, null).ToString("dd-MMM-yyyy")));
            reportParameters.Add(new ReportParameter("ToDate", DateTime.ParseExact(dtpToDate.Text, format, null).ToString("dd-MMM-yyyy")));

            var _collectionSheetlist = new ReportDataSource("CollectionSheetDS", collectionSheetDT);
            DataHelper.generateOperationReport(ReportViewer1, "CollectionSheet", reportParameters, _collectionSheetlist);
        }

        protected void btnView_Click(object sender, EventArgs e)
        {   
            //Split Date Time variable
            var fromDateSql = DateTime.ParseExact(dtpFromDate.Text, format, null);
            var fromDay = fromDateSql.ToString("yyyy-MM-dd");

            var toDateSql = DateTime.ParseExact(dtpToDate.Text, format, null);
            var toDay = toDateSql.ToString("yyyy-MM-dd");

            var sql = "SELECT ST.ticket_no,CUS.customer_name,CUS.personal_phone, " +
                    "CUR.currency,PL.principle_less princ_outstanding, ST.due_date, " +
                    "ST.interest_less,ST.principle_less,ST.penalty_less, " +
                    "P.lob_name,SI.name CO_Name " +
                    "FROM schedule_ticket ST " +
                    "LEFT JOIN contract C ON ST.contract_id = C.id " +
                    "LEFT JOIN customer CUS ON C.customer_id = CUS.id "+
                    "LEFT JOIN currency CUR ON C.currency_id = CUR.id "+
                    "LEFT JOIN product P ON C.product_id = P.id "+
                    "LEFT JOIN staff_info SI ON C.pawn_officer_id = SI.id "+
                    "LEFT JOIN(SELECT contract_id, SUM(principle_less) principle_less "+
                    "FROM schedule_ticket " +
                    "WHERE ticket_status != 'P' AND branch_id = " + ddBranchName.SelectedItem.Value + " " +
                    "GROUP BY contract_id) PL ON C.id = PL.contract_id " +
                    "WHERE ST.ticket_status != 'P' AND ST.ticket_status != 'FPP' " +
                    "AND DATE(ST.due_date) BETWEEN DATE('" + fromDay + "') AND DATE('" + toDay + "') " +
                   // "AND C.pawn_officer_id = CASE WHEN? IS NULL THEN C.pawn_officer_id ELSE ? END " +
                    "AND C.branch_id = " + ddBranchName.SelectedItem.Value + " AND C.contract_status IN(4, 7) AND c.`b_status`= TRUE " +
                    "AND ST.ticket_status != 'I' ";
            if (ddOfficer.SelectedItem.Value != "0")
            {
                sql += " AND C.pawn_officer_id=" + ddOfficer.SelectedItem.Value + ";"; 
            }
            
            var collectionSheetDT = db.getDataTable(sql);
            GenerateReport(collectionSheetDT);
        }

        protected void ddBranchName_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddBranchName.SelectedValue != "")
            {
                ddOfficer.Enabled = true;
                DataHelper.populateOfficerDDL(ddOfficer, Convert.ToInt32(ddBranchName.SelectedValue));
            }
            else
            {
                ddOfficer.Enabled = false;
                ddOfficer.SelectedItem.Text = "";
            }
        }
    }
}