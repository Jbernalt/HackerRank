using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackerRank.Responses
{
    public class WebHookCommentResponse
    {
        public string object_kind { get; set; }
        public User user { get; set; }
        public int project_id { get; set; }
        public Object_Attributes object_attributes { get; set; }
        public ProjectResponse project { get; set; }


        public class User
        {
            public int id { get; set; }
            public string name { get; set; }
            public string username { get; set; }
        }

        public class Object_Attributes
        {
            public string note { get; set; }
            public string created_at { get; set; }
        }
    }
}
