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
using System.Diagnostics;
using HackerRank.Models.Projects;
using HackerRank.ViewModels;
using HackerRank.Models.Transactions;

namespace HackerRank.Services
{
    public interface IGroupService
    {
        public Task<bool> CreateGroup(WebHookSubGroupResponse response);
        public Task<bool> RemoveGroup(int id);
        public Task<List<Project>> GetProjectsForGroup(int groupId);
        public Task<List<User>> GetUsersForGroup(int groupId);
        public Task<bool> RemoveUserFromGroup(WebHookMemberResponse response);
        public Task<bool> AddUserToGroup(WebHookMemberResponse response);
        public Task GetAllGroups();
        public Task<bool> GetProjectsGroups(int id, string projectname);
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

        public async Task<bool> AddUserToGroup(WebHookMemberResponse response)
        {
            var user = await _context.Users.Where(i => i.GitLabId == response.user_id).FirstOrDefaultAsync();
            var group = await _context.Group.Where(i => i.GitlabTeamId == response.group_id).FirstOrDefaultAsync();

            if (user != null && group != null)
            {
                try
                {
                    group.Users.Add(user);
                    await _context.SaveChangesAsync();
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            return false;
        }

        public async Task<bool> RemoveUserFromGroup(WebHookMemberResponse response)
        {
            var user = await _context.Users.Where(i => i.GitLabId == response.user_id).FirstOrDefaultAsync();
            var group = await _context.Group.Where(i => i.GitlabTeamId == response.group_id).Include(u => u.Users).FirstOrDefaultAsync();

            if (user != null && group != null)
            {
                try
                {
                    group.Users.Remove(user);
                    await _context.SaveChangesAsync();
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            return false;
        }

        public async Task GetAllGroups()
        {
            List<GroupResponse> groupResponses = new();
            UriBuilder uriBuilder = new()
            {
                Scheme = "https",
                Host = "gitlab.com/api/v4",
                Path = "groups",
                Query = "per_page=100"
            };

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config["Authentication-GitLab-APIKey"]);
                var response = await client.GetAsync(uriBuilder.ToString());
                var totalPages = int.Parse(response.Headers.GetValues("X-Total-Pages").First());
                var jsonResult = await response.Content.ReadAsStringAsync();

                List<GroupResponse> result = JsonSerializer.Deserialize<List<GroupResponse>>(jsonResult);
                groupResponses.AddRange(result);
                if (totalPages > 1)
                {
                    for (int i = 2; i <= totalPages; i++)
                    {
                        response = await client.GetAsync(uriBuilder.ToString() + "&page=" + i.ToString());
                        jsonResult = await response.Content.ReadAsStringAsync();
                        groupResponses.AddRange(JsonSerializer.Deserialize<List<GroupResponse>>(jsonResult));
                    }
                }
            }

            foreach (var group in groupResponses)
            {
                var exists = await _context.Group.Where(i => i.GitlabTeamId == group.id).FirstOrDefaultAsync();
                if (exists == null)
                {
                    var projects = await GetProjectsForGroup(group.id);
                    Group newGroup = new()
                    {
                        GitlabTeamId = group.id,
                        GroupName = group.name,
                        GroupStats = new(),
                        Projects = projects,
                        Users = new()
                    };
                    _context.Group.Add(newGroup);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    exists.Projects = await GetProjectsForGroup(exists.GitlabTeamId);
                    await _context.SaveChangesAsync();
                }
            }
        }

        //public async Task<List<User>> GetUsersInGroup(Group group)
        //{
        //    List<User> userlist = new();
        //    UriBuilder uriBuilder = new()
        //    {
        //        Scheme = "https",
        //        Host = "gitlab.com/api/v4/groups"
        //    };

        //    List<UserResponse> result = new();

        //    using (var client = new HttpClient())
        //    {
        //        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config["Authentication-GitLab-APIKey"]);
        //        string path = group.GitlabTeamId.ToString() + $"/members";
        //        uriBuilder.Path = path;

        //        var response = await client.GetAsync(uriBuilder.ToString());

        //        var jsonResult = await response.Content.ReadAsStringAsync();

        //        result = JsonSerializer.Deserialize<List<UserResponse>>(jsonResult);
        //    }

        //    foreach (var u in result)
        //    {
        //        var user = await _context.Users.Where(x => x.GitLabId == u.id).FirstOrDefaultAsync();
        //        if (user != null)
        //            userlist.Add(user);
        //    }

        //    return userlist;
        //}

        //public async Task GetGroupData()
        //{
        //    var groupListTotal = await GetAllGroups();
        //    List<Group> groupListNotExist = new();
        //    List<Group> groupListExit = new();

        //    foreach (var g in groupListTotal)
        //    {
        //        var group = await _context.Group.Include("Users").Where(x => x.GitlabTeamId == g.id).FirstOrDefaultAsync();
        //        if (group == null)
        //        {
        //            Group newGroup = new() { GitlabTeamId = g.id, GroupName = g.name };
        //            groupListNotExist.Add(newGroup);
        //        }
        //        else
        //        {
        //            groupListExit.Add(group);
        //        }
        //    }

        //    if (groupListExit.Count > 0)
        //    {
        //        foreach (Group group in groupListExit)
        //        {
        //            var userList = await GetUsersInGroup(group);
        //            foreach (var user in userList)
        //            {
        //                if (!UserExistsInGroup(group, user))
        //                {
        //                    group.Users.Add(user);
        //                }
        //            }
        //        }
        //    }

        //    if (groupListNotExist.Count > 0)
        //    {
        //        foreach (Group group in groupListNotExist)
        //        {
        //            var userList = await GetUsersInGroup(group);

        //            if (userList.Count > 0)
        //            {
        //                if (await CreateGroup(group))
        //                {
        //                    group.Users.AddRange(userList);
        //                    await _context.SaveChangesAsync();
        //                }
        //            }
        //        }
        //    }
        //}

        public async Task<List<Project>> GetProjectsForGroup(int id)
        {
            UriBuilder uriBuilder = new()
            {
                Scheme = "https",
                Host = $"gitlab.com/api/v4/groups/{id}",
                Path = "projects",
                Query = "?per_page=100"
            };

            List<ProjectResponse> projectResponses = new();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config["Authentication-GitLab-APIKey"]);
                var response = await client.GetAsync(uriBuilder.ToString());
                var totalPages = int.Parse(response.Headers.GetValues("X-Total-Pages").First());
                var jsonResult = await response.Content.ReadAsStringAsync();

                projectResponses.AddRange(JsonSerializer.Deserialize<List<ProjectResponse>>(jsonResult));

                if (totalPages > 1)
                {
                    for (int i = 2; i <= totalPages; i++)
                    {
                        response = await client.GetAsync(uriBuilder.ToString() + "&page=" + i.ToString());
                        jsonResult = await response.Content.ReadAsStringAsync();

                        projectResponses.AddRange(JsonSerializer.Deserialize<List<ProjectResponse>>(jsonResult));
                    }
                }
            }

            List<Project> projects = new();
            foreach (var response in projectResponses)
            {
                var p = await _context.Project.Where(p => p.GitLabId == response.id).FirstOrDefaultAsync();
                if (p == null)
                {
                    Project project = new()
                    {
                        GitLabId = response.id,
                        ProjectName = response.name
                    };
                    projects.Add(project);
                }
                else
                {
                    projects.Add(p);
                }
            }
            return projects;
        }

