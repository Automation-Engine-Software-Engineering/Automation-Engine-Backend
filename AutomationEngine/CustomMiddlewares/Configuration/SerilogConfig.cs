using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Settings.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationEngine.CustomMiddlewares.Configuration
{
    public static class SerilogConfig
    {
        public static void AddApplicationLogging(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                 .ReadFrom.Configuration(configuration, new ConfigurationReaderOptions())
                 .Enrich.FromLogContext()
                 .CreateLogger();
            Log.Information("Application Starting...");
        }
    }
}
