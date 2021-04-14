using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackerRank.Models
{
    public class ActionTypes
    {
        public enum ActionType
        {
            [Display(Name = "Commits")]
            Commit,

            [Display(Name = "Issues opened")]
            IssueOpened,

            [Display(Name = "Issues solved")]
            IssueSolved,

            [Display(Name = "Merge requests")]
            MergeRequest,

            [Display(Name = "Comments")]
            Comment
        }
    }
}
