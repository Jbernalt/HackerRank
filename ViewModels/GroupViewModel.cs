using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackerRank.ViewModels
{
    public class GroupViewModel
    {
        public string GroupName { get; set; }
        public int GroupCommits { get; set; }
        public int GroupMergeRequests { get; set; }
        public int GroupIssuesCreated { get; set; }
        public int GroupIssuesSolved { get; set; }
        public int GroupComments { get; set; }


        //Error management
        public int StatusCode { get; set; }
        public string ReasonPhrase { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
    }
}
