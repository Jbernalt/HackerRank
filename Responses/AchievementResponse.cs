using System;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Http;

using static HackerRank.Models.ActionTypes;

namespace HackerRank.Responses
{
    public class AchievementResponse
    {
        public int AchievementId { get; set; }

        [Display(Name = "Name of achievement")]
        public string AchievementName { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Number of actions")]
        public int NumberOfActions { get; set; }

        [Display(Name = "Type of action")]
        [Range(0, int.MaxValue, ErrorMessage = "Select an item please")]
        public ActionType TypeOfAction { get; set; }

        [Display(Name = "Achievement image")]
        public IFormFile Image { get; set; }
    }
}
