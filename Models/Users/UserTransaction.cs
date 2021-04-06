using HackerRank.Models.Projects;
using HackerRank.Models.Transactions;
using System;

namespace HackerRank.Models.Users
{
    public class UserTransaction
    {
        public int TransactionId { get; set; }
        public string UserId { get; set; }
        public DateTime FetchDate { get; set; }
        public Project Project { get; set; }
        public Transaction Transaction { get; set; }
        public User User { get; set; }
    }
}
