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

        public async Task CalculateRating(User user, UserTransaction[] userTransactions)
        {

            var tran = _context.Transaction.ToArray();
            var dailyrating = tran[0].Points * userTransactions[0].Value + tran[1].Points * userTransactions[2].Value + tran[2].Points * userTransactions[4].Value + tran[3].Points * userTransactions[6].Value + tran[4].Points * userTransactions[8].Value;
            user.UserStats.DailyRating = dailyrating;
            await _context.SaveChangesAsync();
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
                        await _context.SaveChangesAsync();
                    }
                    if (t.TransactionId == 2)
                    {
                        u.UserStats.TotalIssuesCreated += t.Value;
                        await _context.SaveChangesAsync();
                    }
                    if (t.TransactionId == 3)
                    {
                        u.UserStats.TotalIssuesSolved += t.Value;
                        await _context.SaveChangesAsync();
                    }
                    if (t.TransactionId == 4)
                    {
                        u.UserStats.TotalMergeRequests += t.Value;
                        await _context.SaveChangesAsync();
                    }
                    if (t.TransactionId == 5)
                    {
                        u.UserStats.TotalComments += t.Value;
                        await _context.SaveChangesAsync();
                    }
                }
                await CalculateRating(u, usertransaction);
            }
        }

    }
}
