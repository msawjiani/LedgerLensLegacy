using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LedgerLensMaking.Models.Data
{
    public class QryCapgainProfit
    {
        public int ShareId { get; set; }
        public int YearId { get; set; }
        public string Company { get; set; }
        public decimal QtyPurchased { get; set; }
        public decimal PurchaseRate { get; set; }
        public decimal GLAmount { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime SaleDate { get; set; }
        public decimal QtySold { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal ProfitOrLoss { get; set; }
        public int Months { get; set; }
        public int AccountId { get; set; }
        public string Account { get; set; }
    }
}
