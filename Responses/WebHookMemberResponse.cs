using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackerRank.Responses
{
    public class WebHookMemberResponse
    {
        public string group_name { get; set; }
        public string group_path { get; set; }
        public int group_id { get; set; }
        public string user_username { get; set; }
        public string user_name { get; set; }
        public string user_email { get; set; }
        public int user_id { get; set; }
        public string group_access { get; set; }
        public object group_plan { get; set; }
        public string event_name { get; set; }
    }
}
