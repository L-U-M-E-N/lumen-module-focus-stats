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

            int cursor = 0;
            int pageSize = 100_000;
            while (true) {
                var activities = await context.Activities.OrderBy(x => x.Device).ThenBy(x => x.StartTime).Skip(cursor).Take(pageSize).ToListAsync(cancellationToken);
                if (activities.Count == 0) {
                    break;
                }

                List<UserFocusedActivity> toDelete = [];
                for (int j = 0; j < activities.Count - 1; j++) {
                    if (cancellationToken.IsCancellationRequested) {
                        return;
                    }

                    var newStart = activities[j].StartTime.AddSeconds(activities[j].SecondsDuration);

                    if (
                        activities[j].Device == activities[j + 1].Device &&
                        activities[j].Name == activities[j + 1].Name &&
                        activities[j].AppOrExe == activities[j + 1].AppOrExe &&
                        newStart.CompareTo(activities[j].StartTime) == 0
                    ) {

                        activities[j].SecondsDuration += activities[j + 1].SecondsDuration;
                        toDelete.Add(activities[j + 1]);
                        activities.RemoveAt(j + 1);

                        j--;
                    }
                }

                context.RemoveRange(toDelete);
                await context.SaveChangesAsync(cancellationToken);
                context.ChangeTracker.Clear();

                cursor += pageSize;
            }

            logger.LogInformation("{Date} - Done with compressing activities!", DateTime.UtcNow);
        }

        public async Task MassivelyApplyNewCleaningRuleAsync(CleaningRule cleaningRule, List<TaggingRule> taggingRules, CancellationToken cancellationToken) {
            logger.LogInformation("{Date} - Massively applying new cleaning rule ...", DateTime.UtcNow);

            await foreach (var activity in context.Activities.AsAsyncEnumerable()) {
                if (cancellationToken.IsCancellationRequested) {
                    return;
                }

                if (cleaningRule.Regex.IsMatch(activity.Name) || cleaningRule.Regex.IsMatch(activity.AppOrExe)) {
                    activity.ApplyNewCleaningRule(cleaningRule);
                    activity.ApplyTaggingRules(taggingRules);
                } else {
                    context.Entry(activity).State = EntityState.Detached;
                }
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
                } else {
                    context.Entry(activity).State = EntityState.Detached;
                }
            }

            logger.LogInformation("{Date} - Done with retagging activities! Saving to database ...", DateTime.UtcNow);

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("{Date} - Done with retagging activities! Saving to database ... Done!", DateTime.UtcNow);
        }
    }
}
