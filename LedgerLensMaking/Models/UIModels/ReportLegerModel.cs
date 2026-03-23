using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LedgerLensMaking.Models.UIModels
{
    public class ReportLegerModel
    {
        public int YearId { get; set; }
        public int AccountId { get; set; }
        public string Account { get; set; }
        public string Category { get; set; }
        public int Unix { get; set; }
        public DateTime TDate { get; set; }
        public string Ref { get; set; }
        public string Particulars { get; set; }
        public decimal Amount { get; set; }
        public decimal DebitColumn { get; set; }
        public decimal CreditColumn { get; set; }
        public decimal RunningBalance { get; set; }
        public string TransactionType { get; set; }
        public string Narration { get; set; }
    }
}
