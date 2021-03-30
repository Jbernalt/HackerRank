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
        Task<List<TopFiveViewModel>> GetTopFive();
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

            double rating = 0;
            foreach (var tran in usertransaction)
            {
                if (tran.Transaction.TransactionId == transaction[0].TransactionId)
                    rating += transaction[0].Points;
                if (tran.Transaction.TransactionId == transaction[1].TransactionId)
                    rating += transaction[1].Points;
                if (tran.Transaction.TransactionId == transaction[2].TransactionId)
                    rating += transaction[2].Points;
                if (tran.Transaction.TransactionId == transaction[3].TransactionId)
                    rating += transaction[3].Points;
                if (tran.Transaction.TransactionId == transaction[4].TransactionId)
                    rating += transaction[4].Points;
            }
            user.UserStats.DailyRating = rating;
        }

        public async Task CalculateMonthlyRating(User user)
        {
            var today = DateTime.UtcNow;

            var transaction = await _context.Transaction.ToArrayAsync();
            var usertransaction = _context.UserTransaction.Where(t => t.FetchDate.Year == today.Year && t.FetchDate.Month == today.Month && t.UserId == user.Id).ToArray();

            double rating = 0;
            foreach (var tran in usertransaction)
            {
                if (tran.Transaction.TransactionId == transaction[0].TransactionId)
                    rating += transaction[0].Points;
                if (tran.Transaction.TransactionId == transaction[1].TransactionId)
                    rating += transaction[1].Points;
                if (tran.Transaction.TransactionId == transaction[2].TransactionId)
                    rating += transaction[2].Points;
                if (tran.Transaction.TransactionId == transaction[3].TransactionId)
                    rating += transaction[3].Points;
                if (tran.Transaction.TransactionId == transaction[4].TransactionId)
                    rating += transaction[4].Points;
            }
            rating /= DateTime.DaysInMonth(today.Year,today.Month);

            user.UserStats.MonthlyRating = rating;
        }

        public async Task UpdateUserStats()
        {
            var users = await _context.Users.Include("UserStats").ToListAsync();

            foreach (var u in users)
            { 
                var usertransaction = await _context.UserTransaction.Where(t => t.UserId == u.Id).ToArrayAsync();

                foreach (var t in usertransaction)
                {
                    if (t.TransactionId == 1)
                        u.UserStats.TotalCommits += 1;

                    if (t.TransactionId == 2)
                        u.UserStats.TotalIssuesCreated += 1;

                    if (t.TransactionId == 3)
                        u.UserStats.TotalIssuesSolved += 1;

                    if (t.TransactionId == 4)
                        u.UserStats.TotalMergeRequests += 1;

                    if (t.TransactionId == 5)
                        u.UserStats.TotalComments += 1;
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<TopFiveViewModel>> GetTopFive()
        {
            var today = DateTime.UtcNow;
            List<Group> groups = await _context.Group.Include("Users").Include("Projects").OrderByDescending(t => t.GroupRating).Take(5).ToListAsync();
            List<TopFiveViewModel> topFiveModel = new();
            
            foreach(var group in groups)
            {
                TopFiveViewModel topFive = new() 
                { 
                    GroupName = group.GroupName,
                    GroupRating = group.GroupRating
                };

                foreach(var user in group.Users)
                {
                    var usertransaction = _context.UserTransaction.Where(t => t.FetchDate.Year == today.Year && t.FetchDate.Month == today.Month && t.FetchDate.Day == today.Day && t.UserId == user.Id).ToArray();
                    foreach (var p in group.Projects)
                    {
                        foreach (var t in usertransaction)
                        {
                            if (p.GitLabId == t.Project.GitLabId)
                            {
                                if (t.TransactionId == 1)
                                    topFive.CommitsDaily += 1;

                                if (t.TransactionId == 2)
                                    topFive.IssuesCreatedDaily += 1;

                                if (t.TransactionId == 3)
                                    topFive.IssuesSolvedDaily += 1;

                                if (t.TransactionId == 4)
                                    topFive.MergeRequestsDaily += 1;

                                if (t.TransactionId == 5)
                                    topFive.CommentsDaily += 1;
                            }
                        }
                    }
                }
                topFive.GroupRating = topFive.CommentsDaily * 0.05 + topFive.CommitsDaily * 0.15 + topFive.MergeRequestsDaily * 0.35 + topFive.IssuesCreatedDaily * 0.15 + topFive.IssuesSolvedDaily * 0.3;
                topFiveModel.Add(topFive);
            }

            List<User> users = await _context.Users.Include("UserStats").OrderByDescending(x => x.UserStats.DailyRating).Take(5).ToListAsync();

            foreach (var user in users)
            {
                await CalculateMonthlyRating(user);
                await CalculateDailyRating(user);
            }

            foreach (var user in users)
            {
                topFiveModel.Add(new()
                {
                    UserName = user.UserName,
                    Commits = user.UserStats.TotalCommits,
                    IssuesCreated = user.UserStats.TotalIssuesCreated,
                    IssuesSolved = user.UserStats.TotalIssuesSolved,
                    MergeRequest = user.UserStats.TotalMergeRequests,
                    Comments = user.UserStats.TotalComments,
                    DailyRating = user.UserStats.DailyRating,
                    MonthlyRating = user.UserStats.MonthlyRating
                });
            }
            return topFiveModel;
        }
    }
}
