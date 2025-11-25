using Microsoft.Extensions.Configuration;

namespace Training.Website
{
    public class Configuration
    {
        private static readonly IConfiguration _configuration = GetConfiguration();

        internal static string? DatabaseConnectionString_OPS()
        {
#if DEBUG
            return _configuration.GetValue<string>("TrainingDatabaseConnectionString_DEVELOPMENT");
#else
            return _configuration.GetValue<string>("TrainingDatabaseConnectionString_PRODUCTION");
#endif
        }

        internal static string? DatabaseConnectionString_CMS()
        {
#if DEBUG
            return _configuration.GetValue<string>("CmsDatabaseConnectionString_DEVELOPMENT");
#else
            return _configuration.GetValue<string>("CmsDatabaseConnectionString_PRODUCTION");
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
