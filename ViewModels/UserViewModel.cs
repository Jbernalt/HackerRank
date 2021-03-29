using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HackerRank.Models.Users;


namespace HackerRank.ViewModels
{
    public class UserViewModel
    {
        //User fields
        public string UserName { get; set; }
        public string Email { get; set; }
        public UserStats UserStats { get; set; }


        //Error management
        public int StatusCode { get; set; }
        public string ReasonPhrase { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }

    }
}
