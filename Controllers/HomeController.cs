using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using HackerRank.Models;
using HackerRank.Models.Users;
using HackerRank.Services;

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

        public async Task<IActionResult> Data()
        {
            await _groupService.GetGroupData();
            await _groupService.GetProjectIdsForGroups();
            await _userService.GetAllUserData(7);

            await _rankingService.CalculateAllUsersRating(true);
            await _rankingService.CalculateAllUsersRating(false);
            await _rankingService.CalculateAllGroupRating();

            return View();
        }

        [IgnoreAntiforgeryToken]
        [HttpPost]
        [Route("search/{username}")]
        public IActionResult Search(string username)
        {
            return Json(_userService.UserSearch(username));
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
