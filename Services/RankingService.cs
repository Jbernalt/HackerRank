using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HackerRank.Data;
using HackerRank.Models.Groups;
using HackerRank.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HackerRank.ViewModels;

namespace HackerRank.Services
{
    public interface IRankingService
    {
        Task UpdateUserStats();
        Task CalculateDailyRating(User user);
        Task CalculateMonthlyRating(User user);
        Task<List<TopFiveUsersViewModel>> GetTopFiveUsers();
        Task<List<TopFiveGroupsViewModel>> GetTopFiveGroups();
    }

    public class RankingService : IRankingService
    {
        private readonly HackerRankContext _context;
        private readonly UserManager<User> _userManager;

        public RankingService(HackerRankContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task CalculateDailyRating(User user)
        {
            var today = DateTime.UtcNow;
            var transaction = await _context.Transaction.ToArrayAsync();
            var usertransaction = await _context.UserTransaction.Where(t => t.FetchDate.Year == today.Year && t.FetchDate.Month == today.Month && t.FetchDate.Day == today.Day && t.UserId == user.Id).ToArrayAsync();
            if(usertransaction != null)
            {
                var dailyRating =
                    transaction[0].Points * usertransaction[0].Value +
                    transaction[1].Points * usertransaction[1].Value +
                    transaction[2].Points * usertransaction[2].Value +
                    transaction[3].Points * usertransaction[3].Value +
                    transaction[4].Points * usertransaction[4].Value;
                    user.UserStats.DailyRating = dailyRating;
            }
            else
            {
                Console.WriteLine($"Error when calculating daily rating for user {user.UserName}. User transactions returned null");
            }

        }

        public async Task CalculateMonthlyRating(User user)
        {
            var today = DateTime.UtcNow.AddMonths(-1);
            var transaction = await _context.Transaction.ToArrayAsync();
            var usertransaction = _context.UserTransaction.Where(t => t.FetchDate.Year == today.Year && t.FetchDate.Month == today.Month).ToArray();
           
            if(usertransaction != null)
            {
                var monthly = usertransaction.GroupBy
                    (t => t.TransactionId,
                     t => t.Value,
                    (key, v) => new { TransactionId = key, Value = v.ToArray() }).ToArray();

                var monthlyRating =
                    (transaction[0].Points * monthly[0].Value[0] +
                    transaction[1].Points * monthly[1].Value[0] +
                    transaction[2].Points * monthly[2].Value[0] +
                    transaction[3].Points * monthly[3].Value[0] +
                    transaction[4].Points * monthly[4].Value[0]) /
                    DateTime.DaysInMonth(today.Year, today.Month);

                user.UserStats.MonthlyRating = monthlyRating;
            }
            else
            {
                Console.WriteLine($"Error when calculating montly rating for user {user.UserName}. User transactions returned null");
            }
        }

        public async Task UpdateUserStats()
        {
            var users = await _context.Users.Include("UserStats").ToListAsync();

            foreach (var u in users)
            {
                if(u != null)
                {
                    var usertransaction = await _context.UserTransaction.Where(t => t.UserId == u.Id).ToArrayAsync();

                    if (usertransaction != null)
                    {
                        foreach (var t in usertransaction)
                        {
                            if (t.TransactionId == 1)
                            {
                                u.UserStats.TotalCommits += t.Value;
                            }
                            if (t.TransactionId == 2)
                            {
                                u.UserStats.TotalIssuesCreated += t.Value;
                            }
                            if (t.TransactionId == 3)
                            {
                                u.UserStats.TotalIssuesSolved += t.Value;
                            }
                            if (t.TransactionId == 4)
                            {
                                u.UserStats.TotalMergeRequests += t.Value;
                            }
                            if (t.TransactionId == 5)
                            {
                                u.UserStats.TotalComments += t.Value;
                            }
                        }
                        await CalculateDailyRating(u);
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    Console.WriteLine("Could not find user when updating stats. User returned null");
                }
            }
        }

        public async Task<List<TopFiveGroupsViewModel>> GetTopFiveGroups()
        {
            var today = DateTime.UtcNow;
            List<Group> groups = await _context.Group.Include("Users").OrderByDescending(t => t.GroupRating).Take(5).ToListAsync();
            List<TopFiveGroupsViewModel> topFiveGroups = new();
            
            foreach(var group in groups)
            {
                TopFiveGroupsViewModel topGroup = new() 
                { 
                    GroupName = group.GroupName,
                    GroupRating = group.GroupRating
                };

                foreach(var user in group.Users)
                {
                    var usertransaction = _context.UserTransaction.Where(t => t.FetchDate.Year == today.Year && t.FetchDate.Month == today.Month && t.FetchDate.Day == today.Day && t.UserId == user.Id).ToArray();
                    topGroup.CommitsDaily += usertransaction[0].Value;
                    topGroup.IssuesCreatedDaily += usertransaction[1].Value;
                    topGroup.IssuesSolvedDaily += usertransaction[2].Value;
                    topGroup.MergeRequestsDaily += usertransaction[3].Value;
                    topGroup.CommentsDaily += usertransaction[4].Value;
                }
                topFiveGroups.Add(topGroup);
            }

            return topFiveGroups;
        }

        public async Task<List<TopFiveUsersViewModel>> GetTopFiveUsers()
        {
            var today = DateTime.UtcNow;
            List<TopFiveUsersViewModel> topFiveUsersViewModel = new();
            List<User> users = await _context.Users.Include("UserStats").OrderByDescending(x => x.UserStats.DailyRating).Take(5).ToListAsync();
           
            foreach(var user in users)
            {
                var usertransaction = _context.UserTransaction.Where(t => t.FetchDate.Year == today.Year && t.FetchDate.Month == today.Month && t.FetchDate.Day == today.Day && t.UserId == user.Id).ToArray();
                topFiveUsersViewModel.Add(  new() 
                { 
                    UserName = user.UserName, 
                    Commits = usertransaction[0].Value, 
                    IssuesCreated = usertransaction[1].Value, 
                    IssuesSolved = usertransaction[2].Value, 
                    MergeRequest = usertransaction[3].Value, 
                    Comments = usertransaction[4].Value,
                    DailyRating = user.UserStats.DailyRating,
                    MonthlyRating = user.UserStats.MonthlyRating
                });
            }
            return topFiveUsersViewModel;
        }
    }
}
