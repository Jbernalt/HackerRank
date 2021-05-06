using AutoMapper;

using HackerRank.Data;
using HackerRank.Models;
using HackerRank.Models.Achievements;
using HackerRank.Models.Projects;
using HackerRank.Models.Users;
using HackerRank.Responses;
using HackerRank.ViewModels;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

using static HackerRank.Models.ActionTypes;

namespace HackerRank.Services
{
    public interface IUserService
    {
        List<ChartData> GetUserCommitChartData(User user);
        Task UpdateUserData(string username, TopFiveViewModel model, StringValues gitLabEvent);
        Task UpdateAchievementsOnUsers();
        Task<UserViewModel> GetUserByUsername(string username, ClaimsPrincipal identity);
        List<string> UserSearch(string username);
        Task<string> UpdateUserLevel(string userName, double points);
    }

    public class UserService : IUserService
    {
        private readonly HackerRankContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;

        public UserService(HackerRankContext context, UserManager<User> userManager, IConfiguration configuration, IMapper mapper)
        {
            _context = context;
            _userManager = userManager;
            _config = configuration;
            _mapper = mapper;
        }

        public List<string> UserSearch(string username)
        {
            List<string> users = _context.Users.Where(u => u.UserName.StartsWith(username) && u.IsPublic == true).Select(x => x.UserName).ToList();
            return users;
        }

        public async Task<UserViewModel> GetUserByUsername(string username, ClaimsPrincipal identity)
        {
            username = username.ToUpper();
            var user = await _context.Users.Where(x => x.NormalizedUserName == username).Include(u => u.UserStats).Include(g => g.Groups).ThenInclude(p => p.Projects).FirstOrDefaultAsync();

            if (user == null)
                return new UserViewModel() { IsPublic = false };

            if (!user.IsPublic && identity.Identity.Name != user.UserName && !identity.IsInRole("Administrator"))
                return new UserViewModel() { IsPublic = false };

            var achievements = await _context.UserAchievement.Where(a => a.User == user && a.IsUnlocked == true).Include(a => a.Achievement).Include(a => a.User).ToListAsync();
            var userLevel = await _context.UserLevels.Include("User").Include("Level").Where(u => u.User.NormalizedUserName == username).FirstOrDefaultAsync();
            List<Project> projects = new();
            UserViewModel model = new();

            foreach (var g in user.Groups)
            {
                projects.AddRange(g.Projects);
            }

            var data = GetUserCommitChartData(user);

            _mapper.Map(user, model);
            model.Projects = projects;
            model.UserAchievements = achievements;
            model.ChartDatas = data;
            model.UserLevel = userLevel;
            return model;
        }

        public async Task UpdateUserData(string username, TopFiveViewModel model, StringValues gitLabEvent)
        {
            int id = 1;
            int projectId = 0;
            var user = await _context.Users.Where(u => u.NormalizedUserName == username.ToUpper()).Include(s => s.UserStats).FirstOrDefaultAsync();
            UserTransaction tran = new()
            {
                User = user,
                FetchDate = DateTime.UtcNow,
                UserId = user.Id
            };

            if (gitLabEvent == "Push Hook")
            {
                projectId = model.WebHookResponse.WebHookCommitResponse.project.id;
                user.UserStats.TotalCommits += 1;
            }
            else if (gitLabEvent == "Issue Hook" && model.WebHookResponse.WebHookIssueResponse.object_attributes.state == "opened")
            {
                id = 2;
                projectId = model.WebHookResponse.WebHookIssueResponse.project.id;
                user.UserStats.TotalIssuesCreated += 1;
            }
            else if (gitLabEvent == "Issue Hook")
            {
                id = 3;
                projectId = model.WebHookResponse.WebHookIssueResponse.project.id;
                user.UserStats.TotalIssuesSolved += 1;
            }
            else if (gitLabEvent == "Merge Request Hook")
            {
                id = 4;
                projectId = model.WebHookResponse.WebHookMergeResponse.project.id;
                user.UserStats.TotalMergeRequests += 1;
            }
            else if (gitLabEvent == "Note Hook")
            {
                id = 5;
                projectId = model.WebHookResponse.WebHookCommentResponse.project.id;
                user.UserStats.TotalComments += 1;
            }

            tran.Project = await _context.Project.Where(p => p.GitLabId == projectId).FirstOrDefaultAsync();
            tran.Transaction = await _context.Transaction.Where(t => t.TransactionId == id).FirstOrDefaultAsync();
            tran.TransactionId = id;

            if (tran.User != null && tran.Transaction != null)
                _context.UserTransaction.Add(tran);

            await _context.SaveChangesAsync();
        }

