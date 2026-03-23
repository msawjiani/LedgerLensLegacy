using LedgerLensMaking.Models.Data;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LedgerLensMaking.Models.UIModels
{
    public class JournalModel 
    {



        public int AccountId { get; set; }
        public string Account { get; set; }
        public decimal DebitColumn { get; set; }
        public decimal CreditColumn { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public string Narration { get; set; }
        public string DateStr { get; set; }
        public string Ref { get; set; }

    }
}
