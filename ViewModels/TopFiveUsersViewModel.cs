using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HackerRank.Models.Users;
using HackerRank.Models.Transactions;

namespace HackerRank.ViewModels
{
    public class TopFiveUsersViewModel
    {
        //User
        public string UserName { get; set; }

        //Stats
        public double DailyRating { get; set; }
        public double MonthlyRating { get; set; }

        //UserTransaction
        public int Commits { get; set; }
        public int IssuesCreated { get; set; }
        public int IssuesSolved { get; set; }
        public int MergeRequest { get; set; }
        public int Comments { get; set; }

    }
}
