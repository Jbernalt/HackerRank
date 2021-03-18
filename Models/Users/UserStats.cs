using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackerRank.Models.Users
{
    public class UserStats
    {
        public double DailyRating { get; set; }
        public double MonthlyRating { get; set; }
        public int UserStatsId { get; set; }
        public int TotalCommits { get; set; }
        public int TotalMergeRequests { get; set; }
        public int TotalIssuesSolved { get; set; }
        public int TotalIssuesCreated { get; set; }
        public int TotalComments { get; set; }
    }
}
