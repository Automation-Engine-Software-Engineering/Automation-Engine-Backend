using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationEngine.CustomMiddlewares.Security
{
    public static class CorsConfiguration
    {
        public static void ConfigureCors(this WebApplication app, string[] headers,string audience)
        {
            app.UseCors(builder =>
            {
                if (app.Environment.IsDevelopment())
                {
                    // تنظیمات CORS در محیط Development
                    builder
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowAnyOrigin()
                        .SetPreflightMaxAge(TimeSpan.FromDays(15));
                }
                else
                {
                    builder
                      .WithHeaders(headers.ToArray())
                      .WithOrigins(audience)
                      .WithMethods("GET", "POST")
                      .AllowAnyOrigin()
                      .SetPreflightMaxAge(TimeSpan.FromMinutes(15));
                }
            });
        }
    }
}
