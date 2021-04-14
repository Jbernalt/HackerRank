using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackerRank.Responses
{
    public class WebHookResponse
    {
        public WebHookCommentResponse WebHookCommentResponse { get; set; }
        public WebHookCommitResponse WebHookCommitResponse { get; set; }
        public WebHookIssueResponse WebHookIssueResponse { get; set; }
        public WebHookMergeResponse WebHookMergeResponse { get; set; }
    }
}
