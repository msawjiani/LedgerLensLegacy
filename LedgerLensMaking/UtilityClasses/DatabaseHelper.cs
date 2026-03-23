using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using LedgerLennMaking.Models.Data;
using LedgerLens;
using LedgerLensMaking;
using LedgerLensMaking.Models.Data;
using LedgerLensMaking.Models.UIModels;

public static class DatabaseHelper
{
    public static int GetUnix()
    {
        string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={GlobalVariables.DatabaseFileName};";
        using (var connection = new OleDbConnection(connectionString))
        {
            var command = new OleDbCommand("SELECT MAX(Unix) FROM GeneralLedger", connection);
            connection.Open();
            var result = command.ExecuteScalar();

            return result == DBNull.Value ? 1 : Convert.ToInt32(result) + 1;
        }
    }

    
    public static List<AccountingYear> GetAccountingYears()
        {
        List<AccountingYear> years = new List<AccountingYear>();

        string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={GlobalVariables.DatabaseFileName};";
        using (OleDbConnection connection = new OleDbConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT * FROM AccountYears";
            GlobalVariables.MaxYearId = -1;
            using (OleDbCommand command = new OleDbCommand(query, connection))
            using (OleDbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    AccountingYear year = new AccountingYear
                    {
                        YearId = reader.GetInt32(0),
                        StartDate = reader.GetDateTime(1), 
                        EndDate = reader.GetDateTime(2) 
                        // Map other columns as needed
                    };
                    years.Add(year);
                    if (year.YearId > GlobalVariables.MaxYearId)
                    {
                        GlobalVariables.MaxYearId = year.YearId;
                    }
                }
            }
        }

