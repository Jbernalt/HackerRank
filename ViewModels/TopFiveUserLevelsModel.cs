using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HackerRank.Models;
using HackerRank.Models.Users;

namespace HackerRank.ViewModels
{
    public class TopFiveUserLevelsModel
    {
        public Level Level { get; set; }
        public int PrestigeLevel { get; set; }
        public double CurrentExperience { get; set; }
        public string Username { get; set; }
        public string ProfileImage { get; set; }
    }
}
