using AutoMapper;

using HackerRank.Data;
using HackerRank.Models;
using HackerRank.Models.Achievements;
using HackerRank.Models.Projects;
using HackerRank.Models.Users;
using HackerRank.Responses;
using HackerRank.ViewModels;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        List<ChartData> GetUserChartData(User user);
        Task<Tuple<User, UserTransaction>> UpdateUserData(string username, WebHookResponse model, StringValues gitLabEvent);
        Task<UserViewModel> GetUserByUsername(string username, bool isOwnProfile, bool isAdmin);
        List<string> UserSearch(string username);
        Task<string> UpdateUserLevel(string userName, double points);
        Task<List<UserViewModel>> GetAllUsers();
        Task SetRoles(List<string> roleNames, string userName);
        Task<List<RoleViewModel>> GetUserRoles(string userName);
        public Task AddUserToGroupsOnRegister(User user);
    }

    public class UserService : IUserService
    {
        private readonly HackerRankContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserService(HackerRankContext context, UserManager<User> userManager, IConfiguration configuration, IMapper mapper, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _config = configuration;
            _mapper = mapper;
            _roleManager = roleManager;
        }

        public async Task<List<UserViewModel>> GetAllUsers()
        {
            List<User> users = await _context.Users.Include(x => x.Groups).Include(y => y.UserStats).ToListAsync();
            List<UserViewModel> userViewModels = new();
            _mapper.Map(users, userViewModels);
            int i = 0;
            foreach (var u in users)
            {
                var list = await _userManager.GetRolesAsync(u);
                userViewModels[i].Roles = list.ToList();
                i++;
            }
            return userViewModels;
        }

        public async Task AddUserToGroupsOnRegister(User user)
        {
            var groups = await _context.Group.ToListAsync();
            UriBuilder uriBuilder = new()
            {
                Scheme = "https",
                Host = "gitlab.com/api/v4/groups"
            };

            foreach (var group in groups)
            {
                UserResponse result = new();
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config["Authentication-GitLab-APIKey"]);
                    string path = group.GitlabTeamId.ToString() + $"/members/" + user.GitLabId.ToString();
                    uriBuilder.Path = path;

                    var response = await client.GetAsync(uriBuilder.ToString());
                    if (response.IsSuccessStatusCode)
                    {
                        group.Users.Add(user);
                    }                    
                }
            }
        }

        public List<string> UserSearch(string username)
        {
            List<string> users = _context.Users.Where(u => u.UserName.Contains(username) && u.IsPublic == true).Select(x => x.UserName).ToList();
            return users;
        }

        public async Task<UserViewModel> GetUserByUsername(string username, bool isOwnProfile, bool isAdmin)
        {
            username = username.ToUpper();
            var user = await _context.Users.Where(x => x.NormalizedUserName == username).Include(u => u.UserStats).Include(g => g.Groups).ThenInclude(p => p.Projects).FirstOrDefaultAsync();

            if (user == null)
                return new UserViewModel() { IsPublic = false };

            if (!user.IsPublic && !isOwnProfile && !isAdmin)
                return new UserViewModel() { IsPublic = false };

            var achievements = await _context.UserAchievement.Where(a => a.User == user && a.IsUnlocked == true).Include(a => a.Achievement).Include(a => a.User).ToListAsync();
            var userLevel = await _context.UserLevels.Include("User").Include("Level").Where(u => u.User.NormalizedUserName == username).FirstOrDefaultAsync();
            List<Project> projects = new();
            UserViewModel model = new();

            foreach (var g in user.Groups)
            {
                projects.AddRange(g.Projects);
            }

            var data = GetUserChartData(user);

            _mapper.Map(user, model);
            model.Projects = projects;
            model.UserAchievements = achievements;
            model.ChartDatas = data;
            model.UserLevel = userLevel;
            return model;
        }

        public async Task<Tuple<User, UserTransaction>> UpdateUserData(string username, WebHookResponse model, StringValues gitLabEvent)
        {
            int id = 1;
            int projectId = 0;
            var user = await _context.Users.Where(u => u.NormalizedUserName == username.ToUpper()).Include(s => s.UserStats).FirstOrDefaultAsync();
            if (user == null)
                return null;

            UserTransaction tran = new()
            {
                User = user,
                FetchDate = DateTime.UtcNow,
                UserId = user.Id
            };

            if (gitLabEvent == "Push Hook")
            {
                projectId = model.WebHookCommitResponse.project.id;
                user.UserStats.TotalCommits += 1;
            }
            else if (gitLabEvent == "Issue Hook" && model.WebHookIssueResponse.object_attributes.state == "opened")
            {
                id = 2;
                projectId = model.WebHookIssueResponse.project.id;
                user.UserStats.TotalIssuesCreated += 1;
            }
            else if (gitLabEvent == "Issue Hook")
            {
                id = 3;
                projectId = model.WebHookIssueResponse.project.id;
                user.UserStats.TotalIssuesSolved += 1;
            }
            else if (gitLabEvent == "Merge Request Hook")
            {
                id = 4;
                projectId = model.WebHookMergeResponse.project.id;
                user.UserStats.TotalMergeRequests += 1;
            }
            else if (gitLabEvent == "Note Hook")
            {
                id = 5;
                projectId = model.WebHookCommentResponse.project.id;
                user.UserStats.TotalComments += 1;
            }

            tran.Project = await _context.Project.Where(p => p.GitLabId == projectId).FirstOrDefaultAsync();
            tran.Transaction = await _context.Transaction.Where(t => t.TransactionId == id).FirstOrDefaultAsync();
            tran.TransactionId = id;

            if (tran.Transaction == null)
                return null;
            
            _context.UserTransaction.Add(tran);
            await _context.SaveChangesAsync();

            return new Tuple<User, UserTransaction>(user, tran);
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
                message = $"{userLevel.User.UserName} just leveled up. They are now level {nextLevel.LevelId} {nextLevel.LevelName}, ";
            }
            else
            {
                if (userLevel.CurrentExperience > userLevel.Level.XpNeeded + 10)
                {
                    userLevel.Level = _context.Levels.Find(1);
                    userLevel.CurrentExperience = 0;
                    userLevel.PrestigeLevel += 1;
                    message = $"{userLevel.User.UserName} just prestiged. They are now prestige {userLevel.PrestigeLevel}, level {userLevel.Level.LevelId} {userLevel.Level.LevelName}, ";
                }
            }

            await _context.SaveChangesAsync();
            return message;
        }

        public List<ChartData> GetUserChartData(User user)
        {
            List<ChartData> chart = new();
            var userTransactions = _context.UserTransaction.Include("User").Where(u => u.User == user).AsEnumerable().GroupBy(d => d.FetchDate.ToString("yyyy, MM, dd, HH")).ToArray();

            foreach (var transaction in userTransactions)
            {
                ChartData data = new()
                {
                    TimeStamp = transaction.Key,
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
        
        public async Task SetRoles(List<string> roleNames, string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if(user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var result = await _userManager.RemoveFromRolesAsync(user, roles);
                if (result.Succeeded)
                {
                    result = await _userManager.AddToRolesAsync(user, roleNames);
                    if (result.Succeeded)
                    {
                        await _context.SaveChangesAsync();
                    }
                } 
            }
        }

        public async Task<List<RoleViewModel>> GetUserRoles(string userName)
        {
            List<RoleViewModel> roleViewModels = new();
            var user = await _context.Users.Where(u => u.UserName == userName).FirstOrDefaultAsync();
            var roles = await _roleManager.Roles.ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);

            foreach(var role in roles)
            {
                RoleViewModel viewModel = new()
                {
                    RoleId = role.Id,
                    RoleName = role.Name,
                    UserName = user.UserName,
                    IsInRole = await _userManager.IsInRoleAsync(user, role.Name)
                };
                roleViewModels.Add(viewModel);
            }

            return roleViewModels;
        }
    }
}
