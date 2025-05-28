using DataLayer.DbContext;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationEngine.CustomMiddlewares.Configuration
{
    public static class DatabaseConfig
    {
        public static void SQLDatabaseConfig(this IServiceCollection services, IConfiguration configuration)
        {
            //Add-Migration InitialCreate -Context Context
            //Update-Database InitialCreate -Context Context
            services.AddDbContext<Context>(options =>
                       options.UseSqlServer(configuration.GetConnectionString("Basic")));

            //Add-Migration InitialCreate -DbContext DynamicDbContext
            //Update-Database InitialCreate -DbContext DynamicDbContext
            services.AddDbContext<DynamicDbContext>(options =>
                       options.UseSqlServer(configuration.GetConnectionString("Dynamic")));

            services.AddScoped<Context>();
            services.AddScoped<DynamicDbContext>();
        }
    }
}
