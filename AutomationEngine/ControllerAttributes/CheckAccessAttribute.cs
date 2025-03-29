using DataLayer.DbContext;
using DataLayer.Models.Enums;
using DataLayer.Models.WorkFlows;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using Services;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using Tools.AuthoraizationTools;
using Tools.TextTools;

namespace AutomationEngine.ControllerAttributes
{
    public class CheckAccess : Attribute, IAsyncAuthorizationFilter
    {
        private readonly WorkFlowEnum? _workflow;

        public CheckAccess(WorkFlowEnum workflow)
        {
            _workflow = workflow;
        }
        public CheckAccess()
        {
            _workflow = null;
        }
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;
            await Authorization.Authorize(httpContext, (int?)_workflow);
        }

    }
    public static class Authorization
    {
        public static async Task Authorize(HttpContext httpContext, int? workflowId = null)
        {
            //ویندوز: cmd: set ASPNETCORE_ENVIRONMENT = Production
            var environment = httpContext.RequestServices.GetService<IWebHostEnvironment>();
            if (environment != null && environment.IsDevelopment())
            {
                // در حالت Development هیچ چکی انجام نمی‌شود
                return;
            }

            // گرفتن IUserService از Dependency Injection
            var userService = httpContext.RequestServices.GetService<IUserService>() ?? throw new CustomException("IUserService وجود ندارد");
            var tokenGeneratorService = httpContext.RequestServices.GetService<TokenGenerator>() ?? throw new CustomException("TokenGenerator وجود ندارد");
            var workflowService = httpContext.RequestServices.GetService<IWorkFlowService>() ?? throw new CustomException("TokenGenerator وجود ندارد");

            // چک کردن توکن
            var token = httpContext.Request.Headers["Authorization"].ToString();
            if (token.IsNullOrWhiteSpace())
                throw new CustomException<object>(new ValidationDto<object>(false, "Authentication", "NotAuthorized", null), 403);

            tokenGeneratorService.ValidateRefreshToken(token);

            // بررسی IP و UserAgent
            var userIdClaim = tokenGeneratorService.GetClaimFromToken(token, ClaimTypes.Name);

            var userId = int.Parse(userIdClaim ?? "0");
            var user = await userService.GetUserById(userId);

            if (user == null)
                throw new CustomException("کاربر یافت نشد");

            var currentIp = httpContext.GetIP();
            var currentUserAgent = httpContext.GetUserAgent();
            if (user.IP != currentIp || user.UserAgent != currentUserAgent)
            {
                throw new CustomException<(string, string)>(new ValidationDto<(string, string)>(false, "Authentication", "NotAuthorized", (currentIp, currentUserAgent)),403);
            }

            // چک کردن دسترسی به Workflow
            var roles = tokenGeneratorService.GetClaimFromToken(token, ClaimTypes.Role);

            if (workflowId != null)
            {
                var workflow = await workflowService.GetWorFlowIncRolesById(workflowId.Value);
                var hasAccess = workflow.Role_WorkFlows.Any(x => x.RoleId.ToString() == roles);
                if (!hasAccess)
                {
                    throw new CustomException<(string, string)>(new ValidationDto<(string, string)>(false, "Authentication", "NotAuthorized", (currentIp, currentUserAgent)), 403);
                }
            }
        }
    }
}
