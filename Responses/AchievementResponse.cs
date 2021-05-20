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
        [Required(ErrorMessage = "Achievements needs a name")]
        public string AchievementName { get; set; }

        [Display(Name = "Description")]
        [Required(ErrorMessage = "Please submit a description for your achievement")]
        public string Description { get; set; }

        [Display(Name = "Number of actions")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "The field {0} must be greater than {1} and lower than {2}.")]
        public int NumberOfActions { get; set; }

        [Display(Name = "Type of action")]
        public ActionType TypeOfAction { get; set; }

        [Display(Name = "Achievement image")]
        public IFormFile Image { get; set; }
    }
}
