using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LedgerLensMaking.Models.Data
{
    public class SubledgerTrans
    {
        public int SubledgerTransId { get; set; }
        public int SubledgerId { get; set; }
        public int TransactionId { get; set; }
        public int Unix { get; set; }
    }
}
