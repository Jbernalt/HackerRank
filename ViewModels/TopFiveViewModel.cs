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
            TopFiveGroups = new List<TopFiveGroupsViewModel>();
            TopFiveUsers = new List<TopFiveUsersViewModel>();
            TopFiveUserLevels = new List<TopFiveUserLevelsModel>();
        }

        public List<TopFiveGroupsViewModel> TopFiveGroups { get; set; }
        public List<TopFiveUsersViewModel> TopFiveUsers { get; set; }
        public List<TopFiveUserLevelsModel> TopFiveUserLevels { get; set; }
    }
}
