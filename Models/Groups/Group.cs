using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HackerRank.Models.Users;
using System.ComponentModel.DataAnnotations;
using HackerRank.Models.Projects;

namespace HackerRank.Models.Groups
{
    public class Group 
    {
        public Group()
        {
            Users = new List<User>();
            GroupStats = new GroupStats();
            Projects = new List<Project>();
        }
        [Key]
        public int GroupID { get; set; }
        public int GitlabTeamId { get; set; }
        public string GroupName { get; set; }
        public GroupStats GroupStats { get; set; }
        public List<Project> Projects { get; set; }
        public List<User> Users { get; set; }
    }
}
