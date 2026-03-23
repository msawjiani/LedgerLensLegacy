using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LedgerLennMaking.Models.Data
{

        public class QryShareBalance
        {
            public int YearId { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; } = DateTime.MinValue;
            public int AccountId { get; set; }
            public string Account { get; set; }
            public int ShareId { get; set; }
            public string Company { get; set; }
            public int TransactionId { get; set; }
            public DateTime TDate { get; set; }
            public DateTime SharePurchaseTdate { get; set; }
            public string Ref { get; set; }
            public string Particulars { get; set; }
            public decimal Amount { get; set; }
            public decimal QtyPurchased { get; set; }
            public decimal PurchaseRate { get; set; }
            public decimal QS { get; set; } // Changed from int to double
            public decimal Balance { get; set; } // Changed from int to double
            public decimal AtCost { get; set; } // Changed from decimal to double
            public int PurchaseTransId { get; set; }
    }

    
}
