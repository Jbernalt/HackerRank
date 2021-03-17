using HackerRank.Models.Transactions;
using System;

namespace HackerRank.Models.Users
{
    public class UserTransaction
    {
        public int Value { get; set; }

        public DateTime FetchDate { get; set; }
        public int TransactionId { get; set; }
        public Transaction Transaction { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
