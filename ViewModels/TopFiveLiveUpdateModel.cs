using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HackerRank.Models.Users;

namespace HackerRank.ViewModels
{
    public class TopFiveLiveUpdateModel
    {
        public List<TopFiveGroupsViewModel> TopFiveGroups { get; set; }
        public List<TopFiveUsersViewModel> TopFiveUsers { get; set; }
        public List<TopFiveUserLevelsModel> TopFiveUserLevels { get; set; }
        public string LiveFeedMessage { get; set; }
    }
}
