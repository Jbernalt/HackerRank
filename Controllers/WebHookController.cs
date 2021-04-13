using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using HackerRank.Hubs;
using HackerRank.Responses;
using HackerRank.ViewModels;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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

        public WebHookController(ILogger<WebHookController> logger, IConfiguration config, IHubContext<LiveFeedHub> liveFeedHubContext)
        {
            _logger = logger;
            _config = config;
            _liveFeedHubContext = liveFeedHubContext;
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

            if (gitLabEvent == "Push Hook")
            {
                model.WebHookResponse.WebHookCommitResponse = JsonSerializer.Deserialize<WebHookCommitResponse>(json);
                message = $"{model.WebHookResponse.WebHookCommitResponse.user_name} made a push to {model.WebHookResponse.WebHookCommitResponse.project.name}"
                    + $", " + DateTime.UtcNow.ToString("dddd, dd MMMM yyyy HH:mm");
            }

            else if (gitLabEvent == "Issue Hook")
            {
                model.WebHookResponse.WebHookIssueResponse = JsonSerializer.Deserialize<WebHookIssueResponse>(json);
                message = $"{model.WebHookResponse.WebHookIssueResponse.user.name} " +
                    $"{model.WebHookResponse.WebHookIssueResponse.object_attributes.state} the issue {model.WebHookResponse.WebHookIssueResponse.object_attributes.title}"
                    + $", " + DateTime.UtcNow.ToString("dddd, dd MMMM yyyy HH:mm");
            }

            else if (gitLabEvent == "Merge Request Hook")
            {
                model.WebHookResponse.WebHookMergeResponse = JsonSerializer.Deserialize<WebHookMergeResponse>(json);
                message = $"{model.WebHookResponse.WebHookMergeResponse.user.name} {model.WebHookResponse.WebHookMergeResponse.object_attributes.state} " +
                    $"from {model.WebHookResponse.WebHookMergeResponse.object_attributes.source_branch} to {model.WebHookResponse.WebHookMergeResponse.object_attributes.target_branch}" +
                    $" on project {model.WebHookResponse.WebHookMergeResponse.project.name}, " + DateTime.UtcNow.ToString("dddd, dd MMMM yyyy HH:mm");
            }

            else if (gitLabEvent == "Note Hook")
            {
                model.WebHookResponse.WebHookCommentResponse = JsonSerializer.Deserialize<WebHookCommentResponse>(json);
                message = $"{model.WebHookResponse.WebHookCommentResponse.user.name} commented on {model.WebHookResponse.WebHookCommentResponse.project.name}"
                    + $", " + DateTime.UtcNow.ToString("dddd, dd MMMM yyyy HH:mm");
            }

            if (message != string.Empty)
            {
                await _liveFeedHubContext.Clients.All.SendAsync("ReceiveMessage", message);
                return Ok();
            }

            return BadRequest();
        }
    }
}
