using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackerRank.ViewModels
{
    public class TopFiveGroupsViewModel
    {
        public string GroupName { get; set; }
        public int CommitsDaily { get; set; }
        public int IssuesCreatedDaily { get; set; }
        public int IssuesSolvedDaily { get; set; }
        public int MergeRequestsDaily { get; set; }
        public int CommentsDaily { get; set; }
        public double GroupRating { get; set; }
    }
}
