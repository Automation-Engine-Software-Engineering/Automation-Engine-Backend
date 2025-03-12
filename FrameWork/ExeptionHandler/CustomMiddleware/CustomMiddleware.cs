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

namespace FrameWork.ExeptionHandler.CustomMiddleware
{
    public class CustomMiddleware
    {
        private readonly RequestDelegate _next;

        public CustomMiddleware(RequestDelegate next)
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
                object outPut = new ResultViewModel() { Message = ex.Message, Status = false, Data = ex, StatusCode = ex.StatusId };
                string jsonString = JsonConvert.SerializeObject(outPut);

                // Convert JSON string to byte array  
                byte[] byteArray = Encoding.UTF8.GetBytes(jsonString);
                context.Response.ContentType = "application/json"; // set content type
                context.Response.StatusCode = (int)HttpStatusCode.Accepted; // status code you want to return
                await context.Response.Body.WriteAsync(byteArray, 0, byteArray.Length, CancellationToken.None);

            }
            catch (Exception ex)
            {
                object outPut = new ResultViewModel() { Message = "خطایی در عملیات رخ داده است (درصورت اطمینان از صحت داده های خود و تکرار مجدد با پشتیبانی تماس حاصل نمایید)", Status = false, StatusCode = 503, Data = ex };
                string jsonString = JsonConvert.SerializeObject(outPut);

                // Convert JSON string to byte array  
                byte[] byteArray = Encoding.UTF8.GetBytes(jsonString);
                context.Response.ContentType = "application/json"; // set content type
                context.Response.StatusCode = (int)HttpStatusCode.Accepted; // status code you want to return
                await context.Response.Body.WriteAsync(byteArray, 0, byteArray.Length, CancellationToken.None);

            }
        }
    }
}
