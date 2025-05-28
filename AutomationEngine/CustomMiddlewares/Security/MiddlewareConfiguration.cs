using AutomationEngine.CustomMiddlewares.Extensions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationEngine.CustomMiddlewares.Security
{
    public static class MiddlewareConfiguration
    {
        public static void ConfigureMiddlewares(this WebApplication app, IConfiguration configuration)
        {
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                app.UseHsts();
                app.UseMiddleware<CspMiddleware>();
                app.HttpsRedirectionMiddleware(configuration);
                app.UseRateLimiter();
            }
        }
    }
}
