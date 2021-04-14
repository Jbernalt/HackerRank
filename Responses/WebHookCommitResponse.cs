using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackerRank.Responses
{
    public class WebHookCommitResponse
    {
        public string object_kind { get; set; }
        public int user_id { get; set; }
        public string user_name { get; set; }
        public string user_username { get; set; }
        public int project_id { get; set; }
        public ProjectResponse project { get; set; }
    }


}
