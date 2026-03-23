using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LedgerLensMaking.Models.UIModels
{
    public class ReportTrialBalanceModel
    {
        public int AccountId { get; set; }
        public string Account { get; set; }
        public decimal DebitColumn { get; set; }
        public decimal CreditColumn { get; set; }
    }
}
