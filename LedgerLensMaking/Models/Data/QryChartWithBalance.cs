using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LedgerLensMaking.Models.Data
{
    public class QryChartWithBalance
    {
        public int AccountId { get; set; }
        public string Account { get; set; }
        public string Category { get; set; }
        public string SubledgerFlag { get; set; }
        public decimal Balance { get; set; }

    }
}
