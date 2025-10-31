using Lumen.Modules.FocusStats.Common;
using Lumen.Modules.FocusStats.Common.Rules;

using Microsoft.EntityFrameworkCore;

using System.Text.Json;
using System.Text.RegularExpressions;

namespace Lumen.Modules.FocusStats.Data {
    public class FocusStatsContext(DbContextOptions options) : DbContext(options) {
        public const string SCHEMA_NAME = "focusstats";

        public DbSet<UserFocusedActivity> Activities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.HasDefaultSchema(SCHEMA_NAME);

            OnUserFocusedActivityModelCreating(modelBuilder);
            OnCleaningRuleModelCreating(modelBuilder);
            OnTaggingRuleModelCreating(modelBuilder);
        }

        private static void OnUserFocusedActivityModelCreating(ModelBuilder modelBuilder) {
            var userFocusedActivityBuilder = modelBuilder.Entity<UserFocusedActivity>();
            userFocusedActivityBuilder.Property<DateTime>(nameof(UserFocusedActivity.StartTime))
                .HasColumnType("timestamp with time zone");

            userFocusedActivityBuilder.Property<int>(nameof(UserFocusedActivity.SecondsDuration))
                .HasColumnType("integer");

            userFocusedActivityBuilder.Property<string>(nameof(UserFocusedActivity.AppOrExe))
                .HasColumnType("character varying");

            userFocusedActivityBuilder.Property<string>(nameof(UserFocusedActivity.Name))
                .HasColumnType("character varying");

            userFocusedActivityBuilder.Property<string>(nameof(UserFocusedActivity.Device))
                .HasColumnType("character varying")
                .HasDefaultValue("Unknown");

            userFocusedActivityBuilder.Property<string[]>(nameof(UserFocusedActivity.Tags))
                .HasColumnType("character varying[]")
                .HasDefaultValue("[]");

            userFocusedActivityBuilder.HasKey(x => new { x.Device, x.StartTime });
        }

        private static void OnCleaningRuleModelCreating(ModelBuilder modelBuilder) {
            var cleaningRuleActivityBuilder = modelBuilder.Entity<CleaningRule>();
            cleaningRuleActivityBuilder.Property<Regex>(nameof(CleaningRule.Regex))
                .HasColumnType("character varying")
                .HasConversion(
                    v => v.ToString(),
                    v => new Regex(v, RegexOptions.Compiled)
                );

            cleaningRuleActivityBuilder.Property<RuleTarget>(nameof(CleaningRule.Target))
                .HasColumnType("character varying")
                .HasConversion(
                    v => v.ToString(),
                    v => Enum.Parse<RuleTarget>(v)
                );

            var emptyOptions = new JsonSerializerOptions();
            cleaningRuleActivityBuilder.Property<Dictionary<string, string>>(nameof(CleaningRule.Tests))
                .HasColumnType("character varying")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, emptyOptions),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, emptyOptions)!
                );

            cleaningRuleActivityBuilder.HasKey(x => new { x.Regex });
        }

        private static void OnTaggingRuleModelCreating(ModelBuilder modelBuilder) {
            var taggingRuleActivityBuilder = modelBuilder.Entity<TaggingRule>();
            taggingRuleActivityBuilder.Property<Regex>(nameof(TaggingRule.Regex))
                .HasColumnType("character varying")
                .HasConversion(
                    v => v.ToString(),
                    v => new Regex(v, RegexOptions.Compiled)
                );

            taggingRuleActivityBuilder.Property<RuleTarget>(nameof(TaggingRule.Target))
                .HasColumnType("character varying")
                .HasConversion(
                    v => v.ToString(),
                    v => Enum.Parse<RuleTarget>(v)
                );

            var emptyOptions = new JsonSerializerOptions();
            taggingRuleActivityBuilder.Property<Dictionary<string, bool>>(nameof(TaggingRule.Tests))
                .HasColumnType("character varying")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, emptyOptions),
                    v => JsonSerializer.Deserialize<Dictionary<string, bool>>(v, emptyOptions)!
                );

            taggingRuleActivityBuilder.HasKey(x => new { x.Regex });
        }
    }
}
