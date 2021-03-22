using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HackerRank.Services;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HackerRank.Controllers
{
    public class RankingController : Controller
    {
        private readonly IRankingService _rankingService;
        private readonly IGroupService _groupService;
        private readonly IUserService _userService;

        public RankingController(IRankingService rankingService, IGroupService groupService, IUserService userService)
        {
            _rankingService = rankingService;
            _groupService = groupService;
            _userService = userService;
        }

        // GET: HomeController1
        public async Task<IActionResult> Index()
        {
            //await _userService.GetAllUserData();
            await _rankingService.UpdateUserStats();
            return View(await _rankingService.ListAllUsers());
        }

        // GET: HomeController1/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: HomeController1/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: HomeController1/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: HomeController1/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: HomeController1/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: HomeController1/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: HomeController1/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
