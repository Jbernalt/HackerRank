using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackerRank.Models
{
    public class ChartData
    {
        public string TimeStamp { get; set; }
        public int NumOfCommits { get; set; }
        public int NumOfIssuesCreated { get; set; }
        public int NumOfIssuesSolved { get; set; }
        public int NumOfMergeRequests { get; set; }
        public int NumOfComments { get; set; }
    }
}
