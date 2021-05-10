using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackerRank.Responses
{
    public class WebHookSubGroupResponse
    {
        public string event_name { get; set; }
        public string name { get; set; }
        public int group_id { get; set; }
        public int parent_group_id { get; set; }
    }
}
