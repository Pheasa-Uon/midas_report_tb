using Microsoft.Reporting.WebForms;
using Report.Utils;
using System;
using System.Data;

namespace Report.Accounting
{
    public partial class AccrualDetail : System.Web.UI.Page
    {
        private DBConnect db = new DBConnect();
        public string format = "dd/MM/yyyy";
        public string dateError = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DataHelper.checkLoginSession();
                txtContract.Text = "";
                dtpSystemDate.Text = DataHelper.getSystemDate().ToString(format);
            }
        }


        private void GenerateReport(DataTable contractDT, DataTable scheduleDT, DataTable airDT)
        {
            ReportParameterCollection reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("SystemDate", DateTime.ParseExact(dtpSystemDate.Text, format, null).ToString("dd-MMM-yyyy")));

            var contractDS = new ReportDataSource("CONTRACT_DS", contractDT);
            var scheduleDS = new ReportDataSource("SCHEDULE_DS", scheduleDT);
            var airDS = new ReportDataSource("AIR_DS", airDT);
            DataHelper.generateAccountingReport(ReportViewer1, "AccrualDetail", reportParameters, contractDS, scheduleDS, airDS);
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            var searchDate = "";
            try
            {
               searchDate = DateTime.ParseExact(dtpSystemDate.Text, format, null).ToString("yyyy-MM-dd");
            }
            catch (Exception ex)
            {
                dateError = "* Date wrong format!";
                return;
            }

            var contractSQL = "SELECT c.id,b.branch_name, c.`contract_no`,cs.customer_name, " +
                " c.`disbursement_date`,c.`expired_date`,p.`lob_name`, " +
                " c.`pawn_price_approved`, st.balance, " +
                " CASE WHEN c.contract_status = 6 THEN 'Redeem' " +
                " WHEN c.contract_status = 7 THEN 'In-Forfeit' " +
                " WHEN c.contract_status = 4 THEN 'Active' ELSE 'Active' END AS contract_status " +
                " FROM contract c " +
                " INNER JOIN customer cs ON c.`customer_id`= cs.id " +
                " INNER JOIN product p ON c.`product_id`= p.id " +
                " INNER JOIN branch b ON c.branch_id = b.id " +
                " INNER JOIN " +
                " (SELECT contract_id, SUM(principle_less) balance " +
                " FROM schedule_ticket WHERE contract_no = '"+txtContract.Text.Trim()+"') st " +
                " ON c.id = st.contract_id " +
                " WHERE c.`contract_no` = '"+ txtContract.Text.Trim()+ "';";

            var contractDT = db.getDataTable(contractSQL);

            var scheduleSQL = "SELECT due_date, s.principle_due, interest, c.`interest_rate`, num_of_day, round(interest / num_of_day, 2) as accrual_amt " +
                " FROM `schedule` s INNER JOIN contract c ON s.`contract_id`= c.id " +
                " WHERE s.contract_no = '"+txtContract.Text.Trim()+"'; ";

            var scheduleDT = db.getDataTable(scheduleSQL);

            var airSQL = "SELECT sy.`system_date` as sys_date, st.`ticket_no`, st.`serial_number`, " +
                " IF(t.balance_side = 1, t.`amount`, 0) AS DR, " +
                " IF(t.balance_side = 2, t.amount, 0) AS CR " +
                " FROM acc_transaction t INNER JOIN " +
                "         contract c ON t.`contract_id` = c.id INNER JOIN " +
                "          contract_gl_product cg ON(t.`contract_id` = cg.contract_id " +
                "          AND(SUBSTRING(t.gl_code, 6, 8) = cg.gl AND cg.account_element = 'AIR0')) " +
                "          LEFT JOIN `schedule` s ON t.`schedule_id` = s.id " +
                "          LEFT JOIN schedule_ticket st ON t.`contract_id` = st.`contract_id`  " +
                "          AND s.`order_no` = st.`order_no` " +
                " LEFT JOIN system_date sy on sy.id=t.system_date_id " +
                " WHERE t.`trx_status` = 1 " +
                " AND DATE(sy.system_date) <= DATE('" + searchDate + "') " +
                " AND c.`contract_no` = '"+txtContract.Text.Trim()+"'; ";
            var airDT = db.getDataTable(airSQL);

            GenerateReport(contractDT, scheduleDT, airDT);
        }
    }

}