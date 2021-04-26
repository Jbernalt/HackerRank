using HackerRank.Data;
using HackerRank.Models.Users;
using HackerRank.Responses;
using HackerRank.Services;

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

        // GET: user/details/username
        [Route("/user/details/{username}")]
        public async Task<ActionResult> Details(string username)
        {
            return View(await _userService.GetUserByUsername(username, User));
        }
    }
}
