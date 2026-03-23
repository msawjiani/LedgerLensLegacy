using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LedgerLensMaking.Models.UIModels
{
    public class ShareBalanceTotal
    {
        public string Account { get; set; }
        public string Company { get; set; }
        public decimal Qty { get; set; }            // closing quantity
        public decimal BalanceAtCost { get; set; }  // total value at cost
    }
}
