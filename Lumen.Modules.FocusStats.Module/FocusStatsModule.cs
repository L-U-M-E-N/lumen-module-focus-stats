using Lumen.Modules.FocusStats.Common;
using Lumen.Modules.FocusStats.Common.Rules;
using Lumen.Modules.FocusStats.Data;
using Lumen.Modules.Sdk;

using Microsoft.EntityFrameworkCore;

using System.Text.Json;

namespace Lumen.Modules.FocusStats.Module {
    public class FocusStatsModule : LumenModuleBase {
        private static readonly List<CleaningRule> CleaningRules = [];
        private static readonly JsonSerializerOptions jsonSerializerOptions = new() {
            PropertyNameCaseInsensitive = true,
        };

        public FocusStatsModule(LumenModuleRunsOnFlag runsOn, ILogger<LumenModuleBase> logger) : base(runsOn, logger) {

        }

        public override async Task InitAsync(LumenModuleRunsOnFlag currentEnv) {
            logger.LogTrace($"[{DateTime.Now}] Loading settings ...");

            var rulesDict = JsonSerializer.Deserialize<Dictionary<string, ParsedRule>>(File.ReadAllText("rules.json"), jsonSerializerOptions);
            foreach (var item in rulesDict) {
                CleaningRules.Add(new CleaningRule(item.Key, item.Value.Replacement, Enum.Parse<RuleTarget>(item.Value.Target), item.Value.Tests));
            }

            // TODO: Run rules on database data
        }

        public override Task RunAsync(LumenModuleRunsOnFlag currentEnv) {
            return currentEnv switch {
                LumenModuleRunsOnFlag.UI => RunUIAsync(),
                _ => throw new NotImplementedException(),
            };
        }

        private async Task RunUIAsync() {
            await RetrieveFocusStats();

            if (ShouldSubmitToAPI()) {
                SubmitDataToAPI();
            }
        }

        private void SubmitDataToAPI() {
            // TODO: Call API
        }

        private static bool ShouldSubmitToAPI() {
            var now = DateTime.Now;
            return now.Minute == 0 && now.Second == 0;
        }

        private async Task RetrieveFocusStats() {
            // TODO: Call Windows API
            var exe = "";
            var name = "";


        }

        public override bool ShouldRunNow(LumenModuleRunsOnFlag currentEnv) {
            return currentEnv switch {
                LumenModuleRunsOnFlag.UI => DateTime.Now.Second == 0,
                _ => false,
            };
        }

        public override async Task ShutdownAsync() {
            // Nothing to do here
        }

        public static new void SetupServices(LumenModuleRunsOnFlag currentEnv, IServiceCollection serviceCollection, string? postgresConnectionString) {
            if (currentEnv == LumenModuleRunsOnFlag.API) {
                serviceCollection.AddDbContext<FocusStatsContext>(o => o.UseNpgsql(postgresConnectionString, x => x.MigrationsHistoryTable("__EFMigrationsHistory", FocusStatsContext.SCHEMA_NAME)));
            }
        }

        public override Type GetDatabaseContextType() {
            return typeof(FocusStatsContext);
        }
    }
}
