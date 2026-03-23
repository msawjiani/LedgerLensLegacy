using LedgerLensMaking.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LedgerLensMaking.Models.UIModels
{
    public class PaymentsSelectedShareDetails
    {
        public SharesChart SelectedShare { get; set; }
        public decimal Quantity { get; set; }
        public decimal Rate { get; set; }
    }
}
