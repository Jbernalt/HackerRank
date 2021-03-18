using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using HackerRank.Models.Groups;
using HackerRank.Responses;
using HackerRank.Services;

using Microsoft.Extensions.Configuration;

namespace HackerRank.Data
{
    public class APIFetchData
    {
        private readonly IGroupService _groupService;
        private readonly IUserService _userService;
        private readonly IConfiguration _config;

        public APIFetchData(IGroupService groupService, IConfiguration config, IUserService userService)
        {
            _groupService = groupService;
            _userService = userService;
            _config = config;
        }
    }
}
