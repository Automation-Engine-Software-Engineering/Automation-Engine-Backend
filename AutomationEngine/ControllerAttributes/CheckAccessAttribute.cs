using DataLayer.Models.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Services;
using System.Security.Claims;

namespace AutomationEngine.ControllerAttributes
{

    public class CheckAccess : Attribute, IAsyncAuthorizationFilter
    {
        private readonly WorkFlowEnum _workflow;

        public CheckAccess(WorkFlowEnum workflow)
        {
            _workflow = workflow;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;

            // گرفتن IUserService از Dependency Injection
            var userService = httpContext.RequestServices.GetService<IUserService>();
            if (userService == null)
            {
                context.Result = new JsonResult(new { message = "Internal Server Error: UserService not found" })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
                return;
            }

            // 1. چک کردن توکن
            var userClaims = httpContext.User;
            if (!userClaims.Identity.IsAuthenticated)
            {
                context.Result = new JsonResult(new { message = "Unauthorized" })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            // 2. بررسی IP و UserAgent
            var userIdClaim = userClaims.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                context.Result = new JsonResult(new { message = "Unauthorized: Missing User ID" })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            var userId = int.Parse(userIdClaim.Value);
            var user = await userService.GetUserById(userId);

            if (user == null)
            {
                context.Result = new JsonResult(new { message = "Unauthorized: User not found" })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            var currentIp = httpContext.Connection.RemoteIpAddress?.ToString();
            var currentUserAgent = httpContext.Request.Headers["User-Agent"].ToString();

            if (user.IP != currentIp || user.UserAgent != currentUserAgent)
            {
                context.Result = new JsonResult(new { message = "Forbidden: IP or User-Agent mismatch" })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }

            // 3. چک کردن دسترسی به Workflow
            var roles = userClaims.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var hasAccess = true;// TODO : change to validate workflow access

            if (!hasAccess)
            {
                context.Result = new JsonResult(new { message = "Forbidden: Access Denied" })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }
        }
    }

}
