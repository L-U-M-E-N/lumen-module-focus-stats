using Lumen.Modules.FocusStats.Business.Exceptions;
using Lumen.Modules.FocusStats.Business.Interfaces;
using Lumen.Modules.FocusStats.Common.Rules;
using Lumen.Modules.FocusStats.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lumen.Modules.FocusStats.Business.Implementations {
    public class TaggingRulesService(FocusStatsContext context, ILogger<TaggingRulesService> logger, IActivitiesService activitiesService) : ITaggingRulesService {
        public async Task AddTaggingRuleAsync(TaggingRule taggingRule, CancellationToken cancellationToken) {
            if (context.TaggingRules.Any((t) => t.Id == taggingRule.Id || t.Regex == taggingRule.Regex)) {
                throw new BusinessRuleException("Invalid tagging rule, Id or RegExp already used");
            }

            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            try {
                await context.TaggingRules.AddAsync(taggingRule, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

                await activitiesService.MassivelyRetagByRegexAsync(taggingRule.Regex, await context.TaggingRules.ToListAsync(cancellationToken), cancellationToken);

                await transaction.CommitAsync(cancellationToken);
            } catch (Exception ex) {
                logger.LogError(ex, "Error when adding new taggingrule");
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public IAsyncEnumerable<TaggingRule> GetTaggingRulesAsync() {
            return context.TaggingRules.AsAsyncEnumerable();
        }

        public async Task RemoveTaggingRuleAsync(Guid id, CancellationToken cancellationToken) {
            var entry = await context.TaggingRules.FirstOrDefaultAsync((t) => t.Id == id, cancellationToken);
            if (entry is null) {
                return;
            }

            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            try {
                context.TaggingRules.Remove(entry);
                await context.SaveChangesAsync(cancellationToken);

                await activitiesService.MassivelyRetagByRegexAsync(entry.Regex, await context.TaggingRules.ToListAsync(cancellationToken), cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            } catch (Exception ex) {
                logger.LogError(ex, "Error when adding new taggingrule");
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
