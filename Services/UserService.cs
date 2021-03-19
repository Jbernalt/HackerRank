using HackerRank.Data;
using HackerRank.Models.Achievements;
using HackerRank.Models.Users;
using HackerRank.Responses;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

using static HackerRank.Models.ActionTypes;

namespace HackerRank.Services
{
    public interface IUserService
    {

    }

    public class UserService : IUserService
    {
        private readonly HackerRankContext _context;
        private readonly UserManager<User> _userManager;

        public UserService(HackerRankContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task UpdateUserAchivements(string username)
        {
            var user = await _context.Users.Where(u => u.UserName == username).FirstOrDefaultAsync();
            var achievements = _context.Achievement.ToArray();
            
            foreach(var a in achievements)
            {
                if (a.TypeOfAction == ActionType.Commit && user.userStats.TotalCommits > a.NumberOfActions)
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
                if (a.TypeOfAction == ActionType.IssueOpened && user.userStats.TotalIssuesCreated > a.NumberOfActions)
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
                if (a.TypeOfAction == ActionType.IssueSolved && user.userStats.TotalIssuesSolved > a.NumberOfActions)
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
                if (a.TypeOfAction == ActionType.MergeRequest && user.userStats.TotalMergeRequests > a.NumberOfActions)
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
                if (a.TypeOfAction == ActionType.Comment && user.userStats.TotalComments > a.NumberOfActions)
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
    }
}
