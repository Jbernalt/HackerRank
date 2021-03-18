using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackerRank.Responses
{
    public class UserResponse
    {
        public int GitLabId { get; set; }
        public string UserName { get; set; }
        public double MonthlyRating { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
