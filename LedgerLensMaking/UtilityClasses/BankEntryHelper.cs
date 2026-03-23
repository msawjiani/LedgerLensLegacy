using LedgerLensMaking.Models.Data;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LedgerLensMaking.UtilityClasses
{
    public class BankEntryHelper
    {
        private readonly string _connectionString;

        public int BankCode { get; set; }
        public string BankDescription { get; set; }
        public int TranactionChartCode { get; set; }
        public string TransactionCodeDescription { get; set; }
        public int SubledgerTranactionsNo0Fd1Shares2 { get; set; } = 0;
        public int SubledgerCode { get; set; }
        public string SubledgerCodeDescription { get; set; }
        public int TypeOfTransaction1Recepit2Payment { get; set; }
        public DateTime TDate { get; set; }
        public decimal Amount { get; set; }
        public string Reference { get; set; }
        public string Narration { get; set; }
        public int YearId { get; set; }
        public int Unix { get; set; }
        public string TransactionType { get; set; }
        // FOR FD CLOSURE
        public decimal InterestOrGainAmount { get; set; } = 0;
        public int InterestAccountCode { get; set; } = -1;
        public string InterestAccountDescription { get; set; } = string.Empty;

        // FOR SHARES ADDITIONAL INFORMATION IS REQUIRED
        public decimal QtyOfShares { get; set; }
        public DateTime TDateForSharePurchase { get; set; }
        public decimal ShareSellingPrice { get; set; }
        public decimal ShareBuyingPrice { get; set; }
        
        public int LTCapgainAccountCode { get; set; }
        public string LTCapgainAccountDescription { get; set; }
        public int STCapgainAccountCode { get; set; }
        public string STCapgainAccountDescription { get; set; }
        public int SharesAccountCode { get; set; }
        public int LTCaplossAccountCode { get; set; }
        public string LTCaplossAccountDescription { get; set; }
        public int STCaplossAccountCode { get; set; }
        public string STCaplossAccountDescription { get; set; }

        public BankEntryHelper()
        {
            string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={GlobalVariables.DatabaseFileName};";
            _connectionString = connectionString;
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

        public void BankEntryHelperStart()
        {
            Unix = GetUnix();

            MainEntry mainEntry = new MainEntry(_connectionString);
            MainEntryModel mainEntryModel = new MainEntryModel();
            mainEntryModel.MeUnix = Unix;
            mainEntryModel.MeYearId = YearId;
            mainEntryModel.MeNarration = Narration;
            mainEntryModel.MeRef = Reference;
            mainEntryModel.MeTDate = TDate;
            mainEntryModel.MeTransactionType = TransactionType;
            mainEntryModel.MeWhichSubledger_0_1_2 = SubledgerTranactionsNo0Fd1Shares2;

            if (TypeOfTransaction1Recepit2Payment == 1) // THIS IS Receipt
            {
                mainEntryModel.MeDebitAccount = BankCode;
                mainEntryModel.MeAmount = Amount;
                mainEntryModel.MeDebitAccountDescription = "To " + TransactionCodeDescription;
                mainEntryModel.MeCreditAccount = TranactionChartCode;
                mainEntryModel.MeCreditAccountDescription = "By " + BankDescription;

                if (SubledgerTranactionsNo0Fd1Shares2 == 1)
                {
                    mainEntryModel.MeWhichIsSubledgerCode1or2DebitTransOrCreditTrans = 2; // Push the Code for Receipt
                    mainEntryModel.MeTransactionChartCode = TranactionChartCode;
                    mainEntryModel.MeSubledgerCode = SubledgerCode;
                    mainEntryModel.MeInterestAccountCode = InterestAccountCode;
                    mainEntryModel.MeInterestAmount = InterestOrGainAmount;
                    //mainEntry.MakeGLEntry(mainEntryModel);
                    MainEntryFDReceipt mainEntryFDReceipt = new MainEntryFDReceipt(_connectionString);
                    mainEntryFDReceipt.MakeFDJournals(mainEntryModel);



                }
                else if (SubledgerTranactionsNo0Fd1Shares2 == 2)
                {

                    // Boss this is going to be complex.It needs to be done using loop
                    // therefore i am creating a seperate class. But to avoid clutter
                    // this is needed. 

                    mainEntryModel.MeSubledgerCode = TranactionChartCode;
                    mainEntryModel.MeCompanyName = SubledgerCodeDescription;
                    mainEntryModel.MeQtyOfShares = QtyOfShares;
                    mainEntryModel.MeSharesAccountCodeOfSharesChart = SharesAccountCode;
                    mainEntryModel.MeShareSellingPrice = ShareSellingPrice;

                    mainEntryModel.MeLTCapgainAccountCode = LTCapgainAccountCode;
                    mainEntryModel.MeSTCapgainAccountCode = STCapgainAccountCode;

                    mainEntryModel.MeLTCaplossAccountCode = LTCaplossAccountCode;
                    mainEntryModel.MeSTCaplossAccountCode = STCaplossAccountCode;







                    MainEntryShareReceipt mainEntryShareReceipt = new MainEntryShareReceipt(_connectionString);
                    mainEntryShareReceipt.MakeShareJournals(mainEntryModel);
                }
                else
                {
                    mainEntry.MakeGLEntry(mainEntryModel);
                }
            }
            else if (TypeOfTransaction1Recepit2Payment == 2) // This is Payment
            {
                mainEntryModel.MeDebitAccount = TranactionChartCode;
                mainEntryModel.MeAmount = Amount;
                mainEntryModel.MeCreditAccount = BankCode;
                mainEntryModel.MeCreditAccountDescription = "By " + TransactionCodeDescription;
                mainEntryModel.MeDebitAccountDescription = "To " + BankDescription;
                if (SubledgerTranactionsNo0Fd1Shares2 == 0)
                {
                    mainEntry.MakeGLEntry(mainEntryModel);
                }
                else if (SubledgerTranactionsNo0Fd1Shares2 == 1)
                {
                    mainEntryModel.MeWhichIsSubledgerCode1or2DebitTransOrCreditTrans = 1; // Pushing the code for payment
                    mainEntryModel.MeSubledgerCode = SubledgerCode;
                    mainEntry.MakeGLEntry(mainEntryModel);
                }
                else if (SubledgerTranactionsNo0Fd1Shares2 == 2)
                {
                    // For shares additional information will also be required
                    mainEntryModel.MeWhichIsSubledgerCode1or2DebitTransOrCreditTrans = 1;
                    mainEntryModel.MeQtyOfShares = QtyOfShares;
                    mainEntryModel.MeShareBuyingPrice = ShareBuyingPrice;
                    mainEntryModel.MeTDateForSharePurchase = TDateForSharePurchase;
                    mainEntryModel.MeSubledgerCodeDescription = SubledgerCodeDescription;
                    mainEntryModel.MeSubledgerCode = SubledgerCode;
                    mainEntry.MakeGLEntry(mainEntryModel);
                }

            }
        }

    }
}
