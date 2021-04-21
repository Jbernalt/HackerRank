using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using HackerRank.Data;
using HackerRank.Hubs;
using HackerRank.Models;
using HackerRank.Models.Users;
using HackerRank.Responses;
using HackerRank.ViewModels;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;


namespace HackerRank.Controllers
{
    [ApiController]
    [Route("api/webhook/")]
    public class WebHookController : Controller
    {
        private readonly IHubContext<LiveFeedHub> _liveFeedHubContext;
        private readonly ILogger<WebHookController> _logger;
        private readonly IConfiguration _config;
        private readonly HackerRankContext _hackerRankContext;

        public WebHookController(ILogger<WebHookController> logger, IConfiguration config, IHubContext<LiveFeedHub> liveFeedHubContext, HackerRankContext hackerRankContext)
        {
            _logger = logger;
            _config = config;
            _liveFeedHubContext = liveFeedHubContext;
            _hackerRankContext = hackerRankContext;
        }

        [IgnoreAntiforgeryToken]
        [HttpPost]
        [Route("receive")]
        public async Task<IActionResult> Receive()
        {
            Request.Headers.TryGetValue("X-Gitlab-Event", out StringValues gitLabEvent);
            Request.Headers.TryGetValue("X-Gitlab-Token", out StringValues gitLabSignature);

            if (gitLabSignature.FirstOrDefault() != _config["Authentication:GitLab:WebHookAuthentication"])
                return Unauthorized();

            TopFiveViewModel model = new();
            using var reader = new StreamReader(Request.Body);
            var json = await reader.ReadToEndAsync();
            string message = string.Empty;
            string username = string.Empty;
            double point = 0;

            if (gitLabEvent == "Push Hook")
            {
                model.WebHookResponse.WebHookCommitResponse = JsonSerializer.Deserialize<WebHookCommitResponse>(json);
                message = $"{model.WebHookResponse.WebHookCommitResponse.user_name} made a push to {model.WebHookResponse.WebHookCommitResponse.project.name}"
                    + $", " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm");
                username = model.WebHookResponse.WebHookCommitResponse.user_username;
                point = 0.15;
            }

            else if (gitLabEvent == "Issue Hook")
            {
                model.WebHookResponse.WebHookIssueResponse = JsonSerializer.Deserialize<WebHookIssueResponse>(json);
                message = $"{model.WebHookResponse.WebHookIssueResponse.user.name} " +
                    $"{model.WebHookResponse.WebHookIssueResponse.object_attributes.state} the issue {model.WebHookResponse.WebHookIssueResponse.object_attributes.title}"
                    + $", " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm");
                username = model.WebHookResponse.WebHookIssueResponse.user.username;

                if(model.WebHookResponse.WebHookIssueResponse.object_attributes.state == "opened")
                    point = 0.15;
                else
                    point = 0.3;


            }

            else if (gitLabEvent == "Merge Request Hook")
            {
                model.WebHookResponse.WebHookMergeResponse = JsonSerializer.Deserialize<WebHookMergeResponse>(json);
                message = $"{model.WebHookResponse.WebHookMergeResponse.user.name} {model.WebHookResponse.WebHookMergeResponse.object_attributes.state} " +
                    $"from {model.WebHookResponse.WebHookMergeResponse.object_attributes.source_branch} to {model.WebHookResponse.WebHookMergeResponse.object_attributes.target_branch}" +
                    $" on project {model.WebHookResponse.WebHookMergeResponse.project.name}, " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm");
                username = model.WebHookResponse.WebHookMergeResponse.user.username;
                point = 0.35;
            }

            else if (gitLabEvent == "Note Hook")
            {
                model.WebHookResponse.WebHookCommentResponse = JsonSerializer.Deserialize<WebHookCommentResponse>(json);
                message = $"{model.WebHookResponse.WebHookCommentResponse.user.name} commented on {model.WebHookResponse.WebHookCommentResponse.project.name}"
                    + $", " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm");
                username = model.WebHookResponse.WebHookCommentResponse.user.username;
                point = 0.05;
            }

            if (message != string.Empty)
            {
                await _liveFeedHubContext.Clients.All.SendAsync("ReceiveMessage", message);
                string updateduserlevel = await UpdateUserLevel(username, point);
                if (!string.IsNullOrWhiteSpace(updateduserlevel))
                {
                    await _liveFeedHubContext.Clients.All.SendAsync("ReceiveMessage", updateduserlevel);
                }
                return Ok();
            }

            return BadRequest();
        }

        public async Task<string> UpdateUserLevel(string userName, double points)
        {
            string message = string.Empty;
            UserLevel userLevel = await _hackerRankContext.UserLevels.Include(u => u.User).Include("Level").Where(u => u.User.UserName == userName).FirstOrDefaultAsync();
            var nextLevel = _hackerRankContext.Levels.Where(i => i.LevelId == userLevel.Level.LevelId + 1).FirstOrDefault();
            userLevel.CurrentExperience += points;

            if(userLevel.CurrentExperience >= nextLevel.XpNeeded)
            {
                userLevel.Level = nextLevel;
                message = $"{userLevel.User.UserName} just leveled up. They are now level {nextLevel.LevelId} {nextLevel.LevelName}, {DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm")}";
            }

            await _hackerRankContext.SaveChangesAsync();
            return message;
        }
    }
}
