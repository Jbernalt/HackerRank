using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackerRank.ViewModels
{
    public class TopFiveUsersViewModel
    {
        public string Username { get; set; }
        public string ProfileImage { get; set; }

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
