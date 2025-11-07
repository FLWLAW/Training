using Microsoft.Extensions.Configuration;

namespace Training.Website
{
    public class Configuration
    {
        internal static string? DatabaseConnectionString()
        {
            IConfiguration config = GetConfiguration();
#if DEBUG
            return config.GetValue<string>("TrainingDatabaseConnectionString_DEVELOPMENT");
#else
            return config.GetValue<string>("TrainingDatabaseConnectionString_PRODUCTION");
#endif
        }

// ==============================================================================================================================================================================================================================================================

        private static IConfiguration GetConfiguration() =>
            new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
    }
}
