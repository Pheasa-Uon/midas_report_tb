using Microsoft.Reporting.WebForms;
using Report.Models;
using Report.Utils;
using System;
using System.Collections.Generic;
using System.Data;

namespace Report.Accounting
{
    public partial class ChartOfAccount : System.Web.UI.Page
    {
        private DBConnect db = new DBConnect();
        static List<Currency> currencyList;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DataHelper.checkLoginSession();
                DataHelper.populateBranchDDL(ddBranchName, DataHelper.getUserId());
                currencyList = DataHelper.populateCurrencyDDL(ddCurrency);
            }
        }
        
        private void GenerateReport(DataTable trialbalanceDT)
        {
            ReportParameterCollection reportParameters = new ReportParameterCollection();
            reportParameters.Add(new ReportParameter("Branch", ddBranchName.SelectedItem.Text));
            reportParameters.Add(new ReportParameter("Currency", ddCurrency.SelectedItem.Text));

            var _chartOfAccount = new ReportDataSource("COADS", trialbalanceDT);
            DataHelper.generateAccountingReport(ReportViewer1, "ChartOfAccount", reportParameters, _chartOfAccount);
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            var chartOfAccountSql = "";
            if (ddBranchName.SelectedItem.Value == "ALL")
            {
                chartOfAccountSql = "SELECT * FROM " +
                                    "(SELECT coal.id, coal.is_leaf, coal.`parent_acc_id`, coal.gl_name, coal.gl, bch.`branch_code`, bch.`appr`, ccc.`currency_code`, ccc.`currency`, aac.`class_name`, aact.`account_type`, coal.side, " +
                                    "CASE WHEN TRIM(coal.side)= 'Debit' AND coal.balance < 0 THEN coal.balance * (-1)  " +
                                    "	WHEN TRIM(coal.side)= 'Credit' AND coal.balance < 0 THEN coal.balance * (-1) ELSE 0 END debit, " +
                                    "CASE WHEN TRIM(coal.side)= 'Credit' AND coal.balance >= 0 THEN coal.balance  " +
                                    "	WHEN TRIM(coal.side) = 'Debit' AND coal.balance > 0 THEN coal.balance ELSE 0 END credit, " +
                                    "CASE WHEN TRIM(coal.side)= 'Debit' AND coal.balance < 0 THEN coal.balance * (-1) " +
                                    "	WHEN TRIM(coal.side)= 'Credit' AND coal.balance < 0 THEN coal.balance * (-1) " +
                                    "	WHEN TRIM(coal.side)= 'Credit' AND coal.balance >= 0 THEN coal.balance  " +
                                    "	WHEN TRIM(coal.side) = 'Debit' AND coal.balance > 0 THEN coal.balance ELSE 0 END balance " +
                                    "FROM acc_chat_of_account coal " +
                                    "LEFT JOIN acc_chat_of_account coam ON coal.`parent_acc_id` = coam.`id` " +
                                    "LEFT JOIN branch bch ON coal.`branch_id` = bch.id " +
                                    "LEFT JOIN currency ccc ON coal.`currency_id` = ccc.id " +
                                    "LEFT JOIN acc_account_class aac ON coal.`acc_class_id` = aac.`id` " +
                                    "LEFT JOIN acc_account_type aact ON coal.`acc_type_id` = aact.id " +
                                    "WHERE coal.`parent_acc_id` IS NOT NULL and coal.`currency_id` = " + ddCurrency.SelectedItem.Value + " and coal.b_status = '1' " +
                                    "UNION ALL " +
                                    "SELECT coaa.id,coaa.is_leaf, coaa.parent_acc_id, coaa.gl_name, coaa.gl, bch.`branch_code`, bch.`appr`, ccc.`currency_code`, ccc.`currency`, aac.`class_name`, aact.`account_type`,coaa.side, " +
                                    "CASE WHEN TRIM(coaa.side)= 'Debit' AND coaa.balance < 0 THEN coaa.balance * (-1)  " +
                                    "	WHEN TRIM(coaa.side)= 'Credit' AND coaa.balance < 0 THEN coaa.balance * (-1) ELSE 0 END debit, " +
                                    "CASE WHEN TRIM(coaa.side)= 'Credit' AND coaa.balance >= 0 THEN coaa.balance  " +
                                    "	WHEN TRIM(coaa.side) = 'Debit' AND coaa.balance > 0 THEN coaa.balance ELSE 0 END credit, " +
                                    "CASE WHEN TRIM(coaa.side)= 'Debit' AND coaa.balance < 0 THEN coaa.balance * (-1)  " +
                                    "	WHEN TRIM(coaa.side)= 'Credit' AND coaa.balance < 0 THEN coaa.balance * (-1)  " +
                                    "	WHEN TRIM(coaa.side)= 'Credit' AND coaa.balance >= 0 THEN coaa.balance  " +
                                    "	WHEN TRIM(coaa.side) = 'Debit' AND coaa.balance > 0 THEN coaa.balance ELSE 0 END balance " +
                                    "FROM acc_chat_of_account coaa " +
                                    "LEFT JOIN branch bch ON coaa.`branch_id` = bch.id " +
                                    "LEFT JOIN currency ccc ON coaa.`currency_id` = ccc.id " +
                                    "LEFT JOIN acc_account_class aac ON coaa.`acc_class_id` = aac.`id` " +
                                    "LEFT JOIN acc_account_type aact ON coaa.`acc_type_id` = aact.id " +
                                    "WHERE coaa.`parent_acc_id` IS NULL and coaa.`currency_id` = " + ddCurrency.SelectedItem.Value + " and coaa.b_status = '1' " +
                                    ") A ORDER BY A.currency_code DESC, A.gl ASC ";
            }
            else
            {
                chartOfAccountSql = "SELECT * FROM " +
                                    "(SELECT coal.id,coal.is_leaf, coal.`parent_acc_id`, coal.gl_name, coal.gl, bch.`branch_code`, bch.`appr`, ccc.`currency_code`, ccc.`currency`, aac.`class_name`, aact.`account_type`, coal.side, " +
                                    "CASE WHEN TRIM(coal.side)= 'Debit' AND coal.balance < 0 THEN coal.balance * (-1)  " +
                                    "	WHEN TRIM(coal.side)= 'Credit' AND coal.balance < 0 THEN coal.balance * (-1) ELSE 0 END debit, " +
                                    "CASE WHEN TRIM(coal.side)= 'Credit' AND coal.balance >= 0 THEN coal.balance  " +
                                    "	WHEN TRIM(coal.side) = 'Debit' AND coal.balance > 0 THEN coal.balance ELSE 0 END credit, " +
                                    "CASE WHEN TRIM(coal.side)= 'Debit' AND coal.balance < 0 THEN coal.balance * (-1) " +
                                    "	WHEN TRIM(coal.side)= 'Credit' AND coal.balance < 0 THEN coal.balance * (-1) " +
                                    "	WHEN TRIM(coal.side)= 'Credit' AND coal.balance >= 0 THEN coal.balance  " +
                                    "	WHEN TRIM(coal.side) = 'Debit' AND coal.balance > 0 THEN coal.balance ELSE 0 END balance " +
                                    "FROM acc_chat_of_account coal " +
                                    "LEFT JOIN acc_chat_of_account coam ON coal.`parent_acc_id` = coam.`id` " +
                                    "LEFT JOIN branch bch ON coal.`branch_id` = bch.id " +
                                    "LEFT JOIN currency ccc ON coal.`currency_id` = ccc.id " +
                                    "LEFT JOIN acc_account_class aac ON coal.`acc_class_id` = aac.`id` " +
                                    "LEFT JOIN acc_account_type aact ON coal.`acc_type_id` = aact.id " +
                                    "WHERE coal.branch_id = " + ddBranchName.SelectedValue + " and coal.`currency_id` = " + ddCurrency.SelectedItem.Value + " AND coal.`parent_acc_id` IS NOT NULL and coal.b_status = '1' " +
                                    "UNION ALL " +
                                    "SELECT coaa.id,coaa.is_leaf, coaa.parent_acc_id, coaa.gl_name, coaa.gl, bch.`branch_code`, bch.`appr`, ccc.`currency_code`, ccc.`currency`, aac.`class_name`, aact.`account_type`, coaa.side, " +
                                    "CASE WHEN TRIM(coaa.side)= 'Debit' AND coaa.balance < 0 THEN coaa.balance * (-1)  " +
                                    "	WHEN TRIM(coaa.side)= 'Credit' AND coaa.balance < 0 THEN coaa.balance * (-1) ELSE 0 END debit, " +
                                    "CASE WHEN TRIM(coaa.side)= 'Credit' AND coaa.balance >= 0 THEN coaa.balance  " +
                                    "	WHEN TRIM(coaa.side) = 'Debit' AND coaa.balance > 0 THEN coaa.balance ELSE 0 END credit, " +
                                    "CASE WHEN TRIM(coaa.side)= 'Debit' AND coaa.balance < 0 THEN coaa.balance * (-1)  " +
                                    "	WHEN TRIM(coaa.side)= 'Credit' AND coaa.balance < 0 THEN coaa.balance * (-1)  " +
                                    "	WHEN TRIM(coaa.side)= 'Credit' AND coaa.balance >= 0 THEN coaa.balance  " +
                                    "	WHEN TRIM(coaa.side) = 'Debit' AND coaa.balance > 0 THEN coaa.balance ELSE 0 END balance " +
                                    "FROM acc_chat_of_account coaa " +
                                    "LEFT JOIN branch bch ON coaa.`branch_id` = bch.id " +
                                    "LEFT JOIN currency ccc ON coaa.`currency_id` = ccc.id " +
                                    "LEFT JOIN acc_account_class aac ON coaa.`acc_class_id` = aac.`id` " +
                                    "LEFT JOIN acc_account_type aact ON coaa.`acc_type_id` = aact.id " +
                                    "WHERE coaa.branch_id = " + ddBranchName.SelectedValue + " and coaa.`currency_id` = " + ddCurrency.SelectedItem.Value + "AND coaa.`parent_acc_id` IS NULL and coaa.b_status = '1' " +
                                    ") A ORDER BY A.currency_code DESC, A.gl ASC ";
            }
            DataTable chartOfAccountDT = db.getDataTable(chartOfAccountSql);
            GenerateReport(chartOfAccountDT);
        }
    }
}