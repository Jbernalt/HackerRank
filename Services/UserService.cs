using AutoMapper;

using HackerRank.Data;
using HackerRank.Models.Achievements;
using HackerRank.Models.Projects;
using HackerRank.Models.Users;
using HackerRank.Responses;
using HackerRank.ViewModels;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

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
        Task GetAllUserData();
        Task UpdateAchievementsOnUsers();
        Task<UserViewModel> GetUserByUsername(string username);
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

        public async Task<UserViewModel> GetUserByUsername(string username)
        {
            username = username.ToUpper();
            var user = await _context.Users.Where(x => x.NormalizedUserName == username).Include(u => u.UserStats).Include(g => g.Groups).ThenInclude(p => p.Projects).FirstOrDefaultAsync();
            var achievements = await _context.UserAchievement.Where(a => a.User == user && a.IsUnlocked == true).Include(a => a.Achievement).Include(a => a.User).ToListAsync();
            List<Project> projects = new();
            UserViewModel model = new();

            foreach (var g in user.Groups)
            {
                projects.AddRange(g.Projects);
            }
            _mapper.Map(user, model);
            model.Projects = projects;
            model.UserAchievements = achievements;
            return model;
        }

        public async Task GetAllUserData()
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config["Authentication:GitLab:APIKey"]);

                UriBuilder uriBuilder = new()
                {
                    Scheme = "https",
                    Host = "gitlab.com/api/v4/users"
                };

                List<EventResponse> eventResponses = new();
                var users = await _context.Users.ToArrayAsync();

                foreach (var user in users)
                {
                    string path = user.GitLabId.ToString() + $"/events";
                    uriBuilder.Path = path;
                    uriBuilder.Query = "?per_page=100";
                    var response = await client.GetAsync(uriBuilder.ToString());
                    var jsonResult = await response.Content.ReadAsStringAsync();

                    eventResponses = JsonSerializer.Deserialize<List<EventResponse>>(jsonResult);

                    List<UserTransaction> userTransactions = new();
                    Project[] projects = await _context.Project.ToArrayAsync();

                    foreach (var e in eventResponses)
                    {
                        UserTransaction tran = new();

                        if (e.action_name == "pushed to" || e.action_name == "pushed new")
                        {
                            tran = new()
                            {
                                User = user,
                                FetchDate = DateTime.UtcNow,
                                Transaction = _context.Transaction.Where(t => t.TransactionId == 1).FirstOrDefault(),
                                Project = projects.Where(x => x.GitLabId == e.project_id).FirstOrDefault(),
                                TransactionId = 1,
                                UserId = user.Id
                            };
                        }
                        else if (e.target_type == "Issue" && e.action_name == "opened")
                        {
                            tran = new()
                            {
                                User = user,
                                FetchDate = DateTime.UtcNow,
                                Transaction = _context.Transaction.Where(t => t.TransactionId == 2).FirstOrDefault(),
                                Project = projects.Where(x => x.GitLabId == e.project_id).FirstOrDefault(),
                                TransactionId = 2,
                                UserId = user.Id
                            };
                        }
                        else if (e.target_type == "Issue" && e.action_name == "closed")
                        {
                            tran = new()
                            {
                                User = user,
                                FetchDate = DateTime.UtcNow,
                                Transaction = _context.Transaction.Where(t => t.TransactionId == 3).FirstOrDefault(),
                                Project = projects.Where(x => x.GitLabId == e.project_id).FirstOrDefault(),
                                TransactionId = 3,
                                UserId = user.Id
                            };
                        }
                        else if (e.target_type == "MergeRequest" && e.action_name == "opened")
                        {
                            tran = new()
                            {
                                User = user,
                                FetchDate = DateTime.UtcNow,
                                Transaction = _context.Transaction.Where(t => t.TransactionId == 4).FirstOrDefault(),
                                Project = projects.Where(x => x.GitLabId == e.project_id).FirstOrDefault(),
                                TransactionId = 4,
                                UserId = user.Id
                            };
                        }
                        else if (e.action_name == "commented on")
                        {
                            tran = new()
                            {
                                User = user,
                                FetchDate = DateTime.UtcNow,
                                Transaction = _context.Transaction.Where(t => t.TransactionId == 5).FirstOrDefault(),
                                Project = projects.Where(x => x.GitLabId == e.project_id).FirstOrDefault(),
                                TransactionId = 5,
                                UserId = user.Id
                            };
                        }
                        if (tran.UserId != null)
                            userTransactions.Add(tran);
                    }
                    _context.UserTransaction.AddRange(userTransactions);
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateAchievementsOnUsers()
        {
            var users = await _context.Users.Include("UserStats").ToArrayAsync();
            var achievements = await _context.Achievement.ToArrayAsync();

            foreach (var u in users)
            {
                foreach (var a in achievements)
                {
                    var userachievemnts = await _context.UserAchievement.Include("Achievement").Where(ua => ua.Achievement.AchievementId == a.AchievementId && ua.User.Id == u.Id).FirstOrDefaultAsync();
                    if (a.TypeOfAction == ActionType.Commit && u.UserStats.TotalCommits >= a.NumberOfActions && userachievemnts == null)
                    {
                        UserAchievement userAchievement = new()
                        {
                            IsUnlocked = true,
                            User = u,
                            Achievement = a
                        };

                        await _context.UserAchievement.AddAsync(userAchievement);
                        await _context.SaveChangesAsync();
                    }
                    if (a.TypeOfAction == ActionType.IssueOpened && u.UserStats.TotalIssuesCreated >= a.NumberOfActions && userachievemnts == null)
                    {
                        UserAchievement userAchievement = new()
                        {
                            IsUnlocked = true,
                            User = u,
                            Achievement = a
                        };

                        await _context.UserAchievement.AddAsync(userAchievement);
                        await _context.SaveChangesAsync();
                    }
                    if (a.TypeOfAction == ActionType.IssueSolved && u.UserStats.TotalIssuesSolved >= a.NumberOfActions && userachievemnts == null)
                    {
                        UserAchievement userAchievement = new()
                        {
                            IsUnlocked = true,
                            User = u,
                            Achievement = a
                        };

                        await _context.UserAchievement.AddAsync(userAchievement);
                        await _context.SaveChangesAsync();
                    }
                    if (a.TypeOfAction == ActionType.MergeRequest && u.UserStats.TotalMergeRequests >= a.NumberOfActions && userachievemnts == null)
                    {
                        UserAchievement userAchievement = new()
                        {
                            IsUnlocked = true,
                            User = u,
                            Achievement = a
                        };

                        await _context.UserAchievement.AddAsync(userAchievement);
                        await _context.SaveChangesAsync();
                    }
                    if (a.TypeOfAction == ActionType.Comment && u.UserStats.TotalComments >= a.NumberOfActions && userachievemnts == null)
                    {
                        UserAchievement userAchievement = new()
                        {
                            IsUnlocked = true,
                            User = u,
                            Achievement = a
                        };

                        await _context.UserAchievement.AddAsync(userAchievement);
                        await _context.SaveChangesAsync();
                    }
                }
            }
        }
    }
}
