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
        public Task AddMembersToGroup(GroupResponse groupResponse);
        public Task CreateGroup(GroupResponse response);
        public void SummarizeGroup();
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
            List<GroupResponse> groupResponses = new();

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
            await _context.SaveChangesAsync();
        }

        public async Task AddMembersToGroup(GroupResponse groupResponse)
        {
            string baseUrlPart1 = "https://gitlab.com/api/v4/groups/";
            string baseUrlPart2 = @"/members";
            List<UserResponse> result = new();
            Group group = await _context.Group.Where(g => g.GitlabTeamId == groupResponse.id).Include("Users").FirstOrDefaultAsync();

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config["Authentication:GitLab:APIKey"]);
                var response = await client.GetAsync(baseUrlPart1 + groupResponse.id.ToString() + baseUrlPart2);

                var jsonResult = await response.Content.ReadAsStringAsync();

                result = JsonSerializer.Deserialize<List<UserResponse>>(jsonResult);                
            }

            foreach (var u in result)
            {
                User user = await _context.Users.Where(x => x.GitLabId == u.id).FirstOrDefaultAsync();
                if (UserExists(group, user))
                {
                    group.Users.Add(user);
                }
            }
            
            await _context.SaveChangesAsync();
        }

        public void SummarizeGroup()
        {
            List<Group> groups = _context.Group.Include("Users").ToList();

            foreach (var group in groups)
            {
                double groupRating = 0;
                if (group.Users.Count != 0)
                {
                    foreach (var user in group.Users)
                    {
                        groupRating += user.UserStats.DailyRating;
                    }

                    groupRating /= group.Users.Count;
                    group.GroupRating = groupRating;

                    if (groupRating > 0)
                        _context.SaveChanges();
                }
            }
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
