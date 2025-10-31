using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Lumen.Modules.FocusStats.Data {
    public class FocusStatsDbContextFactory : IDesignTimeDbContextFactory<FocusStatsContext> {
        public FocusStatsContext CreateDbContext(string[] args) {
            var optionsBuilder = new DbContextOptionsBuilder<FocusStatsContext>();
            optionsBuilder.UseNpgsql();

            return new FocusStatsContext(optionsBuilder.Options);
        }
    }
}
