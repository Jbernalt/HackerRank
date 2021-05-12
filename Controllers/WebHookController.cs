using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using AutoMapper;

using HackerRank.Data;
using HackerRank.Hubs;
using HackerRank.Models;
using HackerRank.Models.Achievements;
using HackerRank.Models.Projects;
using HackerRank.Models.Users;
using HackerRank.Responses;
using HackerRank.Services;
using HackerRank.ViewModels;

using Microsoft.AspNetCore.Authorization;
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
        private readonly IUserService _userService;
        private readonly IGroupService _groupService;
        private readonly IRankingService _rankingService;


        public WebHookController(ILogger<WebHookController> logger, IConfiguration config, IHubContext<LiveFeedHub> liveFeedHubContext, IUserService userService, IGroupService groupService, IRankingService rankingService)
        {
            _logger = logger;
            _config = config;
            _liveFeedHubContext = liveFeedHubContext;
            _userService = userService;
            _groupService = groupService;
            _rankingService = rankingService;

        }

        [IgnoreAntiforgeryToken]
        [HttpPost]
        [Route("receive")]
        public async Task<IActionResult> Receive()
        {
            Request.Headers.TryGetValue("X-Gitlab-Event", out StringValues gitLabEvent);
            Request.Headers.TryGetValue("X-Gitlab-Token", out StringValues gitLabSignature);

            if (gitLabSignature.FirstOrDefault() != _config["Authentication-GitLab-WebHookAuthentication"])
            {
                _logger.LogDebug("WebHook token was invalid");
                return Unauthorized();
            }

            using var reader = new StreamReader(Request.Body);
            var json = await reader.ReadToEndAsync();

            TopFiveViewModel model = new();
            string message = string.Empty;
            string username = string.Empty;
            int projectId = 0;
            double point = 0;

            if (gitLabEvent == "Push Hook")
            {
                _logger.LogDebug("Incoming push hook event");
                model.WebHookResponse.WebHookCommitResponse = JsonSerializer.Deserialize<WebHookCommitResponse>(json);
                message = $"{model.WebHookResponse.WebHookCommitResponse.user_name} made a push to {model.WebHookResponse.WebHookCommitResponse.project.name}"
                    + $", ";
                username = model.WebHookResponse.WebHookCommitResponse.user_username;
                projectId = model.WebHookResponse.WebHookCommitResponse.project_id;
                point = 0.15;
            }

            else if (gitLabEvent == "Issue Hook")
            {
                _logger.LogDebug("Incoming issue hook event");
                model.WebHookResponse.WebHookIssueResponse = JsonSerializer.Deserialize<WebHookIssueResponse>(json);
                message = $"{model.WebHookResponse.WebHookIssueResponse.user.name} " +
                    $"{model.WebHookResponse.WebHookIssueResponse.object_attributes.state} the issue {model.WebHookResponse.WebHookIssueResponse.object_attributes.title}"
                    + $", ";
                username = model.WebHookResponse.WebHookIssueResponse.user.username;
                point = 0.3;
                projectId = model.WebHookResponse.WebHookIssueResponse.project.id;
                if (model.WebHookResponse.WebHookIssueResponse.object_attributes.state == "opened")
                    point = 0.15;
            }

            else if (gitLabEvent == "Merge Request Hook")
            {
                _logger.LogDebug("Incoming merge hook event");
                model.WebHookResponse.WebHookMergeResponse = JsonSerializer.Deserialize<WebHookMergeResponse>(json);
                message = $"{model.WebHookResponse.WebHookMergeResponse.user.name} {model.WebHookResponse.WebHookMergeResponse.object_attributes.state} " +
                    $"from {model.WebHookResponse.WebHookMergeResponse.object_attributes.source_branch} to {model.WebHookResponse.WebHookMergeResponse.object_attributes.target_branch}" +
                    $" on project {model.WebHookResponse.WebHookMergeResponse.project.name}, ";
                username = model.WebHookResponse.WebHookMergeResponse.user.username;
                point = 0.35;
                projectId = model.WebHookResponse.WebHookMergeResponse.project.id;
            }

            else if (gitLabEvent == "Note Hook")
            {
                _logger.LogDebug("Incoming note hook event");
                model.WebHookResponse.WebHookCommentResponse = JsonSerializer.Deserialize<WebHookCommentResponse>(json);
                message = $"{model.WebHookResponse.WebHookCommentResponse.user.name} commented on {model.WebHookResponse.WebHookCommentResponse.project.name}"
                    + $", ";
                username = model.WebHookResponse.WebHookCommentResponse.user.username;
                point = 0.05;
                projectId = model.WebHookResponse.WebHookCommentResponse.project_id;
            }

            try
            {
                _logger.LogDebug("Updating userdata and rating");
                var data = await _userService.UpdateUserData(username, model, gitLabEvent);
                await _rankingService.CalculateRating(data, projectId);
            }
            catch (Exception e)
            {
                _logger.LogDebug("Could not update userdata or rating");
                _logger.LogError(e.Message);
                return StatusCode(500);
            }

            if (message != string.Empty)
            {
                TopFiveLiveUpdateModel liveUpdateModel = new();

                string updateduserlevel = await _userService.UpdateUserLevel(username, point);
                if (!string.IsNullOrWhiteSpace(updateduserlevel))
                {
                    await _liveFeedHubContext.Clients.All.SendAsync("ReceiveMessage", updateduserlevel);
                }

                liveUpdateModel.TopFiveGroups = await _rankingService.GetTopFiveGroups();
                liveUpdateModel.TopFiveUsers = await _rankingService.GetTopFiveUsers();
                liveUpdateModel.TopFiveUserLevels = await _rankingService.GetTopFiveHighestLevels();
                liveUpdateModel.LiveFeedMessage = message;
                string getTopFiveJson = string.Empty;

                try
                {
                    getTopFiveJson = JsonSerializer.Serialize(liveUpdateModel);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                    return StatusCode(500);
                }

                await _liveFeedHubContext.Clients.All.SendAsync("ReceiveMessage", getTopFiveJson);

                return Ok();
            }

            return BadRequest();
        }

        [IgnoreAntiforgeryToken]
        [HttpPost]
        [Route("receivegroups")]
        public async Task<IActionResult> ReceiveGroups()
        {
            Request.Headers.TryGetValue("X-Gitlab-Event", out StringValues gitLabEvent);
            Request.Headers.TryGetValue("X-Gitlab-Token", out StringValues gitLabSignature);

            if (gitLabSignature.FirstOrDefault() != _config["Authentication-GitLab-WebHookAuthenticationGroups"])
                return Unauthorized();

            using var reader = new StreamReader(Request.Body);
            var json = await reader.ReadToEndAsync();

            if (gitLabEvent == "Member Hook")
            {
                WebHookMemberResponse response = new();
                try
                {
                     response = JsonSerializer.Deserialize<WebHookMemberResponse>(json);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                if (response.event_name == "user_add_to_group")
                {
                    if (await _groupService.AddUserToGroup(response))
                        return Ok();
                }
                else if (response.event_name == "user_remove_from_group")
                {
                    if (await _groupService.RemoveUserFromGroup(response))
                        return Ok();
                }
            }
            else if (gitLabEvent == "Subgroup Hook")
            {
                WebHookSubGroupResponse response = JsonSerializer.Deserialize<WebHookSubGroupResponse>(json);

                if (response.event_name == "subgroup_create")
                {
                    if (await _groupService.CreateGroup(response))
                        return Ok();
                }
                else if (response.event_name == "subgroup_destroy")
                {
                    if (await _groupService.RemoveGroup(response.group_id))
                        return Ok();
                }
            }

            return BadRequest();
        }
    }
}
