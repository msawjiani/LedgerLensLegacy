using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LedgerLensMaking.Models.Data
{
    public class QryShareBalanceNett
    {
        public int ShareId { get; set; }
        public string Company { get; set; }
        public decimal TS { get; set; }
        public int AccountId { get; set; }
    }
}
