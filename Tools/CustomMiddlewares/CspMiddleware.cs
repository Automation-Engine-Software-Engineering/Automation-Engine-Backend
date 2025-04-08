using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools.CustomMiddlewares
{
    public class CspMiddleware
    {
        private readonly RequestDelegate _next;

        public CspMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // تنظیم CSP برای سایر مسیرها
            context.Response.OnStarting(() =>
            {
                context.Response.Headers.Add("Content-Security-Policy", "script-src 'self'; style-src 'self'; object-src 'none';");
                return Task.CompletedTask;
            });
            await _next(context);
        }
    }
}
