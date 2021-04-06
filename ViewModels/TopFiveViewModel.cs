using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackerRank.ViewModels
{
    public class TopFiveViewModel
    {
        //Group
        public string GroupName { get; set; }
        public int CommitsDaily { get; set; }
        public int IssuesCreatedDaily { get; set; }
        public int IssuesSolvedDaily { get; set; }
        public int MergeRequestsDaily { get; set; }
        public int CommentsDaily { get; set; }
        public double GroupRating { get; set; }

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
