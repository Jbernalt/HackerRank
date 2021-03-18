﻿using HackerRank.Data;
using HackerRank.Models.Users;
using HackerRank.Responses;

using Microsoft.AspNetCore.Identity;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;


namespace HackerRank.Services
{
    public interface IUserService
    {

    }

    public class UserService : IUserService
    {
        HackerRankContext _context;
        UserManager<User> _userManager;
        RoleManager<IdentityRole> _roleManager;

        public UserService(HackerRankContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        //public async Task GetUser()
        //{
        //    string baseUrl = "https://gitlab.com/api/v4/users/7603033/events";
        //    List<UserResponse> userResponses = new List<UserResponse>();

        //    using (var client = new HttpClient())
        //    {
        //        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "Gitlabapikey");
        //        var response = await client.GetAsync(baseUrl);

        //        var jsonResult = await response.Content.ReadAsStringAsync();

        //        List<UserResponse> result = JsonSerializer.Deserialize<List<UserResponse>>(jsonResult);
        //        userResponses.AddRange(result);
        //    }

        //}

    }

}
