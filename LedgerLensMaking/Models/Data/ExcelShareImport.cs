using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LedgerLensMaking.Models.Data
{
    public class ExcelShareImport
    {
        public int GLAccountCodeForCredit { get; set; }
        public string GLAccountForCredit { get; set; }
        public int GLSharesAccountCodeForDebit { get; set; } 
        public string GLSharesAccountForDebit { get; set; }
        public string Company { get; set; }
        public int ShareId { get; set; } = 0;
        public DateTime TradeDate { get; set; }
        public decimal Qty { get; set; }
        public decimal Rate { get; set; }
        public decimal Amt { get; set; } = 0.0M;
        public string ErrorMessage { get; set; } // New field for errors
    }
}
