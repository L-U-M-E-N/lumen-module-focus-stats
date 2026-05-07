using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;

namespace Lumen.Modules.FocusStats.Service {
    public class Program {
        public static void Main(string[] args) {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

            builder.Configuration
                .AddUserSecrets<Program>()
                .AddJsonFile(AppContext.BaseDirectory + "\\appsettings.json");

            LoggerProviderOptions.RegisterProviderOptions<EventLogSettings, EventLogLoggerProvider>(builder.Services);

            builder.Services.AddSingleton<GetActivitiesService>();
            builder.Services.AddHostedService<WindowsBackgroundService>();

            IHost host = builder.Build();
            host.Run();
        }
    }
}
