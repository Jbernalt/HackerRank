using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HackerRank.Models.Users;
using Microsoft.AspNetCore.Http;
using HackerRank.Data;
using HackerRank.Services;
using System.IO;

namespace HackerRank.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly HackerRankContext _context;
        private readonly IImageService _imageService;

        public IndexModel(UserManager<User> userManager, SignInManager<User> signInManager, HackerRankContext context, IImageService imageService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _imageService = imageService;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Profile image")]
            public IFormFile Image { get; set; }

            [Display(Name = "Description")]
            public string Description { get; set; }
        }

        private async Task LoadAsync(User user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                Description = user.Description
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(IFormFile formFile)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }
            
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            if(user.Description != Input.Description)
            {
                user.Description = Input.Description;
            }

            if (HttpContext.Request.Form.Files.Count > 0)
            {
                var _tempProfileImg = user.ProfileImage;

                var file = HttpContext.Request.Form.Files[0];
                user.ProfileImage = await _imageService.SaveImage(file, true) ?? _tempProfileImg;

                if (_tempProfileImg != user.ProfileImage)
                {
                    _imageService.DeleteImage(_tempProfileImg, true);
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}