        return years;
    }
    public static bool DeleteTransactions(int unix)
    {
        bool result = true; // Assume deletion is allowed unless we find a record with '*' in TransactionType.
        string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={GlobalVariables.DatabaseFileName};";

        using (OleDbConnection connection = new OleDbConnection(connectionString))
        {
            connection.Open();
            using (OleDbTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    // Step 1: Check if any transaction in GeneralLedger has a '*' in TransactionType
                    string checkQuery = "SELECT COUNT(*) FROM GeneralLedger WHERE Unix = ? AND TransactionType LIKE '%*%'";
                    using (OleDbCommand checkCommand = new OleDbCommand(checkQuery, connection, transaction))
                    {
                        checkCommand.Parameters.AddWithValue("?", unix);
                        int count = (int)checkCommand.ExecuteScalar();
                        if (count > 0)
                        {
                            // If a record with '*' in TransactionType exists, disallow deletion
                            result = false;
                            transaction.Rollback(); // Rollback transaction and exit
                            return result;
                        }
                    }

                    // Step 2: Delete from SubledgerTrans (if exists)
                    string deleteSubledgerTransQuery = "DELETE FROM SubledgerTrans WHERE Unix = ?";
                    using (OleDbCommand deleteSubledgerTransCommand = new OleDbCommand(deleteSubledgerTransQuery, connection, transaction))
                    {
                        deleteSubledgerTransCommand.Parameters.AddWithValue("?", unix);
                        deleteSubledgerTransCommand.ExecuteNonQuery();
                    }

                    // Step 3: Delete from ShareSales (if exists)
                    string deleteShareSalesQuery = "DELETE FROM ShareSales WHERE Unix = ?";
                    using (OleDbCommand deleteShareSalesCommand = new OleDbCommand(deleteShareSalesQuery, connection, transaction))
                    {
                        deleteShareSalesCommand.Parameters.AddWithValue("?", unix);
                        deleteShareSalesCommand.ExecuteNonQuery();
                    }

                    // Step 4: Delete from SharePurchase (if exists)
                    string deleteSharePurchaseQuery = "DELETE FROM SharePurchases WHERE Unix = ?";
                    using (OleDbCommand deleteSharePurchaseCommand = new OleDbCommand(deleteSharePurchaseQuery, connection, transaction))
                    {
                        deleteSharePurchaseCommand.Parameters.AddWithValue("?", unix);
                        deleteSharePurchaseCommand.ExecuteNonQuery();
                    }

                    // Step 5: Finally, delete from GeneralLedger
                    string deleteGeneralLedgerQuery = "DELETE FROM GeneralLedger WHERE Unix = ?";
                    using (OleDbCommand deleteGeneralLedgerCommand = new OleDbCommand(deleteGeneralLedgerQuery, connection, transaction))
                    {
                        deleteGeneralLedgerCommand.Parameters.AddWithValue("?", unix);
                        deleteGeneralLedgerCommand.ExecuteNonQuery();
                    }

                    // Commit the transaction after all deletions
                    transaction.Commit();
                }
                catch (Exception)
                {
                    // Rollback transaction in case of any errors
                    transaction.Rollback();
                    result = false;
                    throw;
                }
            }
        }

        return result;
    }

    public static bool CheckDeletion(int unix)
    {
        bool result = true; // Assume deletion is allowed unless we find a record with '*' in TransactionType.
        string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={GlobalVariables.DatabaseFileName};";

        using (OleDbConnection connection = new OleDbConnection(connectionString))
        {
            connection.Open();

            string query = "SELECT TransactionType FROM GeneralLedger WHERE Unix = ?"; // Fetch only the TransactionType column.
            using (OleDbCommand command = new OleDbCommand(query, connection))
            {
                command.Parameters.AddWithValue("?", unix); // Add the parameter for Unix value.

                using (OleDbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string transactionType = reader["TransactionType"].ToString();

                        // If any TransactionType contains '*', disallow deletion.
                        if (transactionType.Contains("*"))
                        {
                            result = false;
                            break; // No need to check further; we found a '*' in one of the records.
                        }
                    }
                }
            }
        }
        return result;
    }
    public static List<Chart> GetCreditAccountsForShareJournal(string Avoid)
    {
        List<Chart> charts = new List<Chart>();

        string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={GlobalVariables.DatabaseFileName};";
        using (OleDbConnection connection = new OleDbConnection(connectionString))
        {
            connection.Open();
            string query;
            if (Avoid == "")
                query = "SELECT * FROM Chart where Category='BS' ";
            else if (Avoid=="GetEverything")
                query = "SELECT * FROM Chart Order By Account  ";
            else if (Avoid == "OnlyShares")
                query = "SELECT * FROM Chart WHERE Category='BS' and SubledgerFlag='SH' Order By Account  ";
            else if (Avoid == "Subledger")
                query = "SELECT * From Chart where Category='BS' and SubledgerFlag not in ('SH' , 'SL') Order By Account";
            else if (Avoid == "Other")
                query = "SELECT * FROM CHART where Category='BS' and SubledgerFlag='SL' Order By Account";
            else if (Avoid == "Journal")
                query = "Select * from CHART where Category  <> 'BANK' and SubledgerFlag='NO' Order By Account";
            else
                query = "SELECT * FROM Chart where Category='BS' and SubledgerFlag<>'SH' Order By Account";

            using (OleDbCommand command = new OleDbCommand(query, connection))
            using (OleDbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Chart chart = new Chart()
                    {
                        AccountId = reader.GetInt32(0),
                        Account = reader.GetString(1),
                        Category = reader.GetString(2),
                        SubledgerFlag = reader.GetString(3),
                    };
                    charts.Add(chart);
                }
            }
            return charts;
        }
    }
    public static List<SharesChart> GetSharesChart()
    {
        List<SharesChart> charts = new List<SharesChart>();

        string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={GlobalVariables.DatabaseFileName};";
        using (OleDbConnection connection = new OleDbConnection(connectionString))
        {
            connection.Open();
            string query;

            query = "SELECT * FROM SharesChart Order By Company ";

            using (OleDbCommand command = new OleDbCommand(query, connection))
            using (OleDbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    SharesChart chart = new SharesChart()
                    {
                        ShareId = reader.GetInt32(0),
                        Company = reader.GetString(1),
                        AccountId = reader.GetInt32(2),
                    };
                    charts.Add(chart);
                }
            }
            return charts;
        }
    }
    public static List<QryChartWithBalance> GetGlAccountsForJournal(string Type="Journal")
    {
        List<QryChartWithBalance> charts = new List<QryChartWithBalance>();

        string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={GlobalVariables.DatabaseFileName};";
        using (OleDbConnection connection = new OleDbConnection(connectionString))
        {
            connection.Open();
            string query;
            if(Type=="Journal")
             query = "Select * from QryChartWithBalance where Category  <> 'BANK' and SubledgerFlag='NO' ORDER BY ACCOUNT";
            else 
                query = "Select * from QryChartWithBalance Order By Account";


            using (OleDbCommand command = new OleDbCommand(query, connection))
            using (OleDbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    QryChartWithBalance chart = new QryChartWithBalance()
                    {
                        AccountId = reader.GetInt32(0),
                        Account = reader.GetString(1),
                        Category = reader.GetString(2),
                        SubledgerFlag = reader.GetString(3),
                        Balance = reader.IsDBNull(4) ? 0 : Convert.ToDecimal (reader.GetValue(4)),   
                    };
                    charts.Add(chart);
                }
            }
            return charts;
        }
    }
    public static List<Chart> GetDebitAccountsForShareJournal()
    {
        List<Chart> charts = new List<Chart>();

        string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={GlobalVariables.DatabaseFileName};";
        using (OleDbConnection connection = new OleDbConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT * FROM Chart where SubledgerFlag='SH' Order By Account ";
            using (OleDbCommand command = new OleDbCommand(query, connection))
            using (OleDbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Chart chart = new Chart()
                    {
                        AccountId = reader.GetInt32(0),
                        Account = reader.GetString(1),
                        Category = reader.GetString(2),
                        SubledgerFlag = reader.GetString(3),
                    };
                    charts.Add(chart);
                }
            }
            return charts;
        }
    }

    public static List<Chart> GetBankAccounts()
    {
        List<Chart> charts = new List<Chart>();

        string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={GlobalVariables.DatabaseFileName};";
        using (OleDbConnection connection = new OleDbConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT * FROM Chart where Category='BANK' Order By Account ";
            using (OleDbCommand command = new OleDbCommand(query, connection))
            using (OleDbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Chart chart = new Chart()
                    {
                        AccountId = reader.GetInt32(0),
                        Account = reader.GetString(1),
                        Category = reader.GetString(2),
                        SubledgerFlag = reader.GetString(3),
                    };
                    charts.Add(chart);
                }
            }
            return charts;
        }
    }

    // Make sure that account selected in the BankDropDown does not figure in the GetAccounts
    public static List<Chart> GetAccounts(int BankCode)
    {
        List<Chart> charts = new List<Chart>();

        string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={GlobalVariables.DatabaseFileName};";
        using (OleDbConnection connection = new OleDbConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT * FROM Chart WHERE AccountId <> ? order by account";
            using (OleDbCommand command = new OleDbCommand(query, connection))
            {
                // Add the parameter in the correct order
                command.Parameters.AddWithValue("?", BankCode);

                using (OleDbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Chart chart = new Chart()
                        {
                            AccountId = reader.GetInt32(0),
                            Account = reader.GetString(1),
                            Category = reader.GetString(2),
                            SubledgerFlag = reader.GetString(3),
                        };
                        charts.Add(chart);
                    }
                }
            }
            return charts;
        }
    }
    public static List<QrySubledgerChart> GetSubledgers(int AccountCode)
    {
        List<QrySubledgerChart> qrySubledgerFDs = new List<QrySubledgerChart>();

        string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={GlobalVariables.DatabaseFileName};";
        using (OleDbConnection connection = new OleDbConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT * FROM QrySubledgerChart WHERE AccountId = ? And Active='Y'";
            using (OleDbCommand command = new OleDbCommand(query, connection))
            {
                // Add the parameter in the correct order
                command.Parameters.AddWithValue("?", AccountCode);

                using (OleDbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        QrySubledgerChart subledgerFDBalance = new QrySubledgerChart()
                        {
                            SubledgerId = reader.GetInt32(0),
                            AccountId = reader.GetInt32(1),
                            Subaccount = reader.GetString(2),
                            Active = reader.GetString(3),
                            TotalFD = reader.IsDBNull(4) ? 0 : Convert.ToInt32(reader.GetValue(4)),

                        };
                        qrySubledgerFDs.Add(subledgerFDBalance);
                    }
                }
            }
            return qrySubledgerFDs;
        }
    }
    public static List<QrySubledgerFDBalance> GetAllFDBalances()
    {
        List<QrySubledgerFDBalance> charts = new List<QrySubledgerFDBalance>();

        string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={GlobalVariables.DatabaseFileName};";
        using (OleDbConnection connection = new OleDbConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT * FROM QrySubledgerFDBalance where TotalFD>0 Order By AccountId,SubledgerId ";
            using (OleDbCommand command = new OleDbCommand(query, connection))
            using (OleDbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    QrySubledgerFDBalance chart = new QrySubledgerFDBalance()
                    {
                        Account = reader.GetString(0),
                        SubledgerId = reader.GetInt32(1),
                        AccountId = reader.GetInt32(2),
                        Subaccount = reader.GetString(3),
                        TotalFD = reader.IsDBNull(4) ? 0 : Convert.ToInt32(reader.GetValue(4)),
                    };
                    charts.Add(chart);
                }
            }
            return charts;
        }


    }

    public static List<QrySubledgerFDBalance> GetFDBalances(int AccountCode)
    {
        List<QrySubledgerFDBalance> qrySubledgerFDs = new List<QrySubledgerFDBalance>();

        string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={GlobalVariables.DatabaseFileName};";
        using (OleDbConnection connection = new OleDbConnection(connectionString))
        {
            connection.Open();
    
            string    query = "SELECT * FROM QrySubledgerFDBalance WHERE AccountId = ? And TotalFD>0";
            using (OleDbCommand command = new OleDbCommand(query, connection))
            {
                // Add the parameter in the correct order

                    command.Parameters.AddWithValue("?", AccountCode);
                

                using (OleDbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        QrySubledgerFDBalance subledgerFDBalance = new QrySubledgerFDBalance()
                        {
                           Account = reader.GetString(0),
                           SubledgerId = reader.GetInt32(1),
                           AccountId =reader.GetInt32(2),
                           Subaccount = reader.GetString(3),
                           TotalFD = reader.GetDecimal(4),

                        };
                        qrySubledgerFDs.Add(subledgerFDBalance);
                    }
                }
            }
            return qrySubledgerFDs;
        }
    }
    public static List<SharesChart> GetShares(int AccountCode)
    {
        List<SharesChart> qryShares = new List<SharesChart>();

        string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={GlobalVariables.DatabaseFileName};";
        using (OleDbConnection connection = new OleDbConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT * FROM SharesChart WHERE AccountId = ? ORDER BY COMPANY";
            using (OleDbCommand command = new OleDbCommand(query, connection))
            {
                // Add the parameter in the correct order
                command.Parameters.AddWithValue("?", AccountCode);

                using (OleDbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SharesChart shareBalance = new SharesChart()
                        {

                            ShareId = reader.GetInt32(0),
                            Company = reader.GetString(1),
                            AccountId = reader.GetInt32(2),


                        };
                        qryShares.Add(shareBalance);
                    }
                }
            }
            return qryShares;
        }
    }
    public static List<QryShareBalance> GetSharesAtCost()
    {
        string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={GlobalVariables.DatabaseFileName};";
        List<QryShareBalance> qryShares = new List<QryShareBalance>();
        try
        {
            
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                string query = "SELECT * FROM QryShareBalance ORDER BY Account,Company,PurchaseDate";
                OleDbCommand command = new OleDbCommand(query, connection);


                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        QryShareBalance shareBalance = new QryShareBalance();

                        try
                        {
                            shareBalance.YearId = reader.GetInt32(reader.GetOrdinal("YearId"));
                        }
                        catch (Exception ex) { Console.WriteLine("Error reading YearId: " + ex.Message); }

                        try
                        {
                            shareBalance.StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate"));
                        }
                        catch (Exception ex) { Console.WriteLine("Error reading StartDate: " + ex.Message); }

                        try
                        {
                            shareBalance.EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate"));
                        }
                        catch (Exception ex) { Console.WriteLine("Error reading EndDate: " + ex.Message); }

                        try
                        {
                            shareBalance.AccountId = reader.GetInt32(reader.GetOrdinal("AccountId"));
                        }
                        catch (Exception ex) { Console.WriteLine("Error reading AccountId: " + ex.Message); }

                        try
                        {
                            shareBalance.Account = reader.GetString(reader.GetOrdinal("Account"));
                        }
                        catch (Exception ex) { Console.WriteLine("Error reading Account: " + ex.Message); }

                        try
                        {
                            shareBalance.ShareId = reader.GetInt32(reader.GetOrdinal("ShareId"));
                        }
                        catch (Exception ex) { Console.WriteLine("Error reading ShareId: " + ex.Message); }

                        try
                        {
                            shareBalance.Company = reader.GetString(reader.GetOrdinal("Company"));
                        }
                        catch (Exception ex) { Console.WriteLine("Error reading Company: " + ex.Message); }

                        try
                        {
                            shareBalance.TransactionId = reader.GetInt32(reader.GetOrdinal("TransactionId"));
                        }
                        catch (Exception ex) { Console.WriteLine("Error reading TransactionId: " + ex.Message); }

                        try
                        {
                            shareBalance.TDate = reader.GetDateTime(reader.GetOrdinal("TDate"));
                        }
                        catch (Exception ex) { Console.WriteLine("Error reading TDate: " + ex.Message); }

                        try
                        {
                            shareBalance.SharePurchaseTdate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate"));
                        }
                        catch (Exception ex) { Console.WriteLine("Error reading PurchaseDate: " + ex.Message); }

                        try
                        {
                            shareBalance.Ref = reader.GetString(reader.GetOrdinal("Ref"));
                        }
                        catch (Exception ex) { Console.WriteLine("Error reading Ref: " + ex.Message); }

                        try
                        {
                            shareBalance.Particulars = reader.GetString(reader.GetOrdinal("Particulars"));
                        }
                        catch (Exception ex) { Console.WriteLine("Error reading Particulars: " + ex.Message); }

                        try
                        {
                            shareBalance.Amount = reader.GetDecimal(reader.GetOrdinal("Amount"));
                        }
                        catch (Exception ex) { Console.WriteLine("Error reading Amount: " + ex.Message); }

                        try
                        {
                            shareBalance.QtyPurchased = reader.IsDBNull(reader.GetOrdinal("QtyPurchased")) ? 0.0M : Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("QtyPurchased")));
                        }
                        catch (Exception ex) { Console.WriteLine("Error reading QtyPurchased: " + ex.Message); }

                        try
                        {
                            shareBalance.QS = reader.IsDBNull(reader.GetOrdinal("QS")) ? 0.0M : Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("QS")));
                        }
                        catch (Exception ex) { Console.WriteLine("Error reading QS: " + ex.Message); }

                        try
                        {
                            shareBalance.PurchaseRate = reader.GetDecimal(reader.GetOrdinal("PurchaseRate"));
                        }
                        catch (Exception ex) { Console.WriteLine("Error reading PurchaseRate: " + ex.Message); }

                        try
                        {
                            shareBalance.Balance = reader.IsDBNull(reader.GetOrdinal("Balance")) ? 0.0M : Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("Balance")));
                        }
                        catch (Exception ex) { Console.WriteLine("Error reading Balance: " + ex.Message); }

                        try
                        {
                            shareBalance.AtCost = reader.IsDBNull(reader.GetOrdinal("AtCost")) ? 0.0M : Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("AtCost")));
                        }
                        catch (Exception ex) { Console.WriteLine("Error reading AtCost: " + ex.Message); }

                        try
                        {
                            shareBalance.PurchaseTransId = reader.GetInt32(reader.GetOrdinal("PurchaseTransId"));
                        }
                        catch (Exception ex) { Console.WriteLine("Error reading PurchaseTransId: " + ex.Message); }

                        qryShares.Add(shareBalance);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
            Console.WriteLine("Stack Trace: " + ex.StackTrace);
        }



        return qryShares;
    }

    public static List<QryCapgainProfit> GetCapitalGainsStatement(int YearId)
    {
        List<QryCapgainProfit> qryShares = new List<QryCapgainProfit>();
        string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={GlobalVariables.DatabaseFileName};";
        using (OleDbConnection connection = new OleDbConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT * FROM QryCapgainProfit3 where YearId=? ORDER BY AccountId,ShareId,PurchaseDate ";
            using (OleDbCommand command = new OleDbCommand(query, connection))
            {
                // Add the parameter in the correct order
                command.Parameters.AddWithValue("?", YearId);

                using (OleDbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        QryCapgainProfit shareBalance = new QryCapgainProfit                        
                        {
                            ShareId = reader.GetInt32(reader.GetOrdinal("ShareId")),
                            YearId = reader.GetInt32(reader.GetOrdinal("YearId")),
                            Company = reader.GetString(reader.GetOrdinal("Company")),
                            QtyPurchased = reader.GetDecimal(reader.GetOrdinal("QtyPurchased")),
                            PurchaseRate = reader.GetDecimal(reader.GetOrdinal("PurchaseRate")),
                            GLAmount =  reader.GetDecimal(reader.GetOrdinal("PGLAmt")),
                            PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                            SaleDate = reader.GetDateTime(reader.GetOrdinal("SaleDate")),
                            QtySold = reader.GetDecimal(reader.GetOrdinal("QtySold")),
                            SellingPrice = reader.GetDecimal(reader.GetOrdinal("SellingPrice")),
                            ProfitOrLoss = reader.GetDecimal(reader.GetOrdinal("ProfitLoss")),
                            Months=reader.GetInt32(reader.GetOrdinal("Months")),
                            AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
                            Account = reader.GetString(reader.GetOrdinal("Account")),
                        };

                        qryShares.Add(shareBalance);
                    }


                }
            }
            return qryShares;
        }
    }
    public static List<QryShareBalanceNett> GetShareBalances(int AccountCode)
    {
        List<QryShareBalanceNett> qryShares = new List<QryShareBalanceNett>();

        string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={GlobalVariables.DatabaseFileName};";
        using (OleDbConnection connection = new OleDbConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT * FROM QryShareBalanceNett WHERE AccountId = ? and ts <> 0 order by company";
            using (OleDbCommand command = new OleDbCommand(query, connection))
            {
                // Add the parameter in the correct order
                command.Parameters.AddWithValue("?", AccountCode);
                Debug.WriteLine(query.ToString());
                using (OleDbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        QryShareBalanceNett shareBalance = new QryShareBalanceNett()
                        {
                            
                            ShareId = reader.GetInt32(0),
                            AccountId = reader.GetInt32(1),
                            Company = reader.GetString(2),
                            TS = reader.IsDBNull(3) ? 0 : Convert.ToDecimal(reader.GetValue(3))

                        };
                        qryShares.Add(shareBalance);
                    }
                }
            }
            return qryShares;
        }
    }

    public static List<BankEntryGridModel> GetGridDetails(int BankCode,int YearId)
    {
        List<BankEntryGridModel> bankEntryGridData = new List<BankEntryGridModel>();

        string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={GlobalVariables.DatabaseFileName};";
        using (OleDbConnection connection = new OleDbConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT * FROM GeneralLedger WHERE AccountId = ? and YearID=? Order By Tdate,Unix";
            using (OleDbCommand command = new OleDbCommand(query, connection))
            {
                // Add the parameter in the correct order
                command.Parameters.AddWithValue("?", BankCode);
                command.Parameters.AddWithValue("?",YearId);

                using (OleDbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        BankEntryGridModel bankEntryGridModel = new BankEntryGridModel()
                        {
                            TransactionId = reader.GetInt32(0),
                            AccountId = reader.GetInt32(1),
                            
                            Unix = reader.GetInt32(3),
                            TDate = reader.GetDateTime(4),
                            Ref =reader.GetString(5),
                            Particulars = reader.GetString(6),
                            Amount = reader.GetDecimal(7),
                            Narration=reader.GetString(9),
                            RunningBalance = 0
                    
                        };
                        bankEntryGridData.Add(bankEntryGridModel);
                    }
                }
            }
            return bankEntryGridData;
        }
    }
    public static List<ReportLegerModel> GetLedgerAccount(int AccountCode, int YearId,string Type)
    {
        List<ReportLegerModel> reportLegers = new List<ReportLegerModel>();

        string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={GlobalVariables.DatabaseFileName};";
        using (OleDbConnection connection = new OleDbConnection(connectionString))
        {
            connection.Open();
            string query;
            if (Type == "Single")
                query = "SELECT * FROM qryGeneralLedger WHERE AccountId = ? and YearID=? Order By Tdate,Unix";
            else
                query = "SELECT * FROM qryGeneralLedger WHERE YearID=? Order By Account, Tdate,Unix";
            using (OleDbCommand command = new OleDbCommand(query, connection))
            {
                // Add the parameter in the correct order
                if (Type == "Single")
                {
                    command.Parameters.AddWithValue("?", AccountCode);
                    command.Parameters.AddWithValue("?", YearId);
                }
                else
                {
                    command.Parameters.AddWithValue("?", YearId);
                }

                using (OleDbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ReportLegerModel model = new ReportLegerModel()
                        {
                            YearId = reader.GetInt32(0),
                            AccountId = reader.GetInt32(1),
                            Account = reader.GetString(2),
                            Category = reader.GetString(3),
                            Unix = reader.GetInt32(4),
                            TDate = reader.GetDateTime(5),
                            Ref = reader.GetString(6),
                            Particulars = reader.GetString(7),
                            Amount = reader.GetDecimal(8),
                            TransactionType = reader.GetString(9),
                            Narration = reader.GetString(10),
                            RunningBalance = 0

                        };
                        reportLegers.Add(model);
                    }
                }
            }
            return reportLegers;
        }
    }


    public static List<Chart> GetCharts()
    {
        List<Chart> charts = new List<Chart>();

        string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={GlobalVariables.DatabaseFileName};";
        using (OleDbConnection connection = new OleDbConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT * FROM Chart Order By Account";
            using (OleDbCommand command = new OleDbCommand(query, connection))
            using (OleDbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Chart chart = new Chart()
                    {
                        AccountId = reader.GetInt32(0),
                        Account=reader.GetString(1),
                        Category=reader.GetString(2),
                        SubledgerFlag = reader.GetString(3),
                    };
                    charts.Add(chart);
                }
            }
            return charts;
        }
    }
    public static void SaveChart(Chart chart)
    {
        string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={GlobalVariables.DatabaseFileName};";
        using (OleDbConnection connection = new OleDbConnection(connectionString))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Chart (Account, Category, SubledgerFlag)
                VALUES (@Account, @Category, @SubledgerFlag)";

            command.Parameters.AddWithValue("@Account", chart.Account);
            command.Parameters.AddWithValue("@Category", chart.Category);
            command.Parameters.AddWithValue("@SubledgerFlag", chart.SubledgerFlag);

            command.ExecuteNonQuery();
        }
    }
    public static void UdateChart(Chart chart)
    {
        string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={GlobalVariables.DatabaseFileName};";
        using (OleDbConnection connection = new OleDbConnection(connectionString))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
            UPDATE CHART 
            SET Account = @Account, 
                Category = @Category, 
                SubledgerFlag = @SubledgerFlag
            WHERE AccountId = @AccountId";

            command.Parameters.AddWithValue("@Account", chart.Account);
            command.Parameters.AddWithValue("@Category", chart.Category);
            command.Parameters.AddWithValue("@SubledgerFlag", chart.SubledgerFlag);
            command.Parameters.AddWithValue("@AccountId", chart.AccountId); // Assuming AccountId is the key

            command.ExecuteNonQuery();
        }
    
    }
    public static void SaveCompany(SharesChart chart)
    {
        string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={GlobalVariables.DatabaseFileName};";
        using (OleDbConnection connection = new OleDbConnection(connectionString))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO SharesChart (Company, AccountId)
                VALUES (@Company, @AccountId )";

            command.Parameters.AddWithValue("@Company", chart.Company);
            command.Parameters.AddWithValue("@AccountId", chart.AccountId);
            

            command.ExecuteNonQuery();
        }
    }

    public static void UpdateCompany(SharesChart chart)
    {
        string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={GlobalVariables.DatabaseFileName};";
        using (OleDbConnection connection = new OleDbConnection(connectionString))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
            UPDATE SHARESCHART 
            SET Company = @Company, 
                AccountId = @AccountId

            WHERE ShareId = @ShareId";

            command.Parameters.AddWithValue("@Company", chart.Company);
            command.Parameters.AddWithValue("@AccountId", chart.AccountId);
            
            command.Parameters.AddWithValue("@ShareId", chart.ShareId); // Assuming AccountId is the key

            command.ExecuteNonQuery();
        }

    }
    public static void UpdateSubledger(SubledgerChart chart)
    {
        string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={GlobalVariables.DatabaseFileName};";
        using (OleDbConnection connection = new OleDbConnection(connectionString))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
            UPDATE SubledgerChart 
            SET Subaccount = @Subaccount, 
                AccountId = @AccountId

            WHERE SubledgerId = @SubledgerId";

            command.Parameters.AddWithValue("@Subaccount", chart.Subaccount);
            command.Parameters.AddWithValue("@AccountId", chart.AccountId);

            command.Parameters.AddWithValue("@SubledgerId", chart.SubledgerId); // Assuming AccountId is the key

            command.ExecuteNonQuery();
        }

    }
    public static void SaveSubledger(SubledgerChart chart)
    {
        string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={GlobalVariables.DatabaseFileName};";
        using (OleDbConnection connection = new OleDbConnection(connectionString))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO SubledgerChart (Subaccount, AccountId)
                VALUES (@Subaccount, @AccountId )";

            command.Parameters.AddWithValue("@Subaccount", chart.Subaccount);
            command.Parameters.AddWithValue("@AccountId", chart.AccountId);


            command.ExecuteNonQuery();
        }
    }

    public static List<Chart> GetGLAccountForSharesChart()
    {
        List<Chart> charts = new List<Chart>();

        string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={GlobalVariables.DatabaseFileName};";
        using (OleDbConnection connection = new OleDbConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT * FROM Chart where SubledgerFlag='SH'";
            using (OleDbCommand command = new OleDbCommand(query, connection))
            using (OleDbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Chart chart = new Chart()
                    {
                        AccountId = reader.GetInt32(0),
                        Account = reader.GetString(1),
                        Category = reader.GetString(2),
                        SubledgerFlag = reader.GetString(3),
                    };
                    charts.Add(chart);
                }
            }
            return charts;
        }
    }
    public static List<SharesChart> GetCompaniesForGLAccount(int AccountId)
    {
        List<SharesChart> sharescharts = new List<SharesChart>();

        string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={GlobalVariables.DatabaseFileName};";
        using (OleDbConnection connection = new OleDbConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT * FROM SharesChart WHERE AccountId = @AccountId Order By Company";
            using (OleDbCommand command = new OleDbCommand(query, connection))
            {
                // Add the parameter to the command
                command.Parameters.AddWithValue("@AccountId", AccountId);

                using (OleDbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SharesChart sharesChart = new SharesChart()
                        {
                            ShareId = reader.GetInt32(0),
                            Company = reader.GetString(1),
                            AccountId = reader.GetInt32(2),
                        };
                        sharescharts.Add(sharesChart);
                    }
                }
            }
        }
        return sharescharts;
    }
    public static List<Chart> GetGLAccountForSubledgerChart()
    {
        List<Chart> charts = new List<Chart>();

        string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={GlobalVariables.DatabaseFileName};";
        using (OleDbConnection connection = new OleDbConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT * FROM Chart where SubledgerFlag='SL'";
            using (OleDbCommand command = new OleDbCommand(query, connection))
            using (OleDbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Chart chart = new Chart()
                    {
                        AccountId = reader.GetInt32(0),
                        Account = reader.GetString(1),
                        Category = reader.GetString(2),
                        SubledgerFlag = reader.GetString(3),
                    };
                    charts.Add(chart);
                }
            }
            return charts;
        }
    }
    public static List<SubledgerChart> GetSubledgerForGLAccount(int AccountId)
    {
        List<SubledgerChart> subledgercharts = new List<SubledgerChart>();

        string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={GlobalVariables.DatabaseFileName};";
        using (OleDbConnection connection = new OleDbConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT * FROM SubledgerChart WHERE AccountId = @AccountId Order By Subaccount";
            using (OleDbCommand command = new OleDbCommand(query, connection))
            {
                // Add the parameter to the command
                command.Parameters.AddWithValue("@AccountId", AccountId);

                using (OleDbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SubledgerChart subledgerChart = new SubledgerChart()
                        {
                            SubledgerId = reader.GetInt32(0),
                            AccountId = reader.GetInt32(1),
                            Subaccount = reader.GetString(2),
                            
                        };
                        subledgercharts.Add(subledgerChart);
                    }
                }
            }
        }
        return subledgercharts;
    }
}
