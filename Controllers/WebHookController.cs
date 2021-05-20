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

using static HackerRank.Models.ActionTypes;

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
        private readonly IAchievementService _achievementService;


        public WebHookController(ILogger<WebHookController> logger, IConfiguration config, IHubContext<LiveFeedHub> liveFeedHubContext, IUserService userService, IGroupService groupService, IRankingService rankingService, IAchievementService achievementService)
        {
            _logger = logger;
            _config = config;
            _liveFeedHubContext = liveFeedHubContext;
            _userService = userService;
            _groupService = groupService;
            _rankingService = rankingService;
            _achievementService = achievementService;

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

            WebHookResponse model = new();
            string message = string.Empty;
            string username = string.Empty;
            string projectname = string.Empty;
            int projectId = 0;
            double point = 0;
            ActionType actionType = ActionType.Commit;

            if (gitLabEvent == "Push Hook")
            {
                _logger.LogDebug("Incoming push hook event");
                model.WebHookCommitResponse = JsonSerializer.Deserialize<WebHookCommitResponse>(json);
                message = $"{model.WebHookCommitResponse.user_name} pushed to {model.WebHookCommitResponse.project.name}, ";
                username = model.WebHookCommitResponse.user_username;
                projectId = model.WebHookCommitResponse.project_id;
                point = 0.15;
                projectname = model.WebHookCommitResponse.project.name;
            }
            else if (gitLabEvent == "Issue Hook")
            {
                _logger.LogDebug("Incoming issue hook event");
                model.WebHookIssueResponse = JsonSerializer.Deserialize<WebHookIssueResponse>(json);
                message = $"{model.WebHookIssueResponse.user.name} {model.WebHookIssueResponse.object_attributes.state} the issue {model.WebHookIssueResponse.object_attributes.title}, ";
                username = model.WebHookIssueResponse.user.username;
                point = 0.3;
                actionType = ActionType.IssueSolved;
                projectId = model.WebHookIssueResponse.project.id;
                if (model.WebHookIssueResponse.object_attributes.state == "opened")
                {
                    point = 0.15;
                    actionType = ActionType.IssueOpened;
                }
                projectname = model.WebHookIssueResponse.project.name;
            }
            else if (gitLabEvent == "Merge Request Hook")
            {
                _logger.LogDebug("Incoming merge hook event");
                model.WebHookMergeResponse = JsonSerializer.Deserialize<WebHookMergeResponse>(json);
                message = $"{model.WebHookMergeResponse.user.name} {model.WebHookMergeResponse.object_attributes.state} " +
                    $"{model.WebHookMergeResponse.object_attributes.source_branch} into {model.WebHookMergeResponse.object_attributes.target_branch}" +
                    $" in the {model.WebHookMergeResponse.project.name} project, ";
                username = model.WebHookMergeResponse.user.username;
                point = 0.35;
                actionType = ActionType.MergeRequest;
                projectId = model.WebHookMergeResponse.project.id;
                projectname = model.WebHookMergeResponse.project.name;
            }
            else if (gitLabEvent == "Note Hook")
            {
                _logger.LogDebug("Incoming note hook event");
                model.WebHookCommentResponse = JsonSerializer.Deserialize<WebHookCommentResponse>(json);
                message = $"{model.WebHookCommentResponse.user.name} commented on {model.WebHookCommentResponse.project.name}"
                    + $", ";
                username = model.WebHookCommentResponse.user.username;
                point = 0.05;
                actionType = ActionType.Comment;
                projectId = model.WebHookCommentResponse.project_id;
                projectname = model.WebHookCommentResponse.project.name;
            }

            try
            {
                await _groupService.GetProjectsGroups(projectId, projectname);
                _logger.LogDebug("Updating userdata and rating");
                var data = await _userService.UpdateUserData(username, model, gitLabEvent);
                if (data == null)
                {
                    _logger.LogDebug("User does not exist in the database or the event is not one which is tracked by the program");
                    return StatusCode(500);
                }

                await _rankingService.CalculateRating(data, projectId);
            }
            catch (Exception e)
            {
                _logger.LogDebug("Could not update userdata or rating");
                _logger.LogError(e.Message);
                return StatusCode(500);
            }

            if (string.IsNullOrWhiteSpace(message))
                return BadRequest();

            TopFiveLiveUpdateModel liveUpdateModel = new();

            liveUpdateModel.LiveFeedMessage = await _userService.UpdateUserLevel(username, point);
            if (!string.IsNullOrWhiteSpace(liveUpdateModel.LiveFeedMessage))
                await SendLiveFeedMessage(liveUpdateModel);

            liveUpdateModel.LiveFeedMessage = await _achievementService.UpdateAchievementsOnUser(username.ToUpper(), actionType);
            if (!string.IsNullOrWhiteSpace(liveUpdateModel.LiveFeedMessage))
                await SendLiveFeedMessage(liveUpdateModel);

            liveUpdateModel.TopFiveGroups = await _rankingService.GetTopFiveGroups();
            liveUpdateModel.TopFiveUsers = await _rankingService.GetTopFiveUsers();
            liveUpdateModel.TopFiveUserLevels = await _rankingService.GetTopFiveHighestLevels();
            liveUpdateModel.LiveFeedMessage = message;

            var result = await SendLiveFeedMessage(liveUpdateModel);

            return result;
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

        public async Task<IActionResult> SendLiveFeedMessage(TopFiveLiveUpdateModel liveUpdateModel)
        {
            string achievementMessage;
            try
            {
                achievementMessage = JsonSerializer.Serialize(liveUpdateModel);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return StatusCode(500);
            }
            await _liveFeedHubContext.Clients.All.SendAsync("ReceiveMessage", achievementMessage);
            return Ok();
        }
    }
}
