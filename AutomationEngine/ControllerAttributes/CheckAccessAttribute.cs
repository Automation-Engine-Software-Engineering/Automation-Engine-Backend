using DataLayer.Models.Enums;
using FrameWork.ExeptionHandler.ExeptionModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Tools.AuthoraizationTools;

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
            var userService = httpContext.RequestServices.GetService<IUserService>() ?? throw new CustomException("IUserService وجود ندارد");
            var tokenGeneratorService = httpContext.RequestServices.GetService<TokenGenerator>() ?? throw new CustomException("TokenGenerator وجود ندارد");
            // 1. چک کردن توکن
            var authorizationHeader = httpContext.Request.Headers["Authorization"].ToString();

            var token = authorizationHeader.Substring("Bearer ".Length).Trim();

            ClaimsPrincipal userClaims = tokenGeneratorService.ValidateRefreshToken(token);

            // 2. بررسی IP و UserAgent
            var userIdClaim = userClaims.FindFirst(ClaimTypes.NameIdentifier);

            var userId = int.Parse(userIdClaim.Value);
            var user = await userService.GetUserById(userId);

            if (user == null)
                throw new CustomException("کاربر یافت نشد");
            var currentIp = httpContext.Connection.RemoteIpAddress?.ToString();
            var currentUserAgent = httpContext.Request.Headers["User-Agent"].ToString();

            if (user.IP != currentIp || user.UserAgent != currentUserAgent)
            {
                //throw new CustomException("Forbidden: IP or User-Agent mismatch");
            }

            // 3. چک کردن دسترسی به Workflow
            var roles = userClaims.FindAll(ClaimTypes.Role).Select(r => r.Value).FirstOrDefault();

            var hasAccess = ValidateWorkflowAccess(roles, _workflow);

            if (!hasAccess)
            {
                throw new CustomException("Forbidden Access Denied");
            }

        }

        private bool ValidateWorkflowAccess(string roles, WorkFlowEnum workflow)
        {
            // TODO: پیاده‌سازی منطق اعتبارسنجی Workflow
            return true; // یا منطق دلخواه برای چک کردن دسترسی‌ها
        }
    }
}
