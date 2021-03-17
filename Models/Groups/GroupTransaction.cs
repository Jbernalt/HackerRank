using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using HackerRank.Models.Transactions;

namespace HackerRank.Models.Groups
{
    public class GroupTransaction
    {
        public int TransactionId { get; set; }
        public int GroupId { get; set; }
        public int Value { get; set; }
        public DateTime FetchDate { get; set; }
        public Group Group { get; set; }
        public Transaction Transaction { get; set; }

    }
}
