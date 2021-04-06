using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HackerRank.Models;
using HackerRank.Models.Achievements;
using HackerRank.Models.Groups;
using HackerRank.Models.Projects;
using HackerRank.Models.Users;

namespace HackerRank.ViewModels
{
    public class UserViewModel
    {
        public string Username { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
        public DateTime DateCreated { get; set; }
        public List<Group> Groups { get; set; }
        public List<Project> Projects { get; set; }
        public UserStats UserStats { get; set; }
        public List<UserAchievement> UserAchievements { get; set; }
        public List<ChartData> ChartDatas { get; set; }
    }
}
