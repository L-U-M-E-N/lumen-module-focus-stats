using Lumen.Modules.FocusStats.Business.Exceptions;
using Lumen.Modules.FocusStats.Business.Interfaces;
using Lumen.Modules.FocusStats.Common.Rules;
using Lumen.Modules.FocusStats.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lumen.Modules.FocusStats.Business.Implementations {
    public class CleaningRulesService(FocusStatsContext context, ILogger<CleaningRulesService> logger, IActivitiesService activitiesService) : ICleaningRulesService {
        public async Task AddCleaningRuleAsync(CleaningRule cleaningRule, CancellationToken cancellationToken) {
            if (context.CleaningRules.Any((t) => t.Id == cleaningRule.Id || t.Regex == cleaningRule.Regex)) {
                throw new BusinessRuleException("Invalid cleaning rule, Id or RegExp already used");
            }

            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            try {
                await context.CleaningRules.AddAsync(cleaningRule, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

                await activitiesService.MassivelyApplyNewCleaningRuleAsync(cleaningRule, cancellationToken);

                await transaction.CommitAsync(cancellationToken);
            } catch (Exception ex) {
                logger.LogError(ex, "Error when adding new cleaningrule");
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public IAsyncEnumerable<CleaningRule> GetCleaningRulesAsync() {
            return context.CleaningRules.AsAsyncEnumerable();
        }

        public async Task RemoveCleaningRuleAsync(Guid id, CancellationToken cancellationToken) {
            var entry = await context.CleaningRules.FirstOrDefaultAsync((t) => t.Id == id, cancellationToken);
            if (entry is null) {
                return;
            }

            context.CleaningRules.Remove(entry);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
