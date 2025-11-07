using Lumen.Modules.FocusStats.Common.Dto;
using Lumen.Modules.FocusStats.Common.Rules;

using System.Text.RegularExpressions;

namespace Lumen.Modules.FocusStats.Business.Interfaces {
    public interface IActivitiesService {
        Task AddNewActivitiesAsync(IEnumerable<NewUserActivityDto> activities, CancellationToken cancellationToken);
        Task MassivelyRetagByRegexAsync(Regex regexToMatch, List<TaggingRule> rules, CancellationToken cancellationToken);
        Task MassivelyApplyNewCleaningRuleAsync(CleaningRule cleaningRule, CancellationToken cancellationToken);
        Task CompressActivitiesAsync(CancellationToken cancellationToken);
    }
}
