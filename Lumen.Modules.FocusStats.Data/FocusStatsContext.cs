using Lumen.Modules.FocusStats.Common;
using Lumen.Modules.FocusStats.Common.Rules;

using Microsoft.EntityFrameworkCore;

using System.Text.Json;
using System.Text.RegularExpressions;

namespace Lumen.Modules.FocusStats.Data {
    public class FocusStatsContext(DbContextOptions options) : DbContext(options) {
        public const string SCHEMA_NAME = "focusstats";

        public DbSet<CleaningRule> CleaningRules { get; set; }
        public DbSet<TaggingRule> TaggingRules { get; set; }
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

            userFocusedActivityBuilder.Property<List<string>>(nameof(UserFocusedActivity.Tags))
                .HasColumnType("character varying[]");

            userFocusedActivityBuilder.HasKey(x => new { x.Device, x.StartTime });
            userFocusedActivityBuilder.HasIndex(x => new { x.StartTime, x.SecondsDuration, x.AppOrExe, x.Name, x.Device });
        }

        private static void OnCleaningRuleModelCreating(ModelBuilder modelBuilder) {
            var cleaningRuleActivityBuilder = modelBuilder.Entity<CleaningRule>();

            cleaningRuleActivityBuilder.Property<Guid>(nameof(CleaningRule.Id))
                .HasColumnType("uuid");

            cleaningRuleActivityBuilder.Property<Regex>(nameof(CleaningRule.Regex))
                .HasColumnType("character varying")
                .HasConversion(
                    v => v.ToString(),
                    v => new Regex(v, RegexOptions.Compiled)
                );

            cleaningRuleActivityBuilder.Property<string>(nameof(CleaningRule.Replacement))
                .HasColumnType("character varying");

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

            cleaningRuleActivityBuilder.HasKey((x) => x.Id);
            cleaningRuleActivityBuilder.HasIndex(x => new { x.Regex }).IsUnique();
        }

        private static void OnTaggingRuleModelCreating(ModelBuilder modelBuilder) {
            var taggingRuleActivityBuilder = modelBuilder.Entity<TaggingRule>();

            taggingRuleActivityBuilder.Property<Guid>(nameof(TaggingRule.Id))
                .HasColumnType("uuid");

            taggingRuleActivityBuilder.Property<Regex>(nameof(TaggingRule.Regex))
                .HasColumnType("character varying")
                .HasConversion(
                    v => v.ToString(),
                    v => new Regex(v, RegexOptions.Compiled)
                );

            taggingRuleActivityBuilder.Property<ICollection<string>>(nameof(TaggingRule.Tags))
                .HasColumnType("character varying[]");

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

            taggingRuleActivityBuilder.HasKey((x) => x.Id);
            taggingRuleActivityBuilder.HasIndex(x => new { x.Regex }).IsUnique();
        }
    }
}