        public async Task<List<User>> GetUsersForGroup(int id)
        {
            UriBuilder uriBuilder = new()
            {
                Scheme = "https",
                Host = "gitlab.com/api/v4/groups",
                Query = "?per_page=100"
            };

            List<UserResponse> projectResponses = new();
            using (var client = new HttpClient())
            {
                string path = id.ToString() + $"/members";
                uriBuilder.Path = path;

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config["Authentication-GitLab-APIKey"]);
                var response = await client.GetAsync(uriBuilder.ToString());
                var totalPages = int.Parse(response.Headers.GetValues("X-Total-Pages").First());

                var jsonResult = await response.Content.ReadAsStringAsync();
                projectResponses = JsonSerializer.Deserialize<List<UserResponse>>(jsonResult);
            }

            List<User> users = new();
            foreach (var response in projectResponses)
            {
                var p = await _context.Users.Where(p => p.GitLabId == response.id).FirstOrDefaultAsync();
                if (p != null)
                {
                    users.Add(p);
                }
            }
            return users;
        }

        public async Task<bool> RemoveGroup(int id)
        {
            var group = await _context.Group
                .Where(x => x.GitlabTeamId == id)
                .Include(p => p.Projects)
                .Include(u => u.Users).FirstOrDefaultAsync();
            if (group == null)
                return false;

            try
            {
                _context.Project.RemoveRange(group.Projects);
                _context.Group.Remove(group);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
        
        public async Task<bool> CreateGroup(WebHookSubGroupResponse response)
        {
            Group group = new()
            {
                GitlabTeamId = response.group_id,
                GroupName = response.name,
                GroupStats = new(),
                Projects = await GetProjectsForGroup(response.group_id),
                Users = await GetUsersForGroup(response.group_id)
            };

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

        public async Task<bool> GetProjectsGroups(int id, string projectname)
        {
            if (_context.Project.Where(x => x.GitLabId == id).FirstOrDefault() != null)
                return false;

            UriBuilder uriBuilder = new()
            {
                Scheme = "https",
                Host = $"gitlab.com/api/v4/projects/{id}/groups"
            };

            List<GroupResponse> groupResponse = new();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config["Authentication-GitLab-APIKey"]);
                var response = await client.GetAsync(uriBuilder.ToString());
                var totalPages = int.Parse(response.Headers.GetValues("X-Total-Pages").First());
                var jsonResult = await response.Content.ReadAsStringAsync();

                groupResponse.AddRange(JsonSerializer.Deserialize<List<GroupResponse>>(jsonResult));

                if (totalPages > 1)
                {
                    for (int i = 2; i <= totalPages; i++)
                    {
                        response = await client.GetAsync(uriBuilder.ToString() + "&page=" + i.ToString());
                        jsonResult = await response.Content.ReadAsStringAsync();

                        groupResponse.AddRange(JsonSerializer.Deserialize<List<GroupResponse>>(jsonResult));
                    }
                }
            }

            var project = new Project()
            {
                GitLabId = id,
                ProjectName = projectname
            };

            _context.Project.Add(project);

            foreach (var group in groupResponse)
            {
                var g = await _context.Group.Where(x => x.GitlabTeamId == group.id).Include(p => p.Projects).FirstOrDefaultAsync();
                g.Projects.Add(project);
            }
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
