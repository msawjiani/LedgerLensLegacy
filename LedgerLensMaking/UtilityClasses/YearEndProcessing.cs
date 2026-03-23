using LedgerLensMaking.Models.Data;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LedgerLensMaking.UtilityClasses
{
    public class YearEndProcessing : BaseRepository
    {
        private List<AccountingYear> accountingYears = new List<AccountingYear>();
        private List<QryTrailBalance> qryTrailBalances = new List<QryTrailBalance>();

        private DateTime GlobalNextYearStart;
        private DateTime GlobalNextYearEnd;
        private DateTime GlobalPreviousYearStart;
        private DateTime GlobalPreviousYearEnd;
        private int GlobalPreviousYearId;
        private int GlobalNextYearId;
        private int GlobalUnix;
        private int RetainedEarningsId;





        public YearEndProcessing() : base() { }
        public void StartYearEndProcessing()
        {
            PopulateYearList();
            var lastYear = accountingYears
           .OrderByDescending(x => x.YearId)
           .FirstOrDefault();


            GlobalPreviousYearId = lastYear.YearId;
            GlobalPreviousYearStart = lastYear.StartDate;
            GlobalPreviousYearEnd = lastYear.EndDate;
            RetainedEarningsId = GlobalVariables.RetainedEarningsId;



            DateTime dtNextStart = lastYear.StartDate.AddYears(1);
            DateTime dtNextEnd = lastYear.EndDate.AddYears(1);

            GlobalNextYearStart = dtNextStart;
            GlobalNextYearEnd = dtNextEnd;

            AccountingYear ay = new AccountingYear
            {
                StartDate = dtNextStart,
                EndDate = dtNextEnd,
            };

            GlobalUnix = GetUnix();

            AccountingYearRepository accountingYearRepository = new AccountingYearRepository(_connectionString);
            GlobalNextYearId = accountingYearRepository.InsertYear(ay);
            PopulateTrailBalanceList();
            decimal nettRetainedEarnings = 0.00M;

            foreach (var row in qryTrailBalances)
            {

                if(row.Total == 0)
                    continue;

                if (row.Category == "PL")
                {
                    nettRetainedEarnings += row.Total;
                    PLEntry(row, RetainedEarningsId);
                }
                else if (row.Category == "BANK" || row.Category == "BS")
                {
                    BSEntry(row);
                }
                else if (row.Category == "RE")
                {
                    // OBVIOUSLY i am going to ignore this because this needs to 
                    // be manually fixed if the balance is not zero
                    // Goal is to make retained earnings ZERO after finalization before commening entry
                    // for the next year. Make 1 solid journal enry based on 
                    // Accontants (CA's) figures
                }

            } // END FOR EACH
           
            MakeRetainedEarningsZeroForNewYear(nettRetainedEarnings, RetainedEarningsId);
            PopulateHistoryTable();

        }
        private void PopulateHistoryTable()
        {

            foreach (var row in qryTrailBalances)
            {

                HistoricalData historical = new HistoricalData()
                {

                    YearId = GlobalPreviousYearId,
                    StartDate = GlobalPreviousYearStart,
                    EndDate = GlobalPreviousYearEnd,
                    AccountId = row.AccountId,
                    Balance = row.Total

                };


                //TransactionRepository transactionRepository = new TransactionRepository(_connectionString);
                HisotricalDataRepository hisotricalDataRepository = new HisotricalDataRepository(_connectionString);
                int ignoreId = hisotricalDataRepository.InsertHistoricalData(historical);


            }

        }

        private void MakeRetainedEarningsZeroForNewYear(decimal nettRetainedEarnings, int RetainedEarningId)
        {
            GeneralLedger gl1 = new GeneralLedger();
            decimal absoluteAmount;

            if (nettRetainedEarnings < 0) // Hopefully this should be case!
            {
                gl1.Tdate = GlobalPreviousYearEnd;
                gl1.Ref = "-Ret.Earns Entry -";
                gl1.Unix = GlobalUnix;
                gl1.YearId = GlobalPreviousYearId;
                gl1.TransactionType = "E*";
                absoluteAmount = Math.Abs(nettRetainedEarnings);
                gl1.Narration = "Being Tranferring Retained Earnings from old year to New Year";

                gl1.AccountId = RetainedEarningId;
                gl1.Particulars = "To Retained Earnings c/d";
                gl1.Amount = absoluteAmount;
            }
            else
            {
                gl1.Tdate = GlobalPreviousYearEnd;
                gl1.Ref = "-Ret.Earns Entry -";
                gl1.Unix = GlobalUnix;
                gl1.YearId = GlobalPreviousYearId;
                gl1.TransactionType = "E*";
                absoluteAmount = Math.Abs(nettRetainedEarnings);
                gl1.Narration = "Being Tranferring Retained Earnings from old year to New Year";

                gl1.AccountId = RetainedEarningId;
                gl1.Particulars = "By Retained Earnings c/d";
                gl1.Amount = absoluteAmount;


            }

            GeneralLedger gl2 = new GeneralLedger();


            if (nettRetainedEarnings < 0) // Hopefully this should be case!
            {
                gl2.Tdate = GlobalNextYearStart;
                gl2.Ref = "-Ret.Earns Entry-";
                gl2.Unix = GlobalUnix;
                gl2.Narration = "Tranferring Retained Earnings from old year to New Year";
                gl2.YearId = GlobalNextYearId;
                gl2.TransactionType = "E*";
                gl2.AccountId = RetainedEarningId;
                gl2.Particulars = "By Retained Earnings b/d";
                gl2.Amount = -absoluteAmount;
            }
            else
            {
                gl2.Tdate = GlobalNextYearStart;
                gl2.Ref = "-Ret.Earns Entry-";
                gl2.Unix = GlobalUnix;
                gl2.Narration = "Tranferring Retained Earnings from old year to New Year";
                gl2.YearId = GlobalNextYearId;
                gl2.TransactionType = "E*";
                gl2.AccountId = RetainedEarningId;
                gl2.Particulars = "To Retained Earnings b/d";
                gl2.Amount = -absoluteAmount;


            }
            // As of now, i dont need to use gl1TID, gl2TID 
            int gl1TID = MakeGLEntry(gl1);
            int gl2TID = MakeGLEntry(gl2);





        }

        private void PLEntry(QryTrailBalance row, int RetainedEarningId)
        {

            GeneralLedger gl1 = new GeneralLedger();
            gl1.Tdate = GlobalPreviousYearEnd;
            gl1.Ref = "-Closing Entry-";
            gl1.Unix = GlobalUnix;
            gl1.YearId = GlobalPreviousYearId;
            gl1.TransactionType = "R*";
            decimal absoluteAmount = Math.Abs(row.Total);

            gl1.Narration = "Being Closing Entry Transfer to Retained Earnings";

            if (row.Total > 0)
            {
                gl1.AccountId = row.AccountId;
                gl1.Particulars = "By Retained Earnings";
                gl1.Amount = -absoluteAmount;
            }
            else
            {
                gl1.AccountId = row.AccountId;
                gl1.Particulars = "To Retained Earnings";
                gl1.Amount = absoluteAmount;

            }

            GeneralLedger gl2 = new GeneralLedger();
            gl2.Tdate = GlobalPreviousYearEnd;
            gl2.Ref = "-Closing Entry-";
            gl2.Unix = GlobalUnix;
            gl2.AccountId = row.AccountId;
            gl2.Narration = "Being Closing Entry Transfer to Retained Earnings";
           
            gl2.YearId = GlobalPreviousYearId;
            gl2.TransactionType = "R*";

            if (row.Total > 0)
            {
                gl2.AccountId = RetainedEarningId;
                gl2.Particulars = "By " + row.Account;
                gl2.Amount = absoluteAmount;
            }
            else
            {
                gl2.AccountId = RetainedEarningId;
                gl2.Particulars = "By " + row.Account;
                gl2.Amount = -absoluteAmount;
            }
            // As of now, i dont need to use gl1TID, gl2TID 
            int gl1TID = MakeGLEntry(gl1);
            int gl2TID = MakeGLEntry(gl2);


        }

        private void BSEntry(QryTrailBalance row)
        {

            GeneralLedger gl1 = new GeneralLedger();

            gl1.Tdate = GlobalPreviousYearEnd;
            gl1.Ref = "-Closing Entry-";
            gl1.Unix = GlobalUnix;
            gl1.YearId = GlobalPreviousYearId;
            gl1.TransactionType = "C*";
            decimal absoluteAmount = Math.Abs(row.Total);
            gl1.Narration = "Being Closing Entry Balance c/d";
            if (row.Total > 0)
            {
                gl1.AccountId = row.AccountId;
                gl1.Particulars = "By Balance C/d";
                gl1.Amount = -absoluteAmount;
            }
            else
            {
                gl1.AccountId = row.AccountId;
                gl1.Particulars = "To Balance c/d";
                gl1.Amount = absoluteAmount;

            }
            GeneralLedger gl2 = new GeneralLedger();

            gl2.Tdate = GlobalNextYearStart;
            gl2.Ref = "-Opening Entry-";
            gl2.Unix = GlobalUnix;
            gl2.YearId = GlobalNextYearId;
            gl2.TransactionType = "C*";
            gl2.Narration = "Being Closing Entry Balance b/d";
            if (row.Total > 0)
            {
                gl2.AccountId = row.AccountId;
                gl2.Particulars = "To Balance b/d";
                gl2.Amount = absoluteAmount;
            }
            else
            {
                gl2.AccountId = row.AccountId;
                gl2.Particulars = "By Balance b/d";
                gl2.Amount = -absoluteAmount;

            }
            // As of now, i dont need to use gl1TID, gl2TID 
            int gl1TID = MakeGLEntry(gl1);
            int gl2TID = MakeGLEntry(gl2);



        }
        private int MakeGLEntry(GeneralLedger gl)
        {

            GeneralLedger tractionClosing = new GeneralLedger
            {
                AccountId = gl.AccountId,
                YearId = gl.YearId,
                Unix = gl.Unix,
                Tdate = gl.Tdate,
                Ref = gl.Ref,
                Particulars = gl.Particulars,
                Amount = gl.Amount,
                TransactionType = gl.TransactionType,
                Narration = gl.Narration,
            };
            TransactionRepository transactionRepository = new TransactionRepository(_connectionString);
            int transactionID = transactionRepository.InsertGeneralLedgerTransaction(tractionClosing);
            return transactionID;

        }


        private int GetUnix()
        {
            using (var connection = new OleDbConnection(_connectionString))
            {
                var command = new OleDbCommand("SELECT MAX(Unix) FROM GeneralLedger", connection);
                connection.Open();
                var result = command.ExecuteScalar();

                return result == DBNull.Value ? 1 : Convert.ToInt32(result) + 1;
            }
        }

        private void PopulateTrailBalanceList()
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    string query = "SELECT * FROM qryTrialBalance";
                    OleDbCommand command = new OleDbCommand(query, connection);


                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            QryTrailBalance qryTrialBalance = new QryTrailBalance
                            {
                                AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
                                Account = reader.GetString(reader.GetOrdinal("Account")),
                                Category = reader.GetString(reader.GetOrdinal("Category")),
                                SubledgerFlag = reader.GetString(reader.GetOrdinal("SubledgerFlag")),
                                Total = reader.GetDecimal(reader.GetOrdinal("Total"))

                            };

                            qryTrailBalances.Add(qryTrialBalance);
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



        private void PopulateYearList()
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    string query = "SELECT * FROM AccountYears Order By StartDate";
                    OleDbCommand command = new OleDbCommand(query, connection);


                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            AccountingYear accountingYear = new AccountingYear()
                            {
                                YearId = reader.GetInt32(reader.GetOrdinal("YearId")),
                                StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                                EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                            };

                            accountingYears.Add(accountingYear);
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
