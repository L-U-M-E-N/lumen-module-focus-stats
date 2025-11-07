using Lumen.Modules.FocusStats.Business.Exceptions;
using Lumen.Modules.FocusStats.Business.Interfaces;
using Lumen.Modules.FocusStats.Common.Dto;
using Lumen.Modules.FocusStats.Common.Rules;

using Microsoft.AspNetCore.Mvc;

namespace Lumen.Modules.FocusStats.Module.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class FocusStatsController(ILogger<FocusStatsController> logger, IActivitiesService activitiesService, ICleaningRulesService cleaningRulesService, ITaggingRulesService taggingRulesService) : ControllerBase {

        // Activities-related routes
        [HttpPost("activities")]
        public async Task<IActionResult> SubmitActivities([FromBody] IEnumerable<NewUserActivityDto> activities, CancellationToken cancellationToken = default) {
            try {
                await activitiesService.AddNewActivitiesAsync(activities, cancellationToken);
                return Ok();
            } catch (Exception ex) {
                logger.LogError(ex, "Unexpected error when adding activities");
                return StatusCode(StatusCodes.Status500InternalServerError, "Unexpected error");
            }
        }

        [HttpPost("compress")]
        public async Task<IActionResult> CompressActivities(CancellationToken cancellationToken = default) {
            try {
                await activitiesService.CompressActivitiesAsync(cancellationToken);
                return Ok();
            } catch (Exception ex) {
                logger.LogError(ex, "Unexpected error when compressing activities");
                return StatusCode(StatusCodes.Status500InternalServerError, "Unexpected error");
            }
        }

        // Tagging-rules-related routes
        [HttpGet("taggingRules")]
        public async IAsyncEnumerable<TaggingRuleDto> GetTaggingRules() {
            await foreach (var rule in taggingRulesService.GetTaggingRulesAsync()) {
                yield return new TaggingRuleDto {
                    Id = rule.Id,
                    Regex = rule.Regex.ToString(),
                    Tags = rule.Tags,
                    Target = rule.Target,
                    Tests = rule.Tests,
                };
            }
        }

        [HttpPost("taggingRule")]
        public async Task<IActionResult> AddTaggingRule([FromBody] TaggingRuleDto taggingRule, CancellationToken cancellationToken = default) {
            try {
                await taggingRulesService.AddTaggingRuleAsync(new TaggingRule(
                    taggingRule.Regex,
                    taggingRule.Tags.ToList(),
                    taggingRule.Target,
                    taggingRule.Tests
                ), cancellationToken);
                return Ok();
            } catch (BusinessRuleException ex) {
                logger.LogWarning(ex, "Business rule error when adding tagging rule");
                return BadRequest(ex.Message);
            } catch (Exception ex) {
                logger.LogError(ex, "Unexpected error when adding tagging rule");
                return StatusCode(StatusCodes.Status500InternalServerError, "Unexpected error");
            }
        }

        [HttpDelete("taggingRule/{id:guid}")]
        public async Task<IActionResult> DeleteTaggingRule([FromRoute] Guid id, CancellationToken cancellationToken) {
            try {
                await taggingRulesService.RemoveTaggingRuleAsync(id, cancellationToken);
                return Ok();
            } catch (BusinessRuleException ex) {
                logger.LogWarning(ex, "Business rule error when removing tagging rule");
                return BadRequest(ex.Message);
            } catch (Exception ex) {
                logger.LogError(ex, "Unexpected error when removing tagging rule");
                return StatusCode(StatusCodes.Status500InternalServerError, "Unexpected error");
            }
        }

        // Cleaning-rules-related routes
        [HttpGet("cleaningRules")]
        public async IAsyncEnumerable<CleaningRuleDto> GetCleaningRules() {
            await foreach (var rule in cleaningRulesService.GetCleaningRulesAsync()) {
                yield return new CleaningRuleDto {
                    Id = rule.Id,
                    Regex = rule.Regex.ToString(),
                    Replacement = rule.Replacement,
                    Target = rule.Target,
                    Tests = rule.Tests,
                };
            }
        }

        [HttpPost("cleaningRule")]
        public async Task<IActionResult> AddCleaningRule([FromBody] CleaningRuleDto cleaningRule, CancellationToken cancellationToken = default) {
            try {
                await cleaningRulesService.AddCleaningRuleAsync(new CleaningRule(
                    cleaningRule.Regex,
                    cleaningRule.Replacement,
                    cleaningRule.Target,
                    cleaningRule.Tests
                ), cancellationToken);
                return Ok();
            } catch (BusinessRuleException ex) {
                logger.LogWarning(ex, "Business rule error when adding cleaning rule");
                return BadRequest(ex.Message);
            } catch (Exception ex) {
                logger.LogError(ex, "Unexpected error when adding cleaning rule");
                return StatusCode(StatusCodes.Status500InternalServerError, "Unexpected error");
            }
        }

        [HttpDelete("cleaningRule/{id:guid}")]
        public async Task<IActionResult> DeleteCleaningRule([FromRoute] Guid id, CancellationToken cancellationToken) {
            try {
                await cleaningRulesService.RemoveCleaningRuleAsync(id, cancellationToken);
                return Ok();
            } catch (BusinessRuleException ex) {
                logger.LogWarning(ex, "Business rule error when removing cleaning rule");
                return BadRequest(ex.Message);
            } catch (Exception ex) {
                logger.LogError(ex, "Unexpected error when removing cleaning rule");
                return StatusCode(StatusCodes.Status500InternalServerError, "Unexpected error");
            }
        }
    }
}
