using FrameWork.ExeptionHandler.ExeptionModel;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using Tools.AuthoraizationTools;

namespace AutomationEngine.CustomMiddlewares.Security
{
    public static class RateLimiterConfig
    {
        public static void ConfigureRateLimiter(this IServiceCollection services, IConfiguration configuration)
        {
            var exceptionRateLimiter = new CustomException("AppSettings", "RateLimiter");
            var queueLimit = int.Parse(configuration["RateLimiter:QueueLimit"] ?? throw exceptionRateLimiter);
            var permitLimit = int.Parse(configuration["RateLimiter:PermitLimit"] ?? throw exceptionRateLimiter);
            var window = TimeSpan.Parse(configuration["RateLimiter:Window"] ?? throw exceptionRateLimiter);

            var queueLimitLogin = int.Parse(configuration["RateLimiter:QueueLimitLogin"] ?? throw exceptionRateLimiter);
            var permitLimitLogin = int.Parse(configuration["RateLimiter:PermitLimitLogin"] ?? throw exceptionRateLimiter);
            var windowLogin = TimeSpan.Parse(configuration["RateLimiter:WindowLogin"] ?? throw exceptionRateLimiter);

            services.AddRateLimiter(options =>
            {
                // محدودیت سراسری: حداکثر 5 درخواست در هر 10 ثانیه
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.GetIP(),
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = permitLimit, // تعداد درخواست مجاز
                            Window = window, // بازه زمانی
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = queueLimit,// حداکثر تعداد درخواست در صف
                            AutoReplenishment = true
                        });
                });
                options.AddPolicy("LoginRateLimit", context =>
                   RateLimitPartition.GetFixedWindowLimiter(
                       context.GetIP(),
                       partition => new FixedWindowRateLimiterOptions
                       {
                           PermitLimit = permitLimitLogin,
                           Window = windowLogin, // بازه زمانی
                           QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                           QueueLimit = queueLimitLogin,
                           AutoReplenishment = true
                       }
                   )
               );
                // response of error
                options.OnRejected = (context, token) =>
                {
                    var ex = new CustomException("Authentication", "TooManyRequests", null);
                    throw ex;
                };
            });
        }
    }
}
