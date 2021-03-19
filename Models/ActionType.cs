using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackerRank.Models
{
    public class ActionTypes
    {
        public enum ActionType
        {
            Commit,
            IssueOpened,
            IssueSolved,
            MergeRequest,
            Comment
        }
    }
}
