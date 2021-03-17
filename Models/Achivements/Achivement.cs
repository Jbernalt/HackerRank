using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackerRank.Models.Achievements
{
    public class Achievement
    {
        public int AchievementId { get; set; }
        public string AchievementName { get; set; }
        public string Description { get; set; }
        public byte[] ImageBinaryData { get; set; }
    }
}
