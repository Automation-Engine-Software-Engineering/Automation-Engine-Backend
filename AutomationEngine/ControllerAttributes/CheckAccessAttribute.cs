using DataLayer.Models.Enums;
using DataLayer.Models.WorkFlows;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
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
            // گرفتن IUserService از Dependency Injection
            var userService = httpContext.RequestServices.GetService<IUserService>() ?? throw new CustomException("IUserService وجود ندارد");
            var tokenGeneratorService = httpContext.RequestServices.GetService<TokenGenerator>() ?? throw new CustomException("TokenGenerator وجود ندارد");
            var workflowService = httpContext.RequestServices.GetService<IWorkFlowService>() ?? throw new CustomException("TokenGenerator وجود ندارد");

            // چک کردن توکن
            var authorizationHeader = httpContext.Request.Headers["Authorization"].ToString();
            if(authorizationHeader.IsNullOrWhiteSpace())
                throw new CustomException(new ValidationDto<object>(false, "Authentication", "NotAuthorized", null).GetMessage(403));

            var token = authorizationHeader.Substring("Bearer ".Length).Trim();

            ClaimsPrincipal userClaims = tokenGeneratorService.ValidateRefreshToken(token);

            // بررسی IP و UserAgent
            var userIdClaim = userClaims.FindFirst(ClaimTypes.Name);

            var userId = int.Parse(userIdClaim.Value);
            var user = await userService.GetUserById(userId);

            if (user == null)
                throw new CustomException("کاربر یافت نشد");

            var currentIp = httpContext.GetIP();
            var currentUserAgent = httpContext.GetUserAgent();
            if (user.IP != currentIp || user.UserAgent != currentUserAgent)
            {
                throw new CustomException(new ValidationDto<(string, string)>(false, "Authentication", "NotAuthorized", (currentIp, currentUserAgent)).GetMessage(403));
            }

            // چک کردن دسترسی به Workflow
            var roles = userClaims.FindAll(ClaimTypes.Role).Select(r => r.Value).FirstOrDefault();

            if (workflowId != null)
            {
                var workflow = await workflowService.GetWorFlowIncRolesById(workflowId.Value);
                var hasAccess = workflow.Role_WorkFlows.Any(x => x.RoleId.ToString() == roles);
                if (!hasAccess)
                {
                    throw new CustomException(new ValidationDto<string>(false, "Authentication", "NotAuthorized", roles).GetMessage(403));
                }
            }
        }
    }
}
