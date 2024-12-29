using Lumen.Modules.FocusStats.Common;

using Microsoft.EntityFrameworkCore;

namespace Lumen.Modules.FocusStats.Data {
    public class FocusStatsContext : DbContext {
        public const string SCHEMA_NAME = "focusstats";

        public FocusStatsContext(DbContextOptions options) : base(options) {
        }

        public DbSet<UserFocusedActivity> Activities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.HasDefaultSchema(SCHEMA_NAME);

            var userFocusedActivityBuilder = modelBuilder.Entity<UserFocusedActivity>();
            userFocusedActivityBuilder.Property<DateTime>(nameof(UserFocusedActivity.StartTime))
                .HasColumnType("timestamp with time zone");

            userFocusedActivityBuilder.Property<int>(nameof(UserFocusedActivity.SecondsDuration))
                .HasColumnType("integer");

            userFocusedActivityBuilder.Property<string>(nameof(UserFocusedActivity.ProgramExe))
                .HasColumnType("character varying");

            userFocusedActivityBuilder.Property<string>(nameof(UserFocusedActivity.ProgramName))
                .HasColumnType("character varying");

            userFocusedActivityBuilder.HasKey(x => new { x.ProgramExe, x.ProgramName });

        }
    }
}
