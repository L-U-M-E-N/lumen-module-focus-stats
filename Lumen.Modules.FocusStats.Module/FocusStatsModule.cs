using Lumen.Modules.FocusStats.Business.Implementations;
using Lumen.Modules.FocusStats.Business.Interfaces;
using Lumen.Modules.FocusStats.Data;
using Lumen.Modules.Sdk;

using Microsoft.EntityFrameworkCore;

namespace Lumen.Modules.FocusStats.Module {
    public class FocusStatsModule(IEnumerable<ConfigEntry> configEntries, ILogger<LumenModuleBase> logger, IServiceProvider provider) : LumenModuleBase(configEntries, logger, provider) {
        public override async Task InitAsync(LumenModuleRunsOnFlag currentEnv) {
            if (currentEnv == LumenModuleRunsOnFlag.Worker) {
                await SubmitDataToAPI();
            }
        }

        public override Task RunAsync(LumenModuleRunsOnFlag currentEnv, DateTime date) {
            return currentEnv switch {
                LumenModuleRunsOnFlag.API => RunCompressAsync(),
                LumenModuleRunsOnFlag.Worker => RunClientWorkerAsync(date),
                _ => throw new NotImplementedException(),
            };
        }

        private async Task RunCompressAsync() {
            try {
                var activitiesService = provider.GetRequiredService<IActivitiesService>();
                await activitiesService.CompressActivitiesAsync(CancellationToken.None);
            } catch (Exception e) {
                logger.LogError(e, "[Weekly Task] Unexpected error when compressing activities in database.");
            }
        }

        private async Task RunClientWorkerAsync(DateTime date) {
            await RetrieveFocusStats();

            if (ShouldSubmitToAPI(date)) {
                await SubmitDataToAPI();
            }
        }

        private Task SubmitDataToAPI() {
            return Task.CompletedTask; // TODO: Call API
        }

        private static bool ShouldSubmitToAPI(DateTime date) {
            return date.Minute == 0 && date.Second == 0;
        }

        private Task RetrieveFocusStats() {
            return Task.CompletedTask; // TODO: Call Windows API
        }

        public override bool ShouldRunNow(LumenModuleRunsOnFlag currentEnv, DateTime date) {
            return currentEnv switch {
                LumenModuleRunsOnFlag.API => (date.Second + date.Minute + date.Hour + date.DayOfWeek) == 0, // Run once a week
                _ => false,
            };
        }

        public override Task ShutdownAsync() {
            // Nothing to do
            return Task.CompletedTask;
        }

        public static new void SetupServices(LumenModuleRunsOnFlag currentEnv, IServiceCollection serviceCollection, string? postgresConnectionString) {
            if (currentEnv == LumenModuleRunsOnFlag.API) {
                serviceCollection.AddDbContext<FocusStatsContext>(o => o.UseNpgsql(postgresConnectionString, x => x.MigrationsHistoryTable("__EFMigrationsHistory", FocusStatsContext.SCHEMA_NAME)));
                serviceCollection.AddTransient<IActivitiesService, ActivitiesService>();
                serviceCollection.AddTransient<ICleaningRulesService, CleaningRulesService>();
                serviceCollection.AddTransient<ITaggingRulesService, TaggingRulesService>();
            } else if (currentEnv == LumenModuleRunsOnFlag.Worker) {
                serviceCollection.AddHttpClient();
            }
        }

        public override Type GetDatabaseContextType() {
            return typeof(FocusStatsContext);
        }
    }
}
