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

using Microsoft.AspNetCore.Identity;

namespace HackerRank.ViewModels
{
    public class UserViewModel
    {
        public string Username { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
        public DateTime DateCreated { get; set; }
        public string ProfileImage { get; set; }
        public bool IsPublic { get; set; }
        public List<Group> Groups { get; set; }
        public List<Project> Projects { get; set; }
        public UserStats UserStats { get; set; }
        public List<UserAchievement> UserAchievements { get; set; }
        public List<ChartData> ChartDatas { get; set; }
        public UserLevel UserLevel { get; set; }   
        public List<string> Roles { get; set; }
    }
}
