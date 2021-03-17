using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HackerRank.Data;
using HackerRank.Models.Achievements;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using HackerRank.Services;

namespace HackerRank.Controllers
{
    public class AchievementController : Controller
    {
        private readonly IAchievementService _achievementService;

        public AchievementController(IAchievementService achievementService)
        {
            _achievementService = achievementService;
        }

        // GET: Achievement
        public async Task<IActionResult> Index()
        {
            return View(await _achievementService.ListAllAchievements());
        }

        // GET: Achievement/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Achievement achievement = await _achievementService.Details((int)id);

            return View(achievement);
        }

        // GET: Achievement/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Achievement/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AchievementId,AchievementName,Description")] AchievementViewModel achievementModel, [FromForm(Name = "file")] IFormFile file)
        {
            if (ModelState.IsValid)
            {
                await _achievementService.Create(achievementModel, file);
                return RedirectToAction(nameof(Index));
            }
            return View(achievementModel);
        }

        // GET: Achievement/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var achievement = await _achievementService.FindAchievementModelById((int)id);

            if (achievement == null)
            {
                return NotFound();
            }
            return View(achievement);
        }

        // POST: Achievement/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AchievementId,AchievementName,Description,Image")] AchievementViewModel achievementModel, [FromForm(Name = "editFile")] IFormFile file)
        {
            if (id != achievementModel.AchievementId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _achievementService.Edit(achievementModel, file);
                return RedirectToAction(nameof(Index));
            }

            return View(achievementModel);
        }

        // GET: Achievement/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var achievement = await _achievementService.FindAchievementById((int)id);
            if (achievement == null)
            {
                return NotFound();
            }

            return View(achievement);
        }

        // POST: Achievement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _achievementService.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
