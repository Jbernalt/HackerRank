using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackerRank.Responses
{
    public class EventResponse
    {
        public int id { get; set; }
        public int project_id { get; set; }
        public string action_name { get; set; }
        public string target_type { get; set; }
        public int author_id { get; set; }
        public string author_username { get; set; }
    }
}
