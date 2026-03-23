using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LedgerLensMaking.Models.Data
{
    public class MainEntryModel
    {
        public int MeDebitAccount { get; set; }
        public string MeDebitAccountDescription { get; set; }
        public int MeTransactionChartCode {  get; set; }    
        public int MeCreditAccount { get; set; }
        public string MeCreditAccountDescription { get; set; }
        public int MeYearId { get; set; }
        public int MeUnix { get; set; }
        public DateTime MeTDate { get; set; }
        public DateTime MeTDateForSharePurchase { get; set; }
        public string MeRef { get; set; }
       // public string MeParticulars { get; set; }
        public decimal MeAmount { get; set; }
        public string MeTransactionType { get; set; }
        public string MeNarration { get; set; }
        public int MeSubledgerCode { get; set; }  
        public int MeWhichIsSubledgerCode1or2DebitTransOrCreditTrans     { get; set; }
        public int MeInterestAccountCode { get; set; }
        public string MeInterestAccountDescription { get; set; }
        public decimal MeInterestAmount { get; set; }   

        // Subledger Second Transaction Related
        public int MeWhichSubledger_0_1_2 { get; set; } = 0; // 0 No Subledger 1 FD 2 Shares
        public  string MeSubledgerCodeDescription { get; set; }

        public decimal MeShareBuyingPrice { get; set; }
        // Shares Sales Related
       // public decimal MeShareSoldConsideration {  get; set; }  
        public decimal MeQtyOfShares { get; set; }
        public decimal MeSharesAtCost { get; set; } 
        public decimal MeShareSellingPrice { get; set; }
        
        public int MeLTCapgainAccountCode { get; set; }
        public int MeSTCapgainAccountCode { get; set; }
        public int MeLTCaplossAccountCode { get; set; }
        public int MeSTCaplossAccountCode { get; set; }

        //public string MeSTCapgainAccountDescription { get; set; }
        //public string MeLTCapgainAccountDescription { get; set; }
        //public string MeLTCaplossAccountDescription { get; set; }
        // public string MeSTCaplossAccountDescription { get; set; }
        
        public int MeSharesAccountCodeOfSharesChart {  get; set; }  
        public string MeCompanyName {  get; set; }  

        
        

        
        








    }
}
