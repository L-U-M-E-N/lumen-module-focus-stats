using Lumen.Modules.FocusStats.Common;

using Microsoft.AspNetCore.Mvc;

namespace Lumen.Modules.FocusStats.Module.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class FocusStatsController(ILogger<FocusStatsController> logger) : ControllerBase {
        [HttpPost("activities")]
        public string SubmitActivities([FromBody] IEnumerable<UserFocusedActivity> activities) {
            throw new NotImplementedException();
        }
    }
}
