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
        public int GitLabId { get; set; }
        public double MonthlyRating { get; set; }
        public List<Group> Groups { get; set; }

    }
}
