using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackerRank.Models.Users
{
    public class UserStats
    {
        [Key]
        public string UserStatsId { get; set; }
        public double DailyRating { get; set; }
        public double MonthlyRating { get; set; }
        public int TotalCommits { get; set; }
        public int TotalMergeRequests { get; set; }
        public int TotalIssuesSolved { get; set; }
        public int TotalIssuesCreated { get; set; }
        public int TotalComments { get; set; }
        public User User { get; set; }
    }
}
