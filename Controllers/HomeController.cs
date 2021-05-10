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
        public async Task<IActionResult> AllUsers()
        {
            var lsit = await _userService.GetAllUsers();
            return View(lsit);
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
