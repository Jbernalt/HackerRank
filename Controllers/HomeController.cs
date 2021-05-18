using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using HackerRank.Models;
using HackerRank.Models.Users;
using HackerRank.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HackerRank.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRankingService _rankingService;
        private readonly IGroupService _groupService;
        private readonly IUserService _userService;

        public HomeController(IRankingService rankingService, IGroupService groupService, IUserService userService)
        {
            _rankingService = rankingService;
            _groupService = groupService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["UserList"] = new List<string>();
            var topFive = await _rankingService.GetTopFive();

            return View(topFive);
        }

        [IgnoreAntiforgeryToken]
        [HttpPost]
        [Route("search/{username}")]
        public IActionResult Search(string username)
        {
            return Json(_userService.UserSearch(username));
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AllUsers(string sortOrder)
        {
            var list = await _userService.GetAllUsers();
            
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["MonthlySortParm"] = sortOrder == "monthly_rating_desc" ? "monthly_rating" : "monthly_rating_desc";
            ViewData["DailySortParm"] = sortOrder == "daily_rating_desc" ? "daily_rating" : "daily_rating_desc";
            ViewData["CommitSortParm"] = sortOrder == "commit_desc" ? "commit" : "commit_desc";
            ViewData["IssueCreatedSortParm"] = sortOrder == "issue_created_desc" ? "issue_created" : "issue_created_desc";
            ViewData["IssueSolvedSortParm"] = sortOrder == "issue_solved_desc" ? "issue_solved" : "issue_solved_desc";
            ViewData["MergeSortParm"] = sortOrder == "merge_desc" ? "merge" : "merge_desc";
            ViewData["CommentSortParm"] = sortOrder == "comment_desc" ? "comment" : "comment_desc";

            list = sortOrder switch
            {
                "name_desc" => list.OrderByDescending(s => s.Username).ToList(),
                "monthly_rating" => list.OrderBy(s => s.UserStats.MonthlyRating).ToList(),
                "monthly_rating_desc" => list.OrderByDescending(s => s.UserStats.MonthlyRating).ToList(),
                "daily_rating" => list.OrderBy(s => s.UserStats.DailyRating).ToList(),
                "daily_rating_desc" => list.OrderByDescending(s => s.UserStats.DailyRating).ToList(),
                "commit" => list.OrderBy(s => s.UserStats.TotalCommits).ToList(),
                "commit_desc" => list.OrderByDescending(s => s.UserStats.TotalCommits).ToList(),
                "issue_created" => list.OrderBy(s => s.UserStats.TotalIssuesCreated).ToList(),
                "issue_created_desc" => list.OrderByDescending(s => s.UserStats.TotalIssuesCreated).ToList(),
                "issue_solved" => list.OrderBy(s => s.UserStats.TotalIssuesSolved).ToList(),
                "issue_solved_desc" => list.OrderByDescending(s => s.UserStats.TotalIssuesSolved).ToList(),
                "merge" => list.OrderBy(s => s.UserStats.TotalMergeRequests).ToList(),
                "merge_desc" => list.OrderByDescending(s => s.UserStats.TotalMergeRequests).ToList(),
                "comment" => list.OrderBy(s => s.UserStats.TotalComments).ToList(),
                "comment_desc" => list.OrderByDescending(s => s.UserStats.TotalComments).ToList(),
                _ => list.OrderBy(s => s.Username).ToList(),
            };

            return View(list);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
