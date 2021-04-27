using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackerRank.Models.Groups
{
    public class GroupStats
    {
        [Key]
        public int GroupStatsId { get; set; }
        public double GroupDailyRating { get; set; }
        public double MonthlyRating { get; set; }
        public int CommitsDaily { get; set; }
        public int MergeRequestsDaily { get; set; }
        public int IssuesSolvedDaily { get; set; }
        public int IssuesCreatedDaily { get; set; }
        public int CommentsDaily { get; set; }
        public int TotalCommits { get; set; }
        public int TotalMergeRequests { get; set; }
        public int TotalIssuesSolved { get; set; }
        public int TotalIssuesCreated { get; set; }
        public int TotalComments { get; set; }
        public Group Group { get; set; }
    }
}