        public async Task<string> UpdateUserLevel(string userName, double points)
        {
            string message = string.Empty;
            UserLevel userLevel = await _context.UserLevels.Include(u => u.User).Include("Level").Where(u => u.User.UserName == userName).FirstOrDefaultAsync();
            var nextLevel = await _context.Levels.Where(i => i.LevelId == userLevel.Level.LevelId + 1).FirstOrDefaultAsync();
            userLevel.CurrentExperience += points;

            if (nextLevel != null && userLevel.CurrentExperience >= nextLevel.XpNeeded)
            {
                userLevel.Level = nextLevel;
                message = $"{userLevel.User.UserName} just leveled up. They are now level {nextLevel.LevelId} {nextLevel.LevelName}, {DateTime.UtcNow:dddd, dd MMMM yyyy HH:mm}";
            }
            else
            {
                if (userLevel.CurrentExperience > userLevel.Level.XpNeeded + 10)
                {
                    userLevel.Level = _context.Levels.Find(1);
                    userLevel.CurrentExperience = 0;
                    userLevel.PrestigeLevel += 1;
                    message = $"{userLevel.User.UserName} just prestiged. They are now prestige {userLevel.PrestigeLevel}, level {userLevel.Level.LevelId} {userLevel.Level.LevelName}, {DateTime.UtcNow:dddd, dd MMMM yyyy HH:mm}";
                }
            }

            await _context.SaveChangesAsync();
            return message;
        }

        public async Task UpdateAchievementsOnUsers()
        {
            var users = await _context.Users.Include("UserStats").ToArrayAsync();
            var achievements = await _context.Achievement.ToArrayAsync();

            foreach (var u in users)
            {
                foreach (var a in achievements)
                {
                    var userachievements = await _context.UserAchievement.Include("Achievement").Where(ua => ua.Achievement.AchievementId == a.AchievementId && ua.User.Id == u.Id).FirstOrDefaultAsync();

                    UserAchievement userAchievement = new()
                    {
                        IsUnlocked = true,
                        User = u,
                        Achievement = a
                    };

                    if (a.TypeOfAction == ActionType.Commit && u.UserStats.TotalCommits >= a.NumberOfActions && userachievements == null)
                    {
                        await _context.UserAchievement.AddAsync(userAchievement);
                    }
                    else if (a.TypeOfAction == ActionType.IssueOpened && u.UserStats.TotalIssuesCreated >= a.NumberOfActions && userachievements == null)
                    {
                        await _context.UserAchievement.AddAsync(userAchievement);
                    }
                    else if (a.TypeOfAction == ActionType.IssueSolved && u.UserStats.TotalIssuesSolved >= a.NumberOfActions && userachievements == null)
                    {
                        await _context.UserAchievement.AddAsync(userAchievement);
                    }
                    else if (a.TypeOfAction == ActionType.MergeRequest && u.UserStats.TotalMergeRequests >= a.NumberOfActions && userachievements == null)
                    {
                        await _context.UserAchievement.AddAsync(userAchievement);
                    }
                    else if (a.TypeOfAction == ActionType.Comment && u.UserStats.TotalComments >= a.NumberOfActions && userachievements == null)
                    {
                        await _context.UserAchievement.AddAsync(userAchievement);
                    }

                    await _context.SaveChangesAsync();
                }
            }
        }

        public List<ChartData> GetUserCommitChartData(User user)
        {
            List<ChartData> chart = new();
            var userTransactions = _context.UserTransaction.Include("User").Where(u => u.User == user).AsEnumerable().GroupBy(d => d.FetchDate.Date).ToArray();

            foreach (var transaction in userTransactions)
            {
                ChartData data = new()
                {
                    TimeStamp = transaction.Key.Date,
                    NumOfCommits = transaction.Where(u => u.TransactionId == 1).Count(),
                    NumOfIssuesCreated = transaction.Where(u => u.TransactionId == 2).Count(),
                    NumOfIssuesSolved = transaction.Where(u => u.TransactionId == 3).Count(),
                    NumOfMergeRequests = transaction.Where(u => u.TransactionId == 4).Count(),
                    NumOfComments = transaction.Where(u => u.TransactionId == 5).Count()
                };
                chart.Add(data);
            }
            return chart;
        }
    }
}
