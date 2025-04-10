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
using ViewModels.ViewModels.WorkFlow;
using FrameWork;

namespace AutomationEngine.CustomMiddlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Call the next middleware in the pipeline
                await _next(context);
            }
            catch (CustomException ex)
            {
                var output = new ResultViewModel() { message = ex.Message, status = false, statusCode = ex.StatusId };

                var environment = context.RequestServices.GetService<IWebHostEnvironment>();
                if (environment != null && environment.IsDevelopment())
                    output.data = ex;

                string jsonString = JsonConvert.SerializeObject(output);
                // Convert JSON string to byte array  
                byte[] byteArray = Encoding.UTF8.GetBytes(jsonString);
                context.Response.ContentType = "application/json"; // set content type
                context.Response.StatusCode = ex.StatusId; // status code you want to return
                await context.Response.Body.WriteAsync(byteArray, 0, byteArray.Length, CancellationToken.None);
            }
            catch (Exception ex)
            {
                var output = new ResultViewModel() { message = "خطایی در عملیات رخ داده است (درصورت اطمینان از صحت داده های خود و تکرار مجدد با پشتیبانی تماس حاصل نمایید)", status = false, statusCode = 503, data = ex };

                var environment = context.RequestServices.GetService<IWebHostEnvironment>();
                if (environment != null && environment.IsDevelopment())
                    output.data = ex;

                string jsonString = JsonConvert.SerializeObject(output);
                // Convert JSON string to byte array  
                byte[] byteArray = Encoding.UTF8.GetBytes(jsonString);
                context.Response.ContentType = "application/json"; // set content type
                int statusCode = 503;
                switch (ex)
                {
                    case ArgumentNullException argNullEx:
                        statusCode = 400; // Bad Request
                        break;
                    case UnauthorizedAccessException unauthorizedEx:
                        statusCode = 401; // Unauthorized
                        break;
                    case KeyNotFoundException keyNotFoundEx:
                        statusCode = 404; // Not Found
                        break;
                    case InvalidOperationException invalidOpEx:
                        statusCode = 405; // Method Not Allowed
                        break;
                }
                context.Response.StatusCode = statusCode; // status code to return
                await context.Response.Body.WriteAsync(byteArray, 0, byteArray.Length, CancellationToken.None);
            }
        }
    }
}
