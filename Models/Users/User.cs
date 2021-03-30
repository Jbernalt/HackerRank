using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HackerRank.Models.Groups;
using Microsoft.AspNetCore.Identity;

namespace HackerRank.Models.Users
{
    public class User : IdentityUser
    {
        public User()
        {
            Groups = new List<Group>();
            UserStats = new UserStats();
        }
        public int GitLabId { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public List<Group> Groups { get; set; }
        public UserStats UserStats { get; set; }
    }
}
