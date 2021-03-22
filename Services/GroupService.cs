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
using AutoMapper;

namespace HackerRank.Services
{
    public interface IGroupService
    {
        public Task<List<GroupResponse>> GetData();
        public Task AddMembersToGroup(List<GroupResponse> group);
        public Task<Group> CreateGroup(GroupResponse response);
        public Task SummarizeGroup(List<Group> group);

    }

    public class GroupService : IGroupService
    {
        
        HackerRankContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        public GroupService(HackerRankContext context, IConfiguration config, IMapper mapper)
        {
            _context = context;
            _config = config;
            _mapper = mapper;
        }

        public async Task<List<GroupResponse>> GetData()
        {
            string baseUrl = "https://gitlab.com/api/v4/groups";
            List<GroupResponse> groupResponses = new List<GroupResponse>();
            List<Group> groups = new();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config["Authentication:GitLab:APIKey"]);
                var response = await client.GetAsync(baseUrl);

                var jsonResult = await response.Content.ReadAsStringAsync();

                List<GroupResponse> result = JsonSerializer.Deserialize<List<GroupResponse>>(jsonResult);
                groupResponses.AddRange(result);
            }

                await AddMembersToGroup(groupResponses);
           
            return groupResponses;
        }

        public async Task<Group> CreateGroup(GroupResponse response)
        {
            Group group = new();
            group.GitlabTeamId = response.id;
            group.GroupName = response.name;

            await _context.Group.AddAsync(group);
            await _context.SaveChangesAsync();
            return group;
        }

        public async Task AddMembersToGroup(List<GroupResponse> groups)
        {
            string baseUrlPart1 = "https://gitlab.com/api/v4/groups/";
            string baseUrlPart2 = @"/members";
            List<UserResponse> result = new();

            foreach(var group in groups)
            {
                Group theGroup = await _context.Group.Where(g => g.GitlabTeamId == group.id).Include("Users").FirstOrDefaultAsync();
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config["Authentication:GitLab:APIKey"]);
                    var response = await client.GetAsync(baseUrlPart1 + group.id.ToString() + baseUrlPart2);
                    var jsonResult = await response.Content.ReadAsStringAsync();
                    result = JsonSerializer.Deserialize<List<UserResponse>>(jsonResult);
                }

                if (theGroup == null)
                    await CreateGroup(group);

                foreach (var u in result)
                {
                    User user = (User)await _context.Users.Where(x => x.UserName == u.username).FirstOrDefaultAsync();
                    if (!UserExistsInGroup(theGroup, user) && user != null)
                    {
                        user.Groups.Add(theGroup);
                    }
                }
            }
            var removeGroups = await _context.Group.Where(g => g.Users.Count <= 0).ToListAsync();
            await _context.SaveChangesAsync();
            _context.Group.RemoveRange(removeGroups);
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

        public bool UserExistsInGroup(Group group, User user)
        {
            bool result = false;
            if (user == null || group == null)
                return result;

            foreach(var u in group.Users)
            {
                if(u == user)
                {
                    result = true;
                    return result;
                };
            }
            return result;
        }
    }
}