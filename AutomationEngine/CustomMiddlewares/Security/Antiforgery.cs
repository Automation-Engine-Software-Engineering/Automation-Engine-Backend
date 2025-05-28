using AutomationEngine.CustomMiddlewares.Security;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Settings.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationEngine.CustomMiddlewares.Security
{
    public static class Antiforgery
    {
        public static void AddCustomAntiforgery(this IServiceCollection services)
        {
            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = "X-CSRF-TOKEN";
                options.Cookie.HttpOnly = true;
            });
        }
    }
}
