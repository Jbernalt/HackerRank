using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HackerRank.Models.Users;

namespace HackerRank.Models.Achivements
{
    public class UserAchivement
    {
        public int UserAchivementId { get; set; }
        public bool IsUnlocked { get; set; }
        public Achivement Achivement { get; set; }
        public User User { get; set; }
    }
}
