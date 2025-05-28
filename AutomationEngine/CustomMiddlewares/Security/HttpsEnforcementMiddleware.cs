using FrameWork.ExeptionHandler.ExeptionModel;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationEngine.CustomMiddlewares.Security
{
    public static class UseHttpsRedirectionMiddleware
    {
        public static void HttpsRedirectionMiddleware(this IApplicationBuilder app, IConfiguration configuration)
        {
            var secure = bool.Parse(configuration["JWTSettings:Secure"] ?? "false");
            if (secure)
            {
                app.UseHttpsRedirection();
                app.Use(async (context, next) =>
                {
                    if (!context.Request.IsHttps)
                        throw new CustomException("Error", "HTTPS");

                    await next();
                });
            }
        }
    }

}
