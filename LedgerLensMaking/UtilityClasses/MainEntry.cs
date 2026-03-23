using LedgerLensMaking.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LedgerLensMaking.UtilityClasses
{
    public class MainEntry : BaseRepository
    {
        public MainEntry(string connectionString) : base() { }



        public void MakeGLEntry(MainEntryModel mainEntryMain)
        {
            MainEntry mainEntry = new MainEntry(_connectionString);

            // Make a small change in the narration for the Shares
            if (mainEntryMain.MeWhichSubledger_0_1_2 == 2)
            {
                mainEntryMain.MeNarration += " **Company: " + mainEntryMain.MeSubledgerCodeDescription + " -Qty-  : " + mainEntryMain.MeQtyOfShares.ToString() + " Rate: " + mainEntryMain.MeShareBuyingPrice.ToString();
            }
            // Create debit and credit transactions
            GeneralLedger debitTransaction = new GeneralLedger
            {
                AccountId = mainEntryMain.MeDebitAccount,
                YearId = mainEntryMain.MeYearId,
                Unix = mainEntryMain.MeUnix,
                Tdate = mainEntryMain.MeTDate,
                Ref = mainEntryMain.MeRef,
                Particulars = mainEntryMain.MeDebitAccountDescription,
                Amount = mainEntryMain.MeAmount,
                TransactionType = mainEntryMain.MeTransactionType,
                Narration = mainEntryMain.MeNarration,
            };

            GeneralLedger creditTransaction = new GeneralLedger
            {
                AccountId = mainEntryMain.MeCreditAccount,
                YearId = mainEntryMain.MeYearId,
                Unix = mainEntryMain.MeUnix,
                Tdate = mainEntryMain.MeTDate,
                Ref = mainEntryMain.MeRef,
                Particulars = mainEntryMain.MeCreditAccountDescription,
                Amount = -mainEntryMain.MeAmount,
                TransactionType = mainEntryMain.MeTransactionType,
                Narration = mainEntryMain.MeNarration,
            };

            TransactionRepository transactionRepository = new TransactionRepository(_connectionString);

            // Insert debit transaction and get its ID
            int debitTransactionId = transactionRepository.InsertGeneralLedgerTransaction(debitTransaction);

            // Insert credit transaction and get its ID
            int creditTransactionId = transactionRepository.InsertGeneralLedgerTransaction(creditTransaction);

            // Assuming whichEntryRequiresTransactionCode is 1 for debit and 2 for credit
            int transactionIdForSubledger = (mainEntryMain.MeWhichIsSubledgerCode1or2DebitTransOrCreditTrans == 1) ? debitTransactionId : creditTransactionId;

            if (mainEntryMain.MeWhichSubledger_0_1_2 == 1)
            {
                SubledgerTrans subledgerTrans = new SubledgerTrans
                {
                    SubledgerId = mainEntryMain.MeSubledgerCode,
                    Unix = mainEntryMain.MeUnix,
                    TransactionId = transactionIdForSubledger

                };

                transactionRepository.InsertSubledgerTransaction(subledgerTrans, transactionIdForSubledger);
            }
            else if (mainEntryMain.MeWhichSubledger_0_1_2 == 2) // FOR SHARES
            {
                

                SharePurchases subLedgerTrans = new SharePurchases
                {
                    ShareId = mainEntryMain.MeSubledgerCode,
                    Unix = mainEntryMain.MeUnix,
                    PurchaseDate = mainEntryMain.MeTDateForSharePurchase,
                    TransactionId = transactionIdForSubledger,
                    QtyPurchased = mainEntryMain.MeQtyOfShares,
                    PurchaseRate = mainEntryMain.MeShareBuyingPrice,


                };
                transactionRepository.InsertSharePurchaseTransaction(subLedgerTrans, transactionIdForSubledger);
            }
        }

    }
}
