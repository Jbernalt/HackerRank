using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using HackerRank.Models.Users;
using HackerRank.Responses;

namespace HackerRank.ViewModels
{
    public class TopFiveViewModel
    {
        public TopFiveViewModel()
        {
            WebHookResponse = new WebHookResponse();
            TopFiveGroups = new List<TopFiveGroupsViewModel>();
            TopFiveUsers = new List<TopFiveUsersViewModel>();
            UserLevel = new List<UserLevel>();
        }

        public List<TopFiveGroupsViewModel> TopFiveGroups { get; set; }
        public List<TopFiveUsersViewModel> TopFiveUsers { get; set; }

        //LiveWebHookFeed
        public WebHookResponse WebHookResponse { get; set; }

        //levels
        public List<UserLevel> UserLevel { get; set; }
    }
}
