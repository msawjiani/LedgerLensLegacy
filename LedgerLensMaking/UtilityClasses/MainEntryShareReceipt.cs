using LedgerLennMaking.Models.Data;
using LedgerLensMaking.Models.Data;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LedgerLensMaking.UtilityClasses
{
    public class MainEntryShareReceipt : BaseRepository
    {
        private List<QryShareBalance> shareBalances = new List<QryShareBalance>();

        public MainEntryShareReceipt(string connectionString) : base() { }

        public void MakeShareJournals(MainEntryModel mainEntryMain)
        {
            PopulateList(mainEntryMain);
            decimal localQtyOfSharesSold =(decimal) mainEntryMain.MeQtyOfShares;

            foreach (QryShareBalance row in shareBalances)
            {
                decimal balance = row.Balance;

                if (balance <= localQtyOfSharesSold)
                {
                    localQtyOfSharesSold -= balance;
                }
                else
                {
                    balance = localQtyOfSharesSold;
                    localQtyOfSharesSold = 0;
                }

                JournalizeEntries(mainEntryMain, row, balance);

                if (localQtyOfSharesSold == 0)
                    break;
            }
        }

        private void JournalizeEntries(MainEntryModel mainEntryModel, QryShareBalance row, decimal balance)
        {
            if (mainEntryModel.MeShareSellingPrice >= row.PurchaseRate)
            {
                mainEntryModel.MeSharesAtCost = (decimal)balance * row.PurchaseRate;
                DoConsolidatedEntriesProfit(mainEntryModel, row, balance);
            }
            else
            {
                mainEntryModel.MeSharesAtCost = (decimal)balance * row.PurchaseRate;
                DoConsolidatedEntriesLoss(mainEntryModel, row, balance);
            }
        }

        private void DoConsolidatedEntriesLoss(MainEntryModel mainEntryModel, QryShareBalance row, decimal balance)
        {
            int months = GetMonthDifferenceLikeDATEDIFF(row.SharePurchaseTdate, mainEntryModel.MeTDate);
            int capLossAccount = (months >= 12) ? mainEntryModel.MeLTCaplossAccountCode : mainEntryModel.MeSTCaplossAccountCode;

            CreateAndInsertTransaction(mainEntryModel, balance, row, capLossAccount, true);
        }

        private void DoConsolidatedEntriesProfit(MainEntryModel mainEntryModel, QryShareBalance row, decimal balance)
        {
            int months = GetMonthDifferenceLikeDATEDIFF(row.SharePurchaseTdate, mainEntryModel.MeTDate);
            int capGainAccount = (months >= 12) ? mainEntryModel.MeLTCapgainAccountCode : mainEntryModel.MeSTCapgainAccountCode;

            CreateAndInsertTransaction(mainEntryModel, balance, row, capGainAccount, false);
        }

        private void CreateAndInsertTransaction(MainEntryModel mainEntryModel, decimal balance, QryShareBalance row, int account, bool isLoss)
        {
            decimal amount = (decimal)balance * mainEntryModel.MeShareSellingPrice;
            decimal purchaseCost = (decimal)balance * row.PurchaseRate;
            decimal difference = isLoss ? purchaseCost - amount : amount - purchaseCost;

            GeneralLedger tractionOneBank = new GeneralLedger
            {
                AccountId = mainEntryModel.MeDebitAccount,
                YearId = mainEntryModel.MeYearId,
                Unix = mainEntryModel.MeUnix,
                Tdate = mainEntryModel.MeTDate,
                Ref = mainEntryModel.MeRef,
                Particulars = "Consolidated Entry. 1 Bank Entry",
                Amount = amount,
                TransactionType = mainEntryModel.MeTransactionType,
                Narration = $"{mainEntryModel.MeNarration} Sold: {mainEntryModel.MeCompanyName} Qty {balance}@{mainEntryModel.MeShareSellingPrice}"
            };

            GeneralLedger tractionTwoShares = new GeneralLedger
            {
                AccountId = mainEntryModel.MeSubledgerCode,
                YearId = mainEntryModel.MeYearId,
                Unix = mainEntryModel.MeUnix,
                Tdate = mainEntryModel.MeTDate,
                Ref = mainEntryModel.MeRef,
                Particulars = "Consolidated Entry. 2 Shares Account",
                Amount = -purchaseCost,
                TransactionType = mainEntryModel.MeTransactionType,
                Narration = $"{mainEntryModel.MeNarration} Sold: {mainEntryModel.MeCompanyName} Qty {balance}@{mainEntryModel.MeShareSellingPrice}"
            };

            GeneralLedger tractionThreeCap = new GeneralLedger
            {
                AccountId = account,
                YearId = mainEntryModel.MeYearId,
                Unix = mainEntryModel.MeUnix,
                Tdate = mainEntryModel.MeTDate,
                Ref = mainEntryModel.MeRef,
                Particulars = $"Consolidated Entry. 3 {(isLoss ? "Loss" : "Gain")} Entry",
                Amount = isLoss ? difference : -difference,
                TransactionType = mainEntryModel.MeTransactionType,
                Narration = $"{mainEntryModel.MeNarration} Sold: {mainEntryModel.MeCompanyName} Qty {balance}@{mainEntryModel.MeShareSellingPrice}"
            };

            TransactionRepository transactionRepository = new TransactionRepository(_connectionString);
            int transactionBank = transactionRepository.InsertGeneralLedgerTransaction(tractionOneBank);
            int transactionShares = transactionRepository.InsertGeneralLedgerTransaction(tractionTwoShares);
            int transactionCap = transactionRepository.InsertGeneralLedgerTransaction(tractionThreeCap);

            SharesSales subLedgerTrans = new SharesSales
            {
                ShareId = mainEntryModel.MeSharesAccountCodeOfSharesChart,
                PurchaseTransId = row.PurchaseTransId,
                QtySold = (decimal)balance,
                SellingPrice = mainEntryModel.MeShareSellingPrice,
                TransactionId = transactionShares,
                Unix = mainEntryModel.MeUnix
            };

            transactionRepository.InsertShareSaleTransactiion(subLedgerTrans);
        }

        private int GetMonthDifferenceLikeDATEDIFF(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
                throw new ArgumentException("endDate must be after startDate");

            int yearDiff = endDate.Year - startDate.Year;
            int monthDiff = endDate.Month - startDate.Month;

            if (monthDiff < 0)
            {
                yearDiff--;
                monthDiff += 12;
            }

            return yearDiff * 12 + monthDiff;
        }
        private void PopulateList(MainEntryModel mainEntryMain)
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    string query = "SELECT * FROM QryShareBalance WHERE ShareId = ? ORDER BY PurchaseDate";
                    OleDbCommand command = new OleDbCommand(query, connection);
                    command.Parameters.AddWithValue("?", mainEntryMain.MeSharesAccountCodeOfSharesChart);

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

                            shareBalances.Add(shareBalance);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                Console.WriteLine("Stack Trace: " + ex.StackTrace);
            }
        }


    }
}
