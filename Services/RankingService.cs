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

        Task CalculateAllUsersRating(bool monthly);
        Task<List<TopFiveViewModel>> GetTopFive();
        public Task<List<TopFiveViewModel>> CalculateTopFiveGroupRating();
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

        public async Task CalculateAllUsersRating(bool monthly)
        {
            var today = DateTime.UtcNow;

            var transaction = await _context.Transaction.ToArrayAsync();
            UserTransaction[] usertransaction;

            foreach (var user in await _context.Users.ToListAsync())
            {
                if (monthly)
                    usertransaction = _context.UserTransaction.Where(t => t.FetchDate.Year == today.Year && t.FetchDate.Month == today.Month && t.UserId == user.Id).ToArray();
                else
                    usertransaction = await _context.UserTransaction.Where(t => t.FetchDate.Date == today.Date && t.UserId == user.Id).ToArrayAsync();

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

                if (monthly)
                {
                    rating /= DateTime.DaysInMonth(today.Year, today.Month);
                    user.UserStats.MonthlyRating = rating;
                }
                else
                    user.UserStats.DailyRating = rating;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<TopFiveViewModel>> CalculateTopFiveGroupRating()
        {
            List<Group> groups = groups = await _context.Group.Include("Users").Include("Projects").ToListAsync();

            List<Transaction> transactions = await _context.Transaction.ToListAsync();
            List<TopFiveViewModel> topFiveModel = new();

            foreach (var group in groups)
            {
                TopFiveViewModel topFive = new()
                {
                    GroupName = group.GroupName
                };

                foreach (var user in group.Users)
                {
                    var usertransaction = _context.UserTransaction.Where(t => t.FetchDate.Date == DateTime.UtcNow.Date && t.UserId == user.Id).AsEnumerable().GroupBy(i => new { i.Project.Id, i.TransactionId }).ToArray();
                    foreach (var t in usertransaction)
                    {
                        if (group.Projects.Where(s => s.Id == t.Key.Id).FirstOrDefault() != null && t.Key.TransactionId == 1)
                            topFive.CommitsDaily = t.Count();

                        else if (group.Projects.Where(s => s.Id == t.Key.Id).FirstOrDefault() != null && t.Key.TransactionId == 2)
                            topFive.IssuesCreatedDaily = t.Count();

                        else if (group.Projects.Where(s => s.Id == t.Key.Id).FirstOrDefault() != null && t.Key.TransactionId == 3)
                            topFive.IssuesSolvedDaily = t.Count();

                        else if (group.Projects.Where(s => s.Id == t.Key.Id).FirstOrDefault() != null && t.Key.TransactionId == 4)
                            topFive.MergeRequestsDaily = t.Count();

                        else if (group.Projects.Where(s => s.Id == t.Key.Id).FirstOrDefault() != null && t.Key.TransactionId == 5)
                            topFive.CommentsDaily = t.Count();
                    }
                }
                topFive.GroupRating = topFive.CommitsDaily * transactions.Where(t => t.TransactionId == 1).FirstOrDefault().Points
                    + topFive.IssuesCreatedDaily * transactions.Where(t => t.TransactionId == 2).FirstOrDefault().Points
                    + topFive.IssuesSolvedDaily * transactions.Where(t => t.TransactionId == 3).FirstOrDefault().Points
                    + topFive.MergeRequestsDaily * transactions.Where(t => t.TransactionId == 4).FirstOrDefault().Points
                    + topFive.CommentsDaily * transactions.Where(t => t.TransactionId == 5).FirstOrDefault().Points;
                if(topFive.GroupRating != 0)
                    topFiveModel.Add(topFive);
                
            }

            return topFiveModel.OrderByDescending(x => x.GroupRating).Take(5).ToList();
        }



        public async Task<List<TopFiveViewModel>> GetTopFive()
        {
            var today = DateTime.UtcNow;
            List<TopFiveViewModel> topFiveModel = await CalculateTopFiveGroupRating();

            List<User> users = await _context.Users.Include("UserStats").OrderByDescending(x => x.UserStats.DailyRating).Take(5).ToListAsync();

            foreach (var user in users)
            {
                var usertransactions = _context.UserTransaction.Where(x => x.FetchDate.Date == today.Date && x.UserId == user.Id).AsEnumerable().GroupBy(i => i.TransactionId).ToArray();
                if (usertransactions.Count() != 0)
                {
                    topFiveModel.Add(new()
                    {
                        UserName = user.UserName,
                        ProfileImage = user.ProfileImage,
                        Commits = usertransactions.Where(t => t.Key == 1).FirstOrDefault() == null ? 0 : usertransactions.Where(t => t.Key == 1).FirstOrDefault().Count(),
                        IssuesCreated = usertransactions.Where(t => t.Key == 2).FirstOrDefault() == null ? 0 : usertransactions.Where(t => t.Key == 2).FirstOrDefault().Count(),
                        IssuesSolved = usertransactions.Where(t => t.Key == 3).FirstOrDefault() == null ? 0 : usertransactions.Where(t => t.Key == 3).FirstOrDefault().Count(),
                        MergeRequest = usertransactions.Where(t => t.Key == 4).FirstOrDefault() == null ? 0 : usertransactions.Where(t => t.Key == 4).FirstOrDefault().Count(),
                        Comments = usertransactions.Where(t => t.Key == 5).FirstOrDefault() == null ? 0 : usertransactions.Where(t => t.Key == 5).FirstOrDefault().Count(),
                        DailyRating = user.UserStats.DailyRating,
                        MonthlyRating = user.UserStats.MonthlyRating
                    });
                }
            }
            return topFiveModel;
        }
    }
}
