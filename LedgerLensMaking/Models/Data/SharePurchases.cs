using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LedgerLensMaking.Models.Data
{
    public class SharePurchases
    {
        public int PurchaseTransId   { get; set; }
        public int ShareId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public int TransactionId { get; set; }
        public int Unix { get; set; }
        public decimal QtyPurchased { get; set; }
        public decimal PurchaseRate { get; set; }

    }
}
