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
using HackerRank.ViewModels;
using AutoMapper;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace HackerRank.Services
{
    public interface IGroupService
    {
        public Task GetGroupData();
        public Task<bool> CreateGroup(Group group);
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

        public async Task<List<GroupResponse>> GetAllGroups()
        {
            List<GroupResponse> groupResponses = new();
            GroupViewModel groupView = new();
            UriBuilder uriBuilder = new()
            {
                Scheme = "https",
                Host = "gitlab.com/api/v4/groups"
            };

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config["Authentication:GitLab:APIKey"]);
                var response = await client.GetAsync(uriBuilder.ToString());
                var jsonResult = await response.Content.ReadAsStringAsync();

                List<GroupResponse> result = JsonSerializer.Deserialize<List<GroupResponse>>(jsonResult);
                groupResponses.AddRange(result);
                

            }
            return groupResponses;
        }

        public async Task<List<User>> GetUsersInGroup(Group group)
        {
            List<User> userlist = new();
            UriBuilder uriBuilder = new()
            {
                Scheme = "https",
                Host = "gitlab.com/api/v4/groups"
            };

            List<UserResponse> result = new();

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config["Authentication:GitLab:APIKey"]);
                string path = group.GitlabTeamId.ToString() + $"/members";
                uriBuilder.Path = path;

                var response = await client.GetAsync(uriBuilder.ToString());

                var jsonResult = await response.Content.ReadAsStringAsync();

                result = JsonSerializer.Deserialize<List<UserResponse>>(jsonResult);
            }

            foreach (var u in result)
            {
                var user = await _context.Users.Where(x => x.GitLabId == u.id).FirstOrDefaultAsync();
                if (user != null)
                    userlist.Add(user);
            }

            return userlist;
        }

        public async Task GetGroupData()
        {
            var groupListTotal = await GetAllGroups();
            List<Group> groupListNotExist = new();
            List<Group> groupListExit = new();

            foreach (var g in groupListTotal)
            {
                var group = await _context.Group.Include("Users").Where(x => x.GitlabTeamId == g.id).FirstOrDefaultAsync();
                if (group == null)
                {
                    Group newGroup = new() { GitlabTeamId = g.id, GroupName = g.name };
                    groupListNotExist.Add(newGroup);
                }
                else
                {
                    groupListExit.Add(group);
                }
            }

            if (groupListExit.Count > 0)
            {
                foreach (Group group in groupListExit)
                {
                    var userList = await GetUsersInGroup(group);
                    foreach (var user in userList)
                    {
                        if (!UserExistsInGroup(group, user))
                        {
                            group.Users.Add(user);
                        }
                    }
                }
            }

            if (groupListNotExist.Count > 0)
            {
                foreach (Group group in groupListNotExist)
                {
                    var userList = await GetUsersInGroup(group);

                    if (userList.Count > 0)
                    {
                        if (await CreateGroup(group))
                        {
                            group.Users.AddRange(userList);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
            }
        }

        public async Task<bool> CreateGroup(Group group)
        {
            try
            {
                await _context.Group.AddAsync(group);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
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

        public bool UserExistsInGroup(Group group, User user)
        {
            bool result = false;

            foreach(var u in group.Users)
            {
                if(u == user)
                {
                    result = true;
                    return result;
                }
            }
            return result;
        }
    }
}
