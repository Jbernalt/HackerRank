using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HackerRank.Models.Users;
using System.ComponentModel.DataAnnotations;

namespace HackerRank.Models.Groups
{
    public class Group 
    {
        [Key]
        public int GroupID { get; set; }
        public int GitlabTeamId { get; set; }
        public string GroupName { get; set; }
        public int GroupRating { get; set; }
        public List<User> Users { get; set; }


    }
}
