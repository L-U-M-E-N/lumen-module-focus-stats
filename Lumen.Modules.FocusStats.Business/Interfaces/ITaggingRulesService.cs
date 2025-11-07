using Lumen.Modules.FocusStats.Common.Rules;

namespace Lumen.Modules.FocusStats.Business.Interfaces {
    public interface ITaggingRulesService {
        public IAsyncEnumerable<TaggingRule> GetTaggingRulesAsync();
        public Task AddTaggingRuleAsync(TaggingRule taggingRule, CancellationToken cancellationToken);
        public Task RemoveTaggingRuleAsync(Guid id, CancellationToken cancellationToken);
    }
}
