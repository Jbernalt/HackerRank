using HackerRank.Data;
using HackerRank.Models.Users;
using HackerRank.Responses;
using HackerRank.Services;
using HackerRank.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace HackerRank.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<ActionResult> Profile(string id)
        {
            return View(await _userService.GetUserByUsername(id, User));
        }

        public async Task<IActionResult> AdminOptions(string id)
        {
            var roles = await _userService.GetUserRoles(id);
            return View(roles);
        }
        [HttpPost]
        public async Task<ActionResult> SetRoles()
        {
            var roleNames = Request.Form["roleCheck"].ToList();
            var userName = Request.Form["userRole"].ToString();
            var array = userName.Split(',');

            await _userService.SetRoles(roleNames, array[0]);
            return RedirectToAction("profile", "user", new { id = array[0] });
        }
    }
}
