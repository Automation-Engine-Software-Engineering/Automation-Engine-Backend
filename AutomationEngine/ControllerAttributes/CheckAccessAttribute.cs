using DataLayer.DbContext;
using Entities.Models.Enums;
using Entities.Models.WorkFlows;
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
        public static async Task<TokenClaims> Authorize(this HttpContext httpContext, int? workflowId = null)
        {
            // گرفتن IUserService از Dependency Injection
            var userService = httpContext.RequestServices.GetService<IUserService>() ?? throw new CustomException("IUserService وجود ندارد");
            var tokenGeneratorService = httpContext.RequestServices.GetService<TokenGenerator>() ?? throw new CustomException("TokenGenerator وجود ندارد");
            var encryptionToolService = httpContext.RequestServices.GetService<EncryptionTool>() ?? throw new CustomException("EncryptionTool وجود ندارد");
            var workflowService = httpContext.RequestServices.GetService<IWorkFlowService>() ?? throw new CustomException("TokenGenerator وجود ندارد");



            //ویندوز: cmd: set ASPNETCORE_ENVIRONMENT = Production
            var environment = httpContext.RequestServices.GetService<IWebHostEnvironment>();
            if (environment != null && environment.IsDevelopment())
            {
                var tokenAuthorization = httpContext.Request.Headers["Authorization"].ToString();
                // در حالت Development هیچ چکی انجام نمی‌شود
                var claimsAuthorization = tokenGeneratorService.ValidateToken(tokenAuthorization, false, false);
                var userIdAuthorization = claimsAuthorization.FindFirstValue(nameof(TokenClaims.UserId));
                var roleIdAuthorization = claimsAuthorization.FindFirstValue(nameof(TokenClaims.RoleId));

                // مقداردهی کلاس TokenClaims
                var tokenClaim = new TokenClaims
                {
                    UserId = userIdAuthorization.IsNullOrWhiteSpace() ? 0 : int.Parse(userIdAuthorization),
                    RoleId = roleIdAuthorization.IsNullOrWhiteSpace() ? 0 : int.Parse(roleIdAuthorization),
                    Token = tokenAuthorization
                };
                return tokenClaim;
            }
            // چک کردن توکن
            var encryptedToken = httpContext.Request.Cookies["access_token"];
            var token = encryptionToolService.DecryptCookie(encryptedToken);

            if (token == null || token.IsNullOrWhiteSpace())
                throw new CustomException<object>(new ValidationDto<object>(false, "Authentication", "NotAuthorized", null), 403);

            var claims = tokenGeneratorService.ValidateToken(token);

            var userIdClaim = claims.FindFirstValue(nameof(TokenClaims.UserId));
            var roleIdClaim = claims.FindFirstValue(nameof(TokenClaims.RoleId));

            // مقداردهی کلاس TokenClaims
            var tokenClaims = new TokenClaims
            {
                UserId = userIdClaim.IsNullOrWhiteSpace() ? 0 : int.Parse(userIdClaim),
                RoleId = roleIdClaim.IsNullOrWhiteSpace() ? 0 : int.Parse(roleIdClaim),
                Token = token
            };

            var user = await userService.GetUserById(tokenClaims.UserId);

            if (user == null)
                throw new CustomException("کاربر یافت نشد");

            var currentIp = httpContext.GetIP();
            var currentUserAgent = httpContext.GetUserAgent();
            if (user.IP != currentIp || user.UserAgent != currentUserAgent)
            {
                throw new CustomException<(string, string)>(new ValidationDto<(string, string)>(false, "Authentication", "NotAuthorized", (currentIp, currentUserAgent)), 403);
            }

            // چک کردن دسترسی به Workflow
            var roleId = tokenClaims.RoleId;

            if (workflowId != null)
            {
                var workflow = await workflowService.GetWorFlowIncRolesById(workflowId.Value);
                var hasAccess = workflow.Role_WorkFlows.Any(x => x.RoleId == roleId);
                if (!hasAccess)
                {
                    throw new CustomException<(string, string)>(new ValidationDto<(string, string)>(false, "Authentication", "NotAuthorized", (currentIp, currentUserAgent)), 403);
                }
            }
            return tokenClaims;
        }
        public static async Task<TokenClaims> AuthorizeRefreshToken(this HttpContext httpContext)
        {
            // گرفتن IUserService از Dependency Injection
            var userService = httpContext.RequestServices.GetService<IUserService>() ?? throw new CustomException("IUserService وجود ندارد");
            var tokenGeneratorService = httpContext.RequestServices.GetService<TokenGenerator>() ?? throw new CustomException("TokenGenerator وجود ندارد");
            var encryptionToolService = httpContext.RequestServices.GetService<EncryptionTool>() ?? throw new CustomException("EncryptionTool وجود ندارد");

            //ویندوز: cmd: set ASPNETCORE_ENVIRONMENT = Production
            var environment = httpContext.RequestServices.GetService<IWebHostEnvironment>();
            if (environment != null && environment.IsDevelopment())
            {
                var tokenAuthorization = httpContext.Request.Headers["Authorization"].ToString();
                // در حالت Development هیچ چکی انجام نمی‌شود
                var claimsAuthorization = tokenGeneratorService.ValidateToken(tokenAuthorization, true, false);
                var userId = claimsAuthorization.FindFirstValue(nameof(TokenClaims.UserId));
                var roleId = claimsAuthorization.FindFirstValue(nameof(TokenClaims.RoleId));

                // مقداردهی کلاس TokenClaims
                var tokenClaim = new TokenClaims
                {
                    UserId = userId.IsNullOrWhiteSpace() ? 0 : int.Parse(userId),
                    RoleId = roleId.IsNullOrWhiteSpace() ? 0 : int.Parse(roleId),
                    Token = tokenAuthorization
                };
                return tokenClaim;
            }
            // چک کردن توکن
            var encryptedToken = httpContext.Request.Cookies["access_token"];
            var token = encryptionToolService.DecryptCookie(encryptedToken);

            if (token == null || token.IsNullOrWhiteSpace())
                throw new CustomException<object>(new ValidationDto<object>(false, "Authentication", "NotAuthorized", null), 403);

            var claims = tokenGeneratorService.ValidateToken(token);

            var userIdClaim = claims.FindFirstValue(nameof(TokenClaims.UserId));
            var roleIdClaim = claims.FindFirstValue(nameof(TokenClaims.RoleId));

            // مقداردهی کلاس TokenClaims
            var tokenClaims = new TokenClaims
            {
                UserId = userIdClaim.IsNullOrWhiteSpace() ? 0 : int.Parse(userIdClaim),
                RoleId = roleIdClaim.IsNullOrWhiteSpace() ? 0 : int.Parse(roleIdClaim),
                Token = token
            };

            var user = await userService.GetUserById(tokenClaims.UserId);

            if (user == null)
                throw new CustomException("کاربر یافت نشد");

            var currentIp = httpContext.GetIP();
            var currentUserAgent = httpContext.GetUserAgent();
            if (user.IP != currentIp || user.UserAgent != currentUserAgent)
            {
                throw new CustomException<(string, string)>(new ValidationDto<(string, string)>(false, "Authentication", "NotAuthorized", (currentIp, currentUserAgent)), 403);
            }

            return tokenClaims;
        }
    }
}
