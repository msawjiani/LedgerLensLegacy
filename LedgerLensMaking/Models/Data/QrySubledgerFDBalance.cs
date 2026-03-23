using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LedgerLensMaking.Models.Data
{
    public class QrySubledgerFDBalance
    {
        public string Account { get; set; }
        public int SubledgerId { get; set; }
        public int AccountId { get; set; }
        public string Subaccount { get; set; }
        public decimal TotalFD { get; set; }
        public string Active {  get; set; }
    }
}
