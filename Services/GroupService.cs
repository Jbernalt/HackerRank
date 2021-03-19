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
        public Task AddMembersToGroup(GroupResponse group);
        public Task CreateGroup(GroupResponse response);
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

                await AddMembersToGroup(g);
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

        public async Task AddMembersToGroup(GroupResponse group)
        {
            string baseUrlPart1 = "https://gitlab.com/api/v4/groups/";
            string baseUrlPart2 = @"/members";
                List<UserResponse> result = new();
            Group theGroup = await _context.Group.Where(g => g.GitlabTeamId == group.id).FirstOrDefaultAsync();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config["Authentication:GitLab:APIKey"]);
                var response = await client.GetAsync(baseUrlPart1 + group.id.ToString() + baseUrlPart2);

                var jsonResult = await response.Content.ReadAsStringAsync();

                result = JsonSerializer.Deserialize<List<UserResponse>>(jsonResult);
                
            }
            List<User> nonexisting = new();
            foreach (var u in result)
            {
                User user = (User)await _context.Users.Where(x => x.UserName == u.username).FirstOrDefaultAsync();
                if (UserExists(theGroup, user))
                {
                    user.Groups.Add(theGroup);
                    theGroup.Users.Add(user);
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
            if (user != null)
            {
                foreach (var u in group.Users)
                {
                    if (u == user)
                    {
                        result = false;
                        break;
                    }
                }
                result = true;
            }
            
            return result;
        }


    }
}

