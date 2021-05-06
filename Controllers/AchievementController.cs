using System.Threading.Tasks;

using HackerRank.Responses;
using HackerRank.Services;
using HackerRank.ViewModels;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace HackerRank.Controllers
{
    public class AchievementController : Controller
    {
        private readonly IAchievementService _achievementService;
        private readonly IUserService _userService;

        public AchievementController(IAchievementService achievementService, IUserService userService)
        {
            _achievementService = achievementService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var user = User;
            await _userService.UpdateAchievementsOnUsers();
            return View(await _achievementService.ListAllAchievements(user));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            AchievementViewModel achievement = await _achievementService.Details((int)id);

            return View(achievement);
        }

        [HttpPost]
        public async Task<IActionResult> SetShowcase()
        {
            var a = Request.Form["IsChecked"].ToList();
            await _achievementService.SetShowCase(a, User);

            return RedirectToAction("profile", "user", new { id = User.Identity.Name });
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create([Bind("AchievementId,AchievementName,Description,NumberOfActions,TypeOfAction")] AchievementResponse achievementModel, [FromForm(Name = "file")] IFormFile file)
        {
            if (ModelState.IsValid)
            {
                await _achievementService.Create(achievementModel, file);
                return RedirectToAction(nameof(Index));
            }
            return View(achievementModel);
        }

        [Authorize(Roles = "Administrator")]
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, [Bind("AchievementId,AchievementName,Description,NumberOfActions,TypeOfAction")] AchievementResponse achievementModel, [FromForm(Name = "editFile")] IFormFile file)
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

        [Authorize(Roles = "Administrator")]
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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _achievementService.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
