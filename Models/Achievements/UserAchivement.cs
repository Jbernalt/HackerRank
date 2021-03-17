using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HackerRank.Models.Users;

namespace HackerRank.Models.Achievements
{
    public class UserAchievement
    {
        public int UserAchievementId { get; set; }
        public bool IsUnlocked { get; set; }
        public Achievement Achievement { get; set; }
        public User User { get; set; }
    }
}
