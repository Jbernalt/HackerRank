using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HackerRank.Data;
using HackerRank.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HackerRank.Services
{
    public interface IRankingService
    {
        Task<List<User>> ListAllUsers();
        Task UpdateUserStats();
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

        public async Task<List<User>> ListAllUsers()
        {
            return await _userManager.Users.ToListAsync();
        }

        public async Task CalculateDailyRating(User user)
        {
            var today = DateTime.UtcNow;
            int year = today.Year;
            int month = today.Month;
            int day = today.Day;

            var transaction = await _context.Transaction.ToArrayAsync();
            var usertransaction = await _context.UserTransaction.Where(t => t.FetchDate.Year == year && t.FetchDate.Month >= month && t.FetchDate.Day >= day).ToArrayAsync();
            var dailyRating = transaction[0].Points * usertransaction[0].Value + transaction[1].Points * usertransaction[1].Value + transaction[2].Points * usertransaction[2].Value + transaction[3].Points * usertransaction[3].Value + transaction[4].Points * usertransaction[4].Value;
            user.UserStats.DailyRating = dailyRating;
        }

        public async Task CalculateMonlthlyRating(User user)
        {
            var today = DateTime.UtcNow;
            int year = today.Year;
            int month = today.Month;

            var transaction = await _context.Transaction.ToArrayAsync();
            var usertransaction = await _context.UserTransaction.Where(t => t.FetchDate.Year == year && t.FetchDate.Month >= month).ToArrayAsync();
            var monthlyRating = transaction[0].Points * usertransaction[0].Value + transaction[1].Points * usertransaction[1].Value + transaction[2].Points * usertransaction[2].Value + transaction[3].Points * usertransaction[3].Value + transaction[4].Points * usertransaction[4].Value;
            user.UserStats.MonthlyRating = monthlyRating;
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
            }
            await _context.SaveChangesAsync();
        }

    }
}
