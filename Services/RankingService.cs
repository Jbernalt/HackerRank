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
using AutoMapper;

namespace HackerRank.Services
{
    public interface IRankingService
    {

        public Task CalculateAllUsersRating(bool monthly);
        public Task<TopFiveViewModel> GetTopFive();
        public Task CalculateAllGroupRating();
        public Task<List<TopFiveUsersViewModel>> GetTopFiveUsers();
        public Task<List<TopFiveGroupsViewModel>> GetTopFiveGroups();
        public Task<List<UserLevel>> GetTopFiveHighestLevels();
        public Task ResetDailyStats();
    }

    public class RankingService : IRankingService
    {
        private readonly HackerRankContext _context;
        private readonly IMapper _mapper;

        public RankingService(HackerRankContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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

        public async Task CalculateAllGroupRating()
        {
            List<Group> groups = await _context.Group.Include("Users").Include(g => g.GroupStats).Include("Projects").ToListAsync();

            List<Transaction> transactions = await _context.Transaction.ToListAsync();

            foreach (var group in groups)
            {
                double rating = 0;
                group.GroupStats.CommitsDaily = 0;
                group.GroupStats.IssuesCreatedDaily = 0;
                group.GroupStats.IssuesSolvedDaily = 0;
                group.GroupStats.CommentsDaily = 0;

                foreach (var user in group.Users)
                {
                    var usertransaction = _context.UserTransaction.Where(t => t.FetchDate.Date == DateTime.UtcNow.Date && t.UserId == user.Id).AsEnumerable().GroupBy(i => new { i.Project.Id, i.TransactionId }).ToArray();
                    foreach (var t in usertransaction)
                    {
                        if (group.Projects.Where(s => s.Id == t.Key.Id).FirstOrDefault() != null && t.Key.TransactionId == 1)
                        {
                            rating += t.Count() * transactions.Where(t => t.TransactionId == 1).FirstOrDefault().Points;
                            group.GroupStats.CommitsDaily += t.Count();
                            group.GroupStats.TotalCommits += t.Count();
                        }
                        else if (group.Projects.Where(s => s.Id == t.Key.Id).FirstOrDefault() != null && t.Key.TransactionId == 2)
                        {
                            rating += t.Count() * transactions.Where(t => t.TransactionId == 2).FirstOrDefault().Points;
                            group.GroupStats.IssuesCreatedDaily += t.Count();
                            group.GroupStats.TotalIssuesCreated += t.Count();
                        }
                        else if (group.Projects.Where(s => s.Id == t.Key.Id).FirstOrDefault() != null && t.Key.TransactionId == 3)
                        {
                            rating += t.Count() * transactions.Where(t => t.TransactionId == 3).FirstOrDefault().Points;
                            group.GroupStats.IssuesSolvedDaily += t.Count();
                            group.GroupStats.TotalIssuesSolved += t.Count();
                        }
                        else if (group.Projects.Where(s => s.Id == t.Key.Id).FirstOrDefault() != null && t.Key.TransactionId == 4)
                        {
                            rating += t.Count() * transactions.Where(t => t.TransactionId == 4).FirstOrDefault().Points;
                            group.GroupStats.MergeRequestsDaily += t.Count();
                            group.GroupStats.TotalMergeRequests += t.Count();
                        }
                        else if (group.Projects.Where(s => s.Id == t.Key.Id).FirstOrDefault() != null && t.Key.TransactionId == 5)
                        {
                            rating += t.Count() * transactions.Where(t => t.TransactionId == 5).FirstOrDefault().Points;
                            group.GroupStats.CommentsDaily += t.Count();
                            group.GroupStats.TotalComments += t.Count();
                        }
                    }
                }
                group.GroupStats.GroupDailyRating = rating / group.Users.Count;
                _context.Update(group);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<List<UserLevel>> GetTopFiveHighestLevels()
        {
            return await _context.UserLevels.Include(i => i.User)
                .Include(o => o.Level)
                .OrderByDescending(o => o.PrestigeLevel)
                .ThenByDescending(p => p.Level.LevelId)
                .ThenByDescending(e => e.CurrentExperience)
                .Take(5)
                .ToListAsync();
        }

        public async Task<List<TopFiveGroupsViewModel>> GetTopFiveGroups()
        {
            List<TopFiveGroupsViewModel> model = new();
            List<Group> groups = await _context.Group.Include(g => g.GroupStats).OrderByDescending(x => x.GroupStats.GroupDailyRating).Take(5).ToListAsync();

            foreach (var group in groups)
            {
                var m = _mapper.Map(group.GroupStats, new TopFiveGroupsViewModel());
                m.GroupName = group.GroupName;
                model.Add(m);
            }
            return model;
        }

        public async Task<List<TopFiveUsersViewModel>> GetTopFiveUsers()
        {
            List<TopFiveUsersViewModel> model = new();
            List<User> users = await _context.Users.Include(u => u.UserStats).OrderByDescending(x => x.UserStats.DailyRating).Take(5).ToListAsync();

            foreach (var user in users)
            {
                var usertransactions = _context.UserTransaction.Where(x => x.FetchDate.Date == DateTime.UtcNow.Date && x.UserId == user.Id).AsEnumerable().GroupBy(i => i.TransactionId).ToArray();
                if (usertransactions.Length != 0)
                {
                    model.Add(new()
                    {
                        Username = user.UserName,
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
            return model;
        }

        public async Task<TopFiveViewModel> GetTopFive()
        {
            TopFiveViewModel topFiveModel = new();
            topFiveModel.TopFiveGroups.AddRange(await GetTopFiveGroups());
            topFiveModel.TopFiveUsers.AddRange(await GetTopFiveUsers());
            topFiveModel.UserLevel.AddRange(await GetTopFiveHighestLevels());
            return topFiveModel;
        }

        //Resets daily stats on groups and dailyrating on users (should be run once a day)
        public async Task ResetDailyStats()
        {
            List<User> users = await _context.Users.Include(u => u.UserStats).ToListAsync();
            List<Group> groups = await _context.Group.Include(u => u.GroupStats).ToListAsync();

            foreach (var user in users)
            {
                user.UserStats.DailyRating = 0;
            }

            foreach (var group in groups)
            {
                group.GroupStats.CommentsDaily = 0;
                group.GroupStats.CommitsDaily = 0;
                group.GroupStats.IssuesCreatedDaily = 0;
                group.GroupStats.IssuesSolvedDaily = 0;
                group.GroupStats.MergeRequestsDaily = 0;
                group.GroupStats.GroupDailyRating = 0;
            }
            await _context.SaveChangesAsync();
        }
    }
}
