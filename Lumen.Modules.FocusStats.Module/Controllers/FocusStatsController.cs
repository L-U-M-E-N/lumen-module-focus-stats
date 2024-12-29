using Lumen.Modules.FocusStats.Common;

using Microsoft.AspNetCore.Mvc;

namespace Lumen.Modules.FocusStats.Module.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class FocusStatsController : ControllerBase {
        private readonly ILogger<FocusStatsController> _logger;

        public FocusStatsController(ILogger<FocusStatsController> logger) {
            _logger = logger;
        }

        [HttpPost("activities")]
        public string SubmitActivities([FromBody] IEnumerable<UserFocusedActivity> activities) {
            throw new NotImplementedException();
        }
    }
}
