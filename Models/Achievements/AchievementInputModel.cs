using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

using static HackerRank.Models.ActionTypes;

namespace HackerRank.Models.Achievements
{
    public class AchievementInputModel
    {
        public int AchievementId { get; set; }
        public string AchievementName { get; set; }
        public string Description { get; set; }
        public int NumberOfActions { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Select an item please")]
        public ActionType TypeOfAction { get; set; }
        public IFormFile Image { get; set; }
    }
}
