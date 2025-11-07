using Lumen.Modules.FocusStats.Common.Rules;

namespace Lumen.Modules.FocusStats.Business.Interfaces {
    public interface ICleaningRulesService {
        public IAsyncEnumerable<CleaningRule> GetCleaningRulesAsync();
        public Task AddCleaningRuleAsync(CleaningRule cleaningRule, CancellationToken cancellationToken);
        public Task RemoveCleaningRuleAsync(Guid id, CancellationToken cancellationToken);
    }
}
