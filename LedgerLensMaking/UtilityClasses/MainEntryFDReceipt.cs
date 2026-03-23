using LedgerLensMaking.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LedgerLensMaking.UtilityClasses
{
    public class MainEntryFDReceipt : BaseRepository
    {
        public MainEntryFDReceipt(string connectionString) : base() { }

        public void MakeFDJournals(MainEntryModel mainEntryModel)
        {
            decimal bankReceipt = mainEntryModel.MeAmount;
            decimal fdPrincipal = mainEntryModel.MeAmount - mainEntryModel.MeInterestAmount;

            // Bank Receipt Entry
            GeneralLedger bankEntry = new GeneralLedger
            {
                AccountId = mainEntryModel.MeDebitAccount,
                YearId = mainEntryModel.MeYearId,
                Unix = mainEntryModel.MeUnix,
                Tdate = mainEntryModel.MeTDate,
                Ref = mainEntryModel.MeRef,
                Particulars = "FD Closed Consolidated Entry 1",
                Amount = bankReceipt,
                TransactionType = mainEntryModel.MeTransactionType,
                Narration = $"{mainEntryModel.MeNarration} FD Closed Principal: {fdPrincipal} Interest {mainEntryModel.MeInterestAmount}",
            };

            // FD Principal Entry
            GeneralLedger fdPrincipalEntry = new GeneralLedger
            {
                AccountId = mainEntryModel.MeTransactionChartCode,
                YearId = mainEntryModel.MeYearId,
                Unix = mainEntryModel.MeUnix,
                Tdate = mainEntryModel.MeTDate,
                Ref = mainEntryModel.MeRef,
                Particulars = "FD Closed Consolidated Entry 2",
                Amount = -fdPrincipal,
                TransactionType = mainEntryModel.MeTransactionType,
                Narration = $"{mainEntryModel.MeNarration} FD Closed Principal: {fdPrincipal} Interest {mainEntryModel.MeInterestAmount}",
            };

            // Interest Entry
            GeneralLedger interestEntry = new GeneralLedger
            {
                AccountId = mainEntryModel.MeInterestAccountCode,
                YearId = mainEntryModel.MeYearId,
                Unix = mainEntryModel.MeUnix,
                Tdate = mainEntryModel.MeTDate,
                Ref = mainEntryModel.MeRef,
                Particulars = "FD Closed Consolidated Entry 3",
                Amount = -mainEntryModel.MeInterestAmount,
                TransactionType = mainEntryModel.MeTransactionType,
                Narration = $"{mainEntryModel.MeNarration} FD Closed Principal: {fdPrincipal} Interest {mainEntryModel.MeInterestAmount}",
            };

            TransactionRepository transactionRepository = new TransactionRepository(_connectionString);

            // Insert transactions
            int transactionBank = transactionRepository.InsertGeneralLedgerTransaction(bankEntry);
            int transactionFD = transactionRepository.InsertGeneralLedgerTransaction(fdPrincipalEntry);
            int transactionInterest = transactionRepository.InsertGeneralLedgerTransaction(interestEntry);

            // Insert subledger transaction
            SubledgerTrans subledgerTrans = new SubledgerTrans
            {
                SubledgerId = mainEntryModel.MeSubledgerCode,
                Unix = mainEntryModel.MeUnix,
                TransactionId = transactionFD,
            };
            transactionRepository.InsertSubledgerTransaction(subledgerTrans, transactionFD);
        }
    }
}
