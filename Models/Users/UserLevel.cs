using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackerRank.Models.Users
{
    public class UserLevel
    {
        public int UserLevelId { get; set; }
        public Level Level { get; set; }
        public int PrestigeLevel { get; set; }
        public double CurrentExperience { get; set; }
        public User User { get; set; }

    }
}
