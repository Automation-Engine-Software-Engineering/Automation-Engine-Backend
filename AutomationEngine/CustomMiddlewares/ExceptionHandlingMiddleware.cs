using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using FrameWork.ExeptionHandler.ExeptionModel;
using ViewModels.ViewModels.Workflow;
using FrameWork;
using AutomationEngine.Controllers;
using Microsoft.Extensions.Logging;

namespace AutomationEngine.CustomMiddlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // فراخوانی middleware بعدی
                await _next(context);
            }
            catch (CustomException ex)
            {
                _logger.LogWarning(ex,ex.Message,ex.Data);
                await ExceptionHandling.HandleCustomExceptionAsync(context, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message, ex.Data);
                await ExceptionHandling.HandleGeneralExceptionAsync(context, ex);
            }
        }
    }
    public static class ExceptionHandling
    {
        public static async Task HandleCustomExceptionAsync(HttpContext context, CustomException ex)
        {
            var output = new ResultViewModel()
            {
                message = ex.Message,
                status = false,
                statusCode = ex.StatusId
            };
            
            var environment = context.RequestServices.GetService<IWebHostEnvironment>();
            if (environment != null && environment.IsDevelopment())
            {
                output.data = ex;
            }

            await WriteJsonResponseAsync(context, ex.StatusId, output);
        }

        public static async Task HandleGeneralExceptionAsync(HttpContext context, Exception ex)
        {
            var output = new ResultViewModel()
            {
                message = "خطایی در عملیات رخ داده است (درصورت اطمینان از صحت داده های خود و تکرار مجدد با پشتیبانی تماس حاصل نمایید)",
                status = false,
                statusCode = 503,
                data = null
            };

            var environment = context.RequestServices.GetService<IWebHostEnvironment>();
            if (environment != null && environment.IsDevelopment())
            {
                output.data = ex;
            }

            // تعیین کد وضعیت بر اساس نوع استثنا
            int statusCode = ex switch
            {
                ArgumentNullException => 400, // Bad Request
                UnauthorizedAccessException => 401, // Unauthorized
                KeyNotFoundException => 404, // Not Found
                InvalidOperationException => 405, // Method Not Allowed
                _ => 503 // Service Unavailable
            };

            await WriteJsonResponseAsync(context, statusCode, output);
        }

        private static async Task WriteJsonResponseAsync(HttpContext context, int statusCode, ResultViewModel output)
        {
            string jsonString = JsonConvert.SerializeObject(output);
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonString);

            context.Response.ContentType = "application/json"; // تنظیم نوع محتوا
            context.Response.StatusCode = statusCode; // تنظیم کد وضعیت

            await context.Response.Body.WriteAsync(byteArray, 0, byteArray.Length, CancellationToken.None);
        }
    }
    }
