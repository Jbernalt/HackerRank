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
using HackerRank.Models.Transactions;

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
            var usertransaction = await _context.UserTransaction.Where(t => t.FetchDate.Date == today.Date && t.UserId == user.Id).ToArrayAsync();

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
            List<Transaction> transactions = await _context.Transaction.ToListAsync();
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
                    var usertransaction = _context.UserTransaction.Where(t => t.FetchDate.Date == today.Date && t.UserId == user.Id).AsEnumerable().GroupBy(i => new { i.Project.Id, i.TransactionId}).ToArray();
                    foreach (var t in usertransaction)
                    {
                        if (group.Projects.Where(s => s.Id == t.Key.Id).FirstOrDefault() != null && t.Key.TransactionId == 1)
                            topFive.CommitsDaily = t.Count();

                        if (group.Projects.Where(s => s.Id == t.Key.Id).FirstOrDefault() != null && t.Key.TransactionId == 2)
                            topFive.IssuesCreatedDaily = t.Count();

                        if (group.Projects.Where(s => s.Id == t.Key.Id).FirstOrDefault() != null && t.Key.TransactionId == 3)
                            topFive.IssuesSolvedDaily = t.Count();

                        if (group.Projects.Where(s => s.Id == t.Key.Id).FirstOrDefault() != null && t.Key.TransactionId == 4)
                            topFive.MergeRequestsDaily = t.Count();

                        if (group.Projects.Where(s => s.Id == t.Key.Id).FirstOrDefault() != null && t.Key.TransactionId == 5)
                            topFive.CommentsDaily = t.Count();
                    }
                }
                topFive.GroupRating = topFive.CommitsDaily * transactions.Where(t => t.TransactionId == 1).FirstOrDefault().Points
                    + topFive.IssuesCreatedDaily * transactions.Where(t => t.TransactionId == 2).FirstOrDefault().Points
                    + topFive.IssuesSolvedDaily * transactions.Where(t => t.TransactionId == 3).FirstOrDefault().Points
                    + topFive.MergeRequestsDaily * transactions.Where(t => t.TransactionId == 4).FirstOrDefault().Points
                    + topFive.CommentsDaily * transactions.Where(t => t.TransactionId == 5).FirstOrDefault().Points;
                topFiveModel.Add(topFive);
            }

            List<User> users = await _context.Users.Include("UserStats").OrderByDescending(x => x.UserStats.DailyRating).Take(5).ToListAsync();

            foreach (var user in users)
            {
                await CalculateMonthlyRating(user);
                await CalculateDailyRating(user);

                var usertransactions = _context.UserTransaction.Where(x => x.FetchDate.Date == today.Date && x.UserId == user.Id).AsEnumerable().GroupBy(i => i.TransactionId).ToArray();
                topFiveModel.Add(new()
                {
                    UserName = user.UserName,
                    Commits = usertransactions.Where(t => t.Key == 1).FirstOrDefault().Count(),
                    IssuesCreated = usertransactions.Where(t => t.Key == 2).FirstOrDefault().Count(),
                    IssuesSolved = usertransactions.Where(t => t.Key == 3).FirstOrDefault().Count(),
                    MergeRequest = usertransactions.Where(t => t.Key == 4).FirstOrDefault().Count(),
                    Comments = usertransactions.Where(t => t.Key == 5).FirstOrDefault().Count(),
                    DailyRating = user.UserStats.DailyRating,
                    MonthlyRating = user.UserStats.MonthlyRating
                });
            }
            return topFiveModel;
        }
    }
}
