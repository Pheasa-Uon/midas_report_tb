using Microsoft.Reporting.WebForms;
using Report.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
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
            DataHelper.checkLoginSession();
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
            reportParameters.Add(new ReportParameter("BranchParameter", ddBranchName.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("OfficerParameter", ddOfficer.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("FromDateParameter", DateTime.ParseExact(dtpFromDate.Text, format, null).ToString("dd-MMM-yyyy")));
            reportParameters.Add(new ReportParameter("ToDateParameter", DateTime.ParseExact(dtpToDate.Text, format, null).ToString("dd-MMM-yyyy")));

            var _collectionSheetlist = new ReportDataSource("CollectionSheetDS", collectionSheetDT);
            DataHelper.generateOperationReport(ReportViewer1, "CollectionSheet", reportParameters, _collectionSheetlist);
        }

        protected void btnView_Click(object sender, EventArgs e)
        {   
            //Split Date Time variable
            var fromDateSql = DateTime.ParseExact(dtpFromDate.Text, format, null);
            var fromDay = fromDateSql.ToString("dd");

            var toDateSql = DateTime.ParseExact(dtpToDate.Text, format, null);
            var toDay = toDateSql.ToString("dd");

            var collectionSheetSql = "SELECT ST.ticket_no,CUS.customer_name,CUS.personal_phone,CONCAT_WS(', ',COM.commune_kh,VIL.village_kh),CONCAT(COM.commune_kh,', ',VIL.village_kh) as cus_address, " +
                                    "CUR.currency,PL.principle_less princ_outstanding, CASE WHEN C.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END as due_date, " +
                                    "ST.interest_less,ST.principle_less,ST.penalty_less,P.lob_name " +
                                    "FROM schedule_ticket ST " +
                                    "LEFT JOIN contract C ON ST.contract_id = C.id " +
                                    "LEFT JOIN customer CUS ON C.customer_id = CUS.id " +
                                    "LEFT JOIN commune COM ON CUS.commune_id = COM.id " +
                                    "LEFT JOIN village VIL ON CUS.village_id = VIL.id " +
                                    "LEFT JOIN currency CUR ON C.currency_id = CUR.id " +
                                    "LEFT JOIN product P ON C.product_id = P.id " +
                                    "LEFT JOIN(SELECT contract_id, SUM(principle_less) principle_less " +
                                        "FROM schedule_ticket " +
                                        "WHERE ticket_status != 'P' AND branch_id =" + ddBranchName.SelectedValue + 
                                        " GROUP BY contract_id) PL ON C.id = PL.contract_id " +
                                    " WHERE ST.ticket_status != 'P' AND ST.ticket_status != 'FPP' AND DATE(CASE WHEN C.product_type_id = 1 THEN ST.due_date ELSE ST.created_date END) BETWEEN " + fromDay + " AND " + toDay + 
                                    " AND C.pawn_officer_id = CASE WHEN " + ddOfficer.SelectedValue + " IS NULL THEN C.pawn_officer_id ELSE " + ddOfficer.SelectedValue + " END AND C.branch_id = " + ddBranchName.SelectedValue ;
            
            var collectionSheetDT = db.getDataTable(collectionSheetSql);
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