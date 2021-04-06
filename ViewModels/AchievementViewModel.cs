using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static HackerRank.Models.ActionTypes;

namespace HackerRank.ViewModels
{
    public class AchievementViewModel
    {
        public int AchievementId { get; set; }
        public string AchievementName { get; set; }
        public string Description { get; set; }
        public int NumberOfActions { get; set; }
        public ActionType TypeOfAction { get; set; }
        public string Image { get; set; }
        public bool IsUnlocked { get; set; }
    }
}
