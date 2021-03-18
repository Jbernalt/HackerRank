using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using HackerRank.Responses;
using System.Text.Json;
using HackerRank.Data;
using HackerRank.Models.GitLabGroups;
using HackerRank.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace HackerRank.Services
{
    public interface IGroupService
    {
        public Task<List<GroupResponse>> GetData();
        public Task GetMembers(List<GroupResponse> groups);
        public Task CreateGroup(GroupResponse response);
        
    }
    
    public class GroupService : IGroupService
    {
        HackerRankContext _context;
        public GroupService(HackerRankContext context)
        {
            _context = context;
        }

        public async Task<List<GroupResponse>> GetData()
        {
            string baseUrl = "https://gitlab.com/api/v4/groups";
            List<GroupResponse> groupResponses = new List<GroupResponse>();
            using(var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "tr3SszQyyabFd34jNbjU");
                var response = await client.GetAsync(baseUrl);

                var jsonResult = await response.Content.ReadAsStringAsync();

                List<GroupResponse> result = JsonSerializer.Deserialize<List<GroupResponse>>(jsonResult);
                groupResponses.AddRange(result);
            }
            foreach(var g in groupResponses)
            {
                GitLabGroup group = await _context.Group.Where(i => i.GitlabTeamId == g.id).Include("Users").FirstOrDefaultAsync();
               if (group == null)
                    await CreateGroup(g);
            }
            await _context.SaveChangesAsync();
            return groupResponses;
        }

        public async Task CreateGroup(GroupResponse response)
        {
            GitLabGroup group = new();
            group.GitlabTeamId = response.id;
            group.GroupName = response.name;

            await _context.Group.AddAsync(group);
        }

        public async Task GetMembers(List<GroupResponse> groups)
        {
            string baseUrlPart1 = "https://gitlab.com/api/v4/groups/";
            string baseUrlPart2 = @"/members";
            foreach(var g in groups)
            {
                //List<UserResponse> users = new();
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "tr3SszQyyabFd34jNbjU");
                    var response = await client.GetAsync(baseUrlPart1 + g.id.ToString() + baseUrlPart2);

                    var jsonResult = await response.Content.ReadAsStringAsync();

                    //List<UserResponse> result = JsonSerializer.Deserialize<List<UserResponse>>(jsonResult);
                    //users.AddRange(result);
                }
                List<User> nonexisting = new();
                GitLabGroup group = await _context.Group.Where(i => i.GitlabTeamId == g.id).Include("Users").FirstOrDefaultAsync();
                //foreach (var u in users)
                //{
                //    User user = await _context.Users.Where(id => id.GitLabId == u.id).Include("Groups").FirstOrDefaultAsync();

                //    if (user == null)
                //        user = CreateUser(u);

                //    if (!UserExists(group, user))
                //    {
                //        user.Groups.Add(group);
                //        group.Users.Add(user);
                //    }                       
                //}
            }
            await _context.SaveChangesAsync();
        }

        public bool UserExists(GitLabGroup group, User user)
        {
            bool result = false;
            if(group.Users != null)
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
