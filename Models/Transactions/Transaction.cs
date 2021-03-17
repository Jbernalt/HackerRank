using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackerRank.Models.Transactions
{
    public class Transaction
    {
        public int TransactionId { get; set; }
        public string Description { get; set; }
        public double Points { get; set; }
    }
}
