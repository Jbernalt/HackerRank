using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HackerRank.Models.GitLabGroups;
using Microsoft.AspNetCore.Identity;

namespace HackerRank.Models.Users
{
    public class User : IdentityUser
    {
        public User()
        {
            Groups = new List<GitLabGroup>();
        }
        public int GitLabId { get; set; }
        public double MonthlyRating { get; set; }
        public DateTime DateCreated { get; set; }
        public byte[] ImageBinaryData { get; set; }
        public List<GitLabGroup> Groups { get; set; }

    }
}
