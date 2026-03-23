using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LedgerLensMaking.Models.Data
{
    public class HistoricalData
    {
        public int HistoryId { get; set; }
        public int YearId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int AccountId { get; set; }
        public decimal Balance { get; set; }

    }
}
