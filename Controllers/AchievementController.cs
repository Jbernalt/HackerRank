using System.Threading.Tasks;

using HackerRank.Responses;
using HackerRank.Services;
using HackerRank.ViewModels;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using HackerRank.Models.Users;
using Microsoft.AspNetCore.Identity;

namespace HackerRank.Controllers
{
    public class AchievementController : Controller
    {
        private readonly IAchievementService _achievementService;
        private readonly IUserService _userService;
        private readonly UserManager<User> _userManager;

        public AchievementController(IAchievementService achievementService, IUserService userService, UserManager<User> signInManager)
        {
            _achievementService = achievementService;
            _userService = userService;
            _userManager = signInManager;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(await _achievementService.ListAllAchievements(user.NormalizedUserName));
        }

        [Authorize]
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
        [Authorize]
        public async Task<IActionResult> SetShowcase()
        {
            var achievementIds = Request.Form["IsChecked"].ToList();
            var user = await _userManager.GetUserAsync(User);
            await _achievementService.SetShowCase(achievementIds, user.NormalizedUserName);

            return RedirectToAction("profile", "user", new { id = user.UserName });
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
