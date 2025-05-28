using DataLayer.DbContext;
using Entities.Models.Enums;
using Entities.Models.MainEngine;
using Entities.Models.Workflows;
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
using ViewModels.ViewModels.AuthenticationDtos;

namespace AutomationEngine.ControllerAttributes
{
    public class CheckAccess : Attribute, IAsyncAuthorizationFilter
    {
        private readonly WorkflowEnum? _workflow;

        public CheckAccess(WorkflowEnum workflow)
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
        private static T GetDependencyService<T>(this HttpContext httpContext)
        {
            var exceptionService = new CustomException("Service", "ServiceNotFound");
            return httpContext.RequestServices.GetService<T>() ?? throw exceptionService;
        }

        private static TokenClaims GetTokenClaims(ClaimsPrincipal claims, string token)
        {
            var userIdClaim = claims?.FindFirstValue(nameof(TokenClaims.UserId));
            var roleIdClaim = claims?.FindFirstValue(nameof(TokenClaims.RoleId));

            return new TokenClaims
            {
                UserId = userIdClaim.IsNullOrWhiteSpace() ? 0 : int.Parse(userIdClaim),
                RoleId = roleIdClaim.IsNullOrWhiteSpace() ? 0 : int.Parse(roleIdClaim),
                Token = token.Replace("Bearer ", "")
            };
        }

        private static async Task<User> GetUserAsync(IUserService userService, int userId)
        {
            var user = await userService.GetUserByIdAsync(userId);
            if (user == null)
                throw new CustomException("Authentication", "NotAuthorized");
            return user;
        }

        private static void ValidateUserAgent(User user, HttpContext httpContext)
        {
            var currentIp = httpContext.GetIP();
            var currentUserAgent = httpContext.GetUserAgent();
            if (user.IP != currentIp || user.UserAgent != currentUserAgent)
            {
                throw new CustomException("Authentication", "NotAuthorized", (currentIp, currentUserAgent));
            }
        }

        private static async Task<TokenClaims> AuthorizeCommon(this HttpContext httpContext, bool isRefreshToken = false, int? workflowId = null)
        {
            var userService = httpContext.GetDependencyService<IUserService>();
            var tokenGeneratorService = httpContext.GetDependencyService<TokenGenerator>();
            var environment = httpContext.GetDependencyService<IWebHostEnvironment>();
            var workflowService = httpContext.GetDependencyService<IWorkflowService>();

            if (environment != null && environment.IsDevelopment())
            {
                var tokenAuthorization = httpContext.Request.Headers["Authorization"].ToString();
                var claimsAuthorization = tokenGeneratorService.ValidateToken(tokenAuthorization, isRefreshToken, false);
                return GetTokenClaims(claimsAuthorization, tokenAuthorization);
            }

            var token = httpContext.Request.Headers["Authorization"].ToString();
            if (token.IsNullOrWhiteSpace())
                throw new CustomException("Authentication", "NotAuthorized");

            var claims = tokenGeneratorService.ValidateToken(token, isRefreshToken);
            var tokenClaims = GetTokenClaims(claims, token);

            var user = await GetUserAsync(userService, tokenClaims.UserId);
            ValidateUserAgent(user, httpContext);

            if (workflowId != null)
            {
                var workflow = await workflowService.GetWorkflowIncRolesById(workflowId.Value);
                var hasAccess = workflow.Role_Workflows?.Any(x => x.RoleId == tokenClaims.RoleId) ?? false;
                if (!hasAccess)
                    throw new CustomException("Authentication", "NotAuthorized", (httpContext.GetIP(), httpContext.GetUserAgent()));
            }

            return tokenClaims;
        }

        public static Task<TokenClaims> Authorize(this HttpContext httpContext, int? workflowId = null) =>
            httpContext.AuthorizeCommon(false, workflowId);

        public static Task<TokenClaims> AuthorizeRefreshToken(this HttpContext httpContext) =>
            httpContext.AuthorizeCommon(true);
    }

}
