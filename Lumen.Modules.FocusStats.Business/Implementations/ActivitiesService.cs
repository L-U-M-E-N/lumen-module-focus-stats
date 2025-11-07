using Lumen.Modules.FocusStats.Business.Interfaces;
using Lumen.Modules.FocusStats.Common;
using Lumen.Modules.FocusStats.Common.Dto;
using Lumen.Modules.FocusStats.Common.Rules;
using Lumen.Modules.FocusStats.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using System.Text.RegularExpressions;

namespace Lumen.Modules.FocusStats.Business.Implementations {
    public class ActivitiesService(FocusStatsContext context, ILogger<ActivitiesService> logger) : IActivitiesService {
        public async Task AddNewActivitiesAsync(IEnumerable<NewUserActivityDto> activities, CancellationToken cancellationToken) {
            var cleaningRules = await context.CleaningRules.ToListAsync(cancellationToken);
            var taggingRules = await context.TaggingRules.ToListAsync(cancellationToken);
            foreach (var activity in activities) {
                if (context.Activities.Any((x) =>
                    x.StartTime == activity.StartTime &&
                    x.SecondsDuration == activity.SecondsDuration &&
                    x.AppOrExe == activity.AppOrExe &&
                    x.Name == activity.Name &&
                    x.Device == activity.Device
                )) {
                    continue; // Duplicate entry
                }

                var newActivity = UserFocusedActivity.CreateActivityAsync(cleaningRules, activity.StartTime, activity.SecondsDuration, activity.AppOrExe, activity.Name, activity.Device);
                newActivity.ApplyTaggingRules(taggingRules);
                context.Activities.Add(newActivity);
            }
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task CompressActivitiesAsync(CancellationToken cancellationToken) {
            logger.LogInformation("{Date} - Starting to compress activities ...", DateTime.UtcNow);
            var activities = await context.Activities.OrderBy(x => x.Device).ThenBy(x => x.StartTime).ToListAsync(cancellationToken);
            List<UserFocusedActivity> toDelete = [];
            for (int i = 0; i < activities.Count - 1; i++) {
                if (cancellationToken.IsCancellationRequested) {
                    return;
                }

                var newStart = activities[i].StartTime.AddSeconds(activities[i].SecondsDuration);

                if (
                    activities[i].Device == activities[i + 1].Device &&
                    activities[i].Name == activities[i + 1].Name &&
                    activities[i].AppOrExe == activities[i + 1].AppOrExe &&
                    newStart.CompareTo(activities[i].StartTime) == 0
                ) {

                    activities[i].SecondsDuration += activities[i + 1].SecondsDuration;
                    toDelete.Add(activities[i + 1]);
                    activities.RemoveAt(i + 1);

                    i--;
                }
            }

            context.RemoveRange(toDelete);

            logger.LogInformation("{Date} - Done with compress activities! Saving to database ...", DateTime.UtcNow);

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("{Date} - Done with compress activities! Saving to database ... Done!", DateTime.UtcNow);
        }

        public async Task MassivelyApplyNewCleaningRuleAsync(CleaningRule cleaningRule, CancellationToken cancellationToken) {
            logger.LogInformation("{Date} - Massively applying new cleaning rule ...", DateTime.UtcNow);

            await foreach (var activity in context.Activities.AsAsyncEnumerable()) {
                if (cancellationToken.IsCancellationRequested) {
                    return;
                }

                activity.ApplyNewCleaningRule(cleaningRule);
            }

            logger.LogInformation("{Date} - Done with applying new cleaning rule! Saving to database ...", DateTime.UtcNow);

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("{Date} - Done with applying new cleaning rule! Saving to database ... Done!", DateTime.UtcNow);
        }

        public async Task MassivelyRetagByRegexAsync(Regex regexToMatch, List<TaggingRule> rules, CancellationToken cancellationToken) {
            logger.LogInformation("{Date} - Massively retagging activities ...", DateTime.UtcNow);

            await foreach (var activity in context.Activities.AsAsyncEnumerable()) {
                if (cancellationToken.IsCancellationRequested) {
                    return;
                }

                if (regexToMatch.IsMatch(activity.Name) || regexToMatch.IsMatch(activity.AppOrExe)) {
                    activity.ApplyTaggingRules(rules);
                }
            }

            logger.LogInformation("{Date} - Done with retagging activities! Saving to database ...", DateTime.UtcNow);

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("{Date} - Done with retagging activities! Saving to database ... Done!", DateTime.UtcNow);
        }
    }
}
