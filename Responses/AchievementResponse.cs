using System;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Http;

using static HackerRank.Models.ActionTypes;

namespace HackerRank.Responses
{
    public class AchievementResponse
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
