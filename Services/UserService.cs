using HackerRank.Data;
using HackerRank.Models.Achievements;
using HackerRank.Models.Users;
using HackerRank.Responses;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using HackerRank.ViewModels;
using AutoMapper;

using static HackerRank.Models.ActionTypes;


namespace HackerRank.Services
{
    public interface IUserService
    {
        Task GetAllUserData();
        Task UpdateUserAchivements(string username);
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
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResult = await response.Content.ReadAsStringAsync();

                        eventResponses = JsonSerializer.Deserialize<List<EventResponse>>(jsonResult);

                        int commitCounter = 0, issuesCreatedCounter = 0, issuesSolvedCounter = 0, mrCounter = 0, commentsCounter = 0;

                        foreach (var e in eventResponses)
                        {
                            if (e.action_name == "pushed to" || e.action_name == "pushed new")
                            {
                                commitCounter++;
                            }
                            else if (e.target_type == "Issue" && e.action_name == "opened")
                            {
                                issuesCreatedCounter++;
                            }
                            else if (e.target_type == "Issue" && e.action_name == "closed")
                            {
                                issuesSolvedCounter++;
                            }
                            else if (e.target_type == "MergeRequest" && e.action_name == "opened")
                            {
                                mrCounter++;
                            }
                            else if (e.action_name == "commented on")
                            {
                                commentsCounter++;
                            }
                        }

                        UserTransaction commit = new()
                        {
                            User = user,
                            FetchDate = DateTime.UtcNow,
                            Transaction = _context.Transaction.Where(t => t.TransactionId == 1).FirstOrDefault(),
                            Value = commitCounter
                        };

                        UserTransaction issuesCreated = new()
                        {
                            User = user,
                            FetchDate = DateTime.UtcNow,
                            Transaction = _context.Transaction.Where(t => t.TransactionId == 2).FirstOrDefault(),
                            Value = issuesCreatedCounter
                        };

                        UserTransaction issuesSolved = new()
                        {
                            User = user,
                            FetchDate = DateTime.UtcNow,
                            Transaction = _context.Transaction.Where(t => t.TransactionId == 3).FirstOrDefault(),
                            Value = issuesSolvedCounter
                        };

                        UserTransaction mergeRequests = new()
                        {
                            User = user,
                            FetchDate = DateTime.UtcNow,
                            Transaction = _context.Transaction.Where(t => t.TransactionId == 4).FirstOrDefault(),
                            Value = mrCounter
                        };

                        UserTransaction comments = new()
                        {
                            User = user,
                            FetchDate = DateTime.UtcNow,
                            Transaction = _context.Transaction.Where(t => t.TransactionId == 5).FirstOrDefault(),
                            Value = commentsCounter
                        };

                        List<UserTransaction> userTransactions = new() { commit, issuesSolved, issuesCreated, mergeRequests, comments };
                        try
                        {
                            await _context.UserTransaction.AddRangeAsync(userTransactions);
                        }
                        catch
                        {
                            Console.WriteLine("Could not add user transactions to database when retrieving userdata");
                        }
                        
                    }
                    else
                    {
                        Console.WriteLine($"Bad request when trying to get userdata. {response.StatusCode}, {response.RequestMessage}");
                    }

                       await _context.SaveChangesAsync();
                } 
            }
        }

        public async Task UpdateUserAchivements(string username)
        {
            var user = await _context.Users.Where(u => u.UserName == username).FirstOrDefaultAsync();
            var achievements = _context.Achievement.ToArray();
            if(user != null)
            {
                foreach (var a in achievements)
                {
                    if (a.TypeOfAction == ActionType.Commit && user.UserStats.TotalCommits > a.NumberOfActions)
                    {
                        UserAchievement userAchievement = new()
                        {
                            IsUnlocked = true,
                            User = user,
                            Achievement = a
                        };

                        await _context.UserAchievement.AddAsync(userAchievement);
                        await _context.SaveChangesAsync();
                    }
                    else if (a.TypeOfAction == ActionType.IssueOpened && user.UserStats.TotalIssuesCreated > a.NumberOfActions)
                    {
                        UserAchievement userAchievement = new()
                        {
                            IsUnlocked = true,
                            User = user,
                            Achievement = a
                        };

                        await _context.UserAchievement.AddAsync(userAchievement);
                        await _context.SaveChangesAsync();
                    }
                    else if (a.TypeOfAction == ActionType.IssueSolved && user.UserStats.TotalIssuesSolved > a.NumberOfActions)
                    {
                        UserAchievement userAchievement = new()
                        {
                            IsUnlocked = true,
                            User = user,
                            Achievement = a
                        };

                        await _context.UserAchievement.AddAsync(userAchievement);
                        await _context.SaveChangesAsync();
                    }
                    else if (a.TypeOfAction == ActionType.MergeRequest && user.UserStats.TotalMergeRequests > a.NumberOfActions)
                    {
                        UserAchievement userAchievement = new()
                        {
                            IsUnlocked = true,
                            User = user,
                            Achievement = a
                        };

                        await _context.UserAchievement.AddAsync(userAchievement);
                        await _context.SaveChangesAsync();
                    }
                    else if (a.TypeOfAction == ActionType.Comment && user.UserStats.TotalComments > a.NumberOfActions)
                    {
                        UserAchievement userAchievement = new()
                        {
                            IsUnlocked = true,
                            User = user,
                            Achievement = a
                        };

                        await _context.UserAchievement.AddAsync(userAchievement);
                        await _context.SaveChangesAsync();
                    }
                }
            }
            else
            {
                Console.WriteLine("Could not find user when updating user achievements");
            }
        }
    }
}
