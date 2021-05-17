using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackerRank.Responses
{
    public class WebHookMergeResponse
    {

        public string object_kind { get; set; }
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
            public string description { get; set; }
        }

        public class Object_Attributes
        {
            public int id { get; set; }
            public string target_branch { get; set; }
            public string source_branch { get; set; }
            public int source_project_id { get; set; }
            public string title { get; set; }
            public string state { get; set; }
            public int target_project_id { get; set; }
            public string action { get; set; }
        }
    }
}
