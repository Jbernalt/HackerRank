using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackerRank.Responses
{
    public class WebHookIssueResponse
    {

        public string object_kind { get; set; }
        public string event_type { get; set; }
        public User user { get; set; }
        public ProjectResponse project { get; set; }
        public Object_Attributes object_attributes { get; set; }


        public class User
        {
            public int id { get; set; }
            public string name { get; set; }
            public string username { get; set; }
        }

        public class ProjectResponse
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        public class Object_Attributes
        {
            public string title { get; set; }
            public int author_id { get; set; }
            public int project_id { get; set; }
            public string description { get; set; }
            public string state { get; set; }
        }
    }
}
