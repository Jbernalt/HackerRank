using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using HackerRank.Responses;
using HackerRank.ViewModels;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace HackerRank.Controllers
{
    [ApiController]
    [Route("api/webhook/")]
    public class WebHookController : Controller
    {
        private readonly ILogger<WebHookController> _logger;
        private readonly IConfiguration _config;

        public WebHookController(ILogger<WebHookController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        [IgnoreAntiforgeryToken]
        [HttpPost]
        [Route("receive")]
        public async Task<PartialViewResult> Receive()
        {
            Request.Headers.TryGetValue("X-Gitlab-Event", out StringValues gitLabEvent);
            Request.Headers.TryGetValue("X-Gitlab-Token", out StringValues gitLabSignature);

            if (gitLabSignature.FirstOrDefault() == _config["Authentication:GitLab:WebHookAuthentication"])
            {
                List<TopFiveViewModel> viewModel = new();
                TopFiveViewModel model = new();
                using var reader = new StreamReader(Request.Body);
                var json = await reader.ReadToEndAsync();

                if (gitLabEvent == "Push Hook")
                    model.WebHookResponse.WebHookCommitResponse = JsonSerializer.Deserialize<WebHookCommitResponse>(json);
                else if (gitLabEvent == "Issue Hook")
                    model.WebHookResponse.WebHookIssueResponse = JsonSerializer.Deserialize<WebHookIssueResponse>(json);
                else if (gitLabEvent == "Merge Request Hook")
                    model.WebHookResponse.WebHookMergeResponse = JsonSerializer.Deserialize<WebHookMergeResponse>(json);
                else if (gitLabEvent == "Note Hook")
                    model.WebHookResponse.WebHookCommentResponse = JsonSerializer.Deserialize<WebHookCommentResponse>(json);

                viewModel.Add(model);
                return PartialView("_PartialLiveWebHookEvent", viewModel);
            }

            return null;
        }
    }
}
