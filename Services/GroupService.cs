using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using HackerRank.Responses;
using System.Text.Json;
using HackerRank.Data;
using HackerRank.Models.Groups;
using HackerRank.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HackerRank.Services
{
    public interface IGroupService
    {
        public Task<List<GroupResponse>> GetData();
        public Task GetMembers(List<GroupResponse> groups);
        public Task CreateGroup(GroupResponse response);
        public Task SummarizeGroup(List<Group> group);

    }

    public class GroupService : IGroupService
    {
        HackerRankContext _context;
        private readonly IConfiguration _config;
        public GroupService(HackerRankContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<List<GroupResponse>> GetData()
        {
            string baseUrl = "https://gitlab.com/api/v4/groups";
            List<GroupResponse> groupResponses = new List<GroupResponse>();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config["Authentication:GitLab:APIKey"]);
                var response = await client.GetAsync(baseUrl);

                var jsonResult = await response.Content.ReadAsStringAsync();

                List<GroupResponse> result = JsonSerializer.Deserialize<List<GroupResponse>>(jsonResult);
                groupResponses.AddRange(result);
            }
            foreach (var g in groupResponses)
            {
                Group group = await _context.Group.Where(i => i.GitlabTeamId == g.id).Include("Users").FirstOrDefaultAsync();
                if (group == null)
                    await CreateGroup(g);
            }
            await _context.SaveChangesAsync();
            return groupResponses;
        }

        public async Task CreateGroup(GroupResponse response)
        {
            Group group = new();
            group.GitlabTeamId = response.id;
            group.GroupName = response.name;

            await _context.Group.AddAsync(group);
        }

        public async Task GetMembers(List<GroupResponse> groups)
        {
            string baseUrlPart1 = "https://gitlab.com/api/v4/groups/";
            string baseUrlPart2 = @"/members";
            foreach (var g in groups)
            {
                List<UserResponse> users = new();
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config["Authentication:GitLab:APIKey"]);
                    var response = await client.GetAsync(baseUrlPart1 + g.id.ToString() + baseUrlPart2);

                    var jsonResult = await response.Content.ReadAsStringAsync();

                    List<UserResponse> result = JsonSerializer.Deserialize<List<UserResponse>>(jsonResult);
                    users.AddRange(result);
                }
                List<User> nonexisting = new();
                Group group = await _context.Group.Where(i => i.GitlabTeamId == g.id).Include("Users").FirstOrDefaultAsync();
                foreach (var u in users)
                {
                    User user = (User)await _context.Users.Where(n => n.UserName == u.UserName).FirstOrDefaultAsync();

                    if (!UserExists(group, user))
                    {
                        user.Groups.Add(group);
                        group.Users.Add(user);
                    }
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task SummarizeGroup(List<Group> groups)
        {
            foreach (var group in groups)
            {
                double groupRating = 0;
                foreach (var user in group.Users)
                {
                    groupRating += user.userStats.DailyRating;
                }
                groupRating /= group.Users.Count();
                group.GroupRating = groupRating;
            }
            await _context.SaveChangesAsync();
        }
  

 

        public bool UserExists(Group group, User user)
        {
            bool result = false;
            if (group.Users != null)
            {
                foreach (var u in group.Users)
                {
                    if (u == user)
                    {
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }
        //public User CreateUser(UserResponse response)
        //{
        //    User user = new()
        //    {
        //        GitLabId = response.id,
        //        UserName = response.username,
        //        DateCreated = DateTime.Now
        //    };
        //    return user;
        //}

    }
}

