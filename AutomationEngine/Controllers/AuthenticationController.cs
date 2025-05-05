using AutomationEngine.ControllerAttributes;
using Azure.Core;
using Entities.Models.Enums;
using Entities.Models.MainEngine;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Services;
using System.Net.Http;
using System.Security.Claims;
using Tools.AuthoraizationTools;
using Tools.TextTools;
using ViewModels;
using ViewModels.ViewModels.AuthenticationDtos;
using ViewModels.ViewModels.RoleDtos;

namespace AutomationEngine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly IUserService _userService;
        private readonly TokenGenerator _tokenGenerator;
        private readonly IConfiguration _configuration;
        private readonly EncryptionTool _encryptionTool;
        private readonly IWebHostEnvironment _webHostEnvironment;
        //private readonly string _issuer;
        //private readonly bool _secure;
        //private readonly TimeSpan _accessTokenLifetime;
        //private readonly TimeSpan _refreshTokenLifetime;
        private readonly CookieOptions _accessTokenCookieOptions;
        private readonly CookieOptions _refreshTokenCookieOptions;

        public AuthenticationController(IRoleService roleService, IUserService userService, TokenGenerator tokenGenerator, IConfiguration configuration, EncryptionTool encryptionTool, IWebHostEnvironment webHostEnvironment)
        {
            _roleService = roleService;
            _userService = userService;
            _tokenGenerator = tokenGenerator;
            _configuration = configuration;
            _encryptionTool = encryptionTool;
            _webHostEnvironment = webHostEnvironment;

            var _issuer = _configuration["JWTSettings:Issuer"];
            var _secure = bool.Parse(_configuration["JWTSettings:Secure"]);
            var _accessTokenLifetime = TimeSpan.Parse(_configuration["JWTSettings:AccessTokenExpireTimespan"]);
            var _refreshTokenLifetime = TimeSpan.Parse(_configuration["JWTSettings:RefreshTokenExpireTimespan"]);
            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.Add(_accessTokenLifetime),
                MaxAge = _accessTokenLifetime,
                SameSite = SameSiteMode.None,
                HttpOnly = true,
                Secure = _secure,
                Domain = _issuer,
                IsEssential = true,
                Path = "/api",
            };
            _accessTokenCookieOptions = cookieOptions;
            cookieOptions.Expires = DateTimeOffset.UtcNow.Add(_refreshTokenLifetime);
            cookieOptions.MaxAge = _refreshTokenLifetime;
            _refreshTokenCookieOptions = cookieOptions;
        }

        // POST: api/login/{userName}  
        [HttpPost("login/{userName}")]
        [EnableRateLimiting("LoginRateLimit")]
        public async Task<ResultViewModel<TokenResultViewModel>> Login(string userName, [FromBody] string password)
        {
            var ip = HttpContext.GetIP();
            var userAgent = HttpContext.GetUserAgent();
            var userRoleId = await _roleService.Login(userName, password);

            var tokens = GenerateTokens(userRoleId.User, userRoleId.RoleId);
            userRoleId.User.IP = ip;
            userRoleId.User.UserAgent = userAgent;
            userRoleId.User.RefreshToken = tokens.RefreshToken;

            await _userService.UpdateUserAsync(userRoleId.User);
            await _userService.SaveChangesAsync();

            var needNewPassword = userRoleId.User.Password.IsNullOrEmpty();
            var result = new TokenResultViewModel();
            if (_webHostEnvironment.IsDevelopment())
            {
                result.AccessToken = tokens.AccessToken;
                result.RefreshToken = tokens.RefreshToken;
                result.NeedNewPassword = needNewPassword;
            }
            else
                result.NeedNewPassword = needNewPassword;

            return new ResultViewModel<TokenResultViewModel>(result);
        }

        // POST: api/generateToken
        [HttpGet("generateToken")]
        [EnableRateLimiting("LoginRateLimit")]
        public async Task<ResultViewModel<TokenResultViewModel>> GenerateToken()
        {
            var claims = await HttpContext.AuthorizeRefreshToken();
            var user = await _userService.GetUserByIdAsync(claims.UserId);

            if (user.RefreshToken != claims.Token)
                throw new CustomException("Authentication", "Login");

            var tokens = GenerateTokens(user, claims.RoleId);

            user.RefreshToken = null;
            await _userService.UpdateUserAsync(user);
            await _userService.SaveChangesAsync();

            var needNewPassword = user.Password.IsNullOrEmpty();
            var result = new TokenResultViewModel();
            if (_webHostEnvironment.IsDevelopment())
            {
                result.AccessToken = tokens.AccessToken;
                result.RefreshToken = tokens.RefreshToken;
                result.NeedNewPassword = needNewPassword;
            }
            else
                result.NeedNewPassword = needNewPassword;

            return new ResultViewModel<TokenResultViewModel>(result);
        }
        [HttpPost("logout")]
        [EnableRateLimiting("LoginRateLimit")]
        public async Task<ResultViewModel<object>> Logout()
        {
            var claims = await HttpContext.AuthorizeRefreshToken();

            if (string.IsNullOrEmpty(claims.Token) || claims.UserId == 0)
                throw new CustomException("Authentication", "Login", 401);

            var user = await _userService.GetUserByIdAsync(claims.UserId);
            if (user.RefreshToken != claims.Token)
                throw new CustomException("Authentication", "Login", claims.Token);

            if (user != null)
            {
                user.RefreshToken = null;

                await _userService.UpdateUserAsync(user);
                await _userService.SaveChangesAsync();
            }
            Response.Cookies.Delete("access_token");
            Response.Cookies.Delete("refresh_token");
            return new ResultViewModel<object>();
        }

        // POST: api/ChangePassword/{userName}  
        [HttpPost("changePassword/{userName}")]
        [CheckAccess]
        public async Task<ResultViewModel<ChangePasswordInputModel>> ChangePassword(string userName, [FromBody] ChangePasswordInputModel input)
        {
            var userIdRoleId = await _roleService.Login(userName, input.OldPassword);

            var salt = HashString.GetSalt();
            var hashPassword = HashString.HashPassword(input.NewPassword, salt);
            userIdRoleId.User.Password = hashPassword;
            userIdRoleId.User.Salt = salt;

            await _userService.UpdateUserAsync(userIdRoleId.User);
            await _userService.SaveChangesAsync();

            return new ResultViewModel<ChangePasswordInputModel>(input);
        }

        // POST: api/ChangePassword/{userName}  
        [HttpGet("user")]
        [CheckAccess]
        public async Task<ResultViewModel<UserDashboardViewModel>> GetUser()
        {
            var claims = await HttpContext.Authorize();
            var user = await _userService.GetUserByIdAsync(claims.UserId);
            var data = new UserDashboardViewModel
            {
                Id = user.Id,
                Name = user.Name
            };
            return new ResultViewModel<UserDashboardViewModel>(data);
        }
        private (string AccessToken, string RefreshToken) GenerateTokens(User User, int? RoleId)
        {
            string userId = User.Id.ToString();
            string? roleId = RoleId.ToString();
            var accessToken = _tokenGenerator.GenerateAccessToken(userId, roleId);
            var refreshToken = _tokenGenerator.GenerateRefreshToken(userId, roleId);

            SetCookies(accessToken, refreshToken);
            return (accessToken, refreshToken);
        }
        private void SetCookies(string accessToken, string refreshToken)
        {
            var encryptedAccessToken = _encryptionTool.EncryptCookie(accessToken);
            Response.Cookies.Append("access_token", encryptedAccessToken, _accessTokenCookieOptions);

            var encryptedRefreshToken = _encryptionTool.EncryptCookie(refreshToken);
            Response.Cookies.Append("refresh_token", encryptedRefreshToken, _refreshTokenCookieOptions);
        }
    }
}
