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
using System;
using System.Net.Http;
using System.Security.Claims;
using Tools.AuthoraizationTools;
using Tools.LoggingTools;
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
        private readonly Logging _logger;

        //private readonly string _issuer;
        //private readonly bool _secure;
        //private readonly TimeSpan _accessTokenLifetime;
        //private readonly TimeSpan _refreshTokenLifetime;
        private readonly CookieOptions _accessTokenCookieOptions;
        private readonly CookieOptions _refreshTokenCookieOptions;

        public AuthenticationController(IRoleService roleService,
            IUserService userService,
            TokenGenerator tokenGenerator,
            IConfiguration configuration,
            EncryptionTool encryptionTool,
            IWebHostEnvironment webHostEnvironment,
            Logging logger)
        {
            _roleService = roleService;
            _userService = userService;
            _tokenGenerator = tokenGenerator;
            _configuration = configuration;
            _encryptionTool = encryptionTool;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
            var _issuer = _configuration["JWTSettings:Issuer"];
            var _secure = bool.Parse(_configuration["JWTSettings:Secure"]);
            var _accessTokenLifetime = TimeSpan.Parse(_configuration["JWTSettings:AccessTokenExpireTimespan"]);
            var _refreshTokenLifetime = TimeSpan.Parse(_configuration["JWTSettings:RefreshTokenExpireTimespan"]);
            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.Add(_accessTokenLifetime),
                MaxAge = _accessTokenLifetime,
                SameSite = SameSiteMode.None,
                HttpOnly = false,
                Secure = _secure,
                Domain = _issuer,
                IsEssential = false,
                Path = "/api",
            };
            _accessTokenCookieOptions = cookieOptions;
            cookieOptions.Expires = DateTimeOffset.UtcNow.Add(_refreshTokenLifetime);
            cookieOptions.MaxAge = _refreshTokenLifetime;
            _refreshTokenCookieOptions = cookieOptions;
        }

        // POST: api/login/{username}  
        [HttpPost("login/{username}")]
        [EnableRateLimiting("LoginRateLimit")]
        public async Task<ResultViewModel<TokenResultViewModel>> Login(string username, [FromBody] string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                throw new CustomException("Authentication", "FieldIsEmpty");

            var ip = HttpContext.GetIP();
            var userAgent = HttpContext.GetUserAgent();
            var user = await CheckUserPassAsync(username, password);

            _logger.LogUserLoginSuccess(user.Id, username, ip, userAgent);

            var role = await _roleService.GetRoleByUserAsync(user.Id);

            var tokens = GenerateTokens(user, role?.Id);
            user.IP = ip;
            user.UserAgent = userAgent;
            user.RefreshToken = tokens.RefreshToken;

            await _userService.UpdateUserAsync(user);
            await _userService.SaveChangesAsync();

            var needNewPassword = user.Password.IsNullOrEmpty();
            var result = new TokenResultViewModel();
            //if (_webHostEnvironment.IsDevelopment())
            //{
            result.AccessToken = tokens.AccessToken;
            result.RefreshToken = tokens.RefreshToken;
            //}

            return new ResultViewModel<TokenResultViewModel>(result);
        }
        private async Task<User> CheckUserPassAsync(string username, string password)
        {
            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null)
            {
                var exception = new CustomException("Authentication", "LoginFailed");
                throw exception;
            }

            if (user.Password.IsNullOrEmpty())
            {
                if (username != user.UserName)
                    throw new CustomException("Authentication", "LoginFailed");
            }
            else
            {
                var hashPassword = HashString.HashPassword(password, user.Salt);
                if (hashPassword != user.Password)
                    throw new CustomException("Authentication", "LoginFailed");
            }
            
            return user;
        }
        // POST: api/generateToken
        [HttpGet("generateToken")]
        [EnableRateLimiting("LoginRateLimit")]
        public async Task<ResultViewModel<TokenResultViewModel>> GenerateToken()
        {
            var claims = await HttpContext.AuthorizeRefreshToken();
            var user = await _userService.GetUserByIdAsync(claims.UserId);

            if(user == null)
                throw new CustomException("Authentication", "NotAuthorized");

            if (user.RefreshToken != claims.Token)
                throw new CustomException("Authentication", "NotAuthorized");

            var tokens = GenerateTokens(user, claims.RoleId);

            user.RefreshToken = null;
            await _userService.UpdateUserAsync(user);
            await _userService.SaveChangesAsync();

            var needNewPassword = user.Password.IsNullOrEmpty();
            var result = new TokenResultViewModel();
            //if (_webHostEnvironment.IsDevelopment())
            //{
            result.AccessToken = tokens.AccessToken;
            result.RefreshToken = tokens.RefreshToken;
            //}

            return new ResultViewModel<TokenResultViewModel>(result);
        }
        [HttpPost("logout")]
        [EnableRateLimiting("LoginRateLimit")]
        public async Task<ResultViewModel<object>> Logout()
        {
            var claims = await HttpContext.AuthorizeRefreshToken();
            var ip = HttpContext.GetIP();
            var userAgent = HttpContext.GetUserAgent();

            if (string.IsNullOrEmpty(claims.Token) || claims.UserId == 0)
                throw new CustomException("Authentication", "LoginFailed", 401);


            var user = await _userService.GetUserByIdAsync(claims.UserId);
            if (user.RefreshToken != claims.Token)
                throw new CustomException("Authentication", "LoginFailed", claims.Token);

            if (user != null)
            {
                user.RefreshToken = null;

                await _userService.UpdateUserAsync(user);
                await _userService.SaveChangesAsync();
            }

            _logger.LogUserLogout(claims.UserId, user.UserName,ip, userAgent);

            Response.Cookies.Delete("access_token");
            Response.Cookies.Delete("refresh_token");
            return new ResultViewModel<object>();
        }

        // POST: api/ChangePassword/{username}  
        [HttpPost("changePassword/{username}")]
        [CheckAccess]
        public async Task<ResultViewModel<ChangePasswordInputModel>> ChangePassword(string username, [FromBody] ChangePasswordInputModel input)
        {
            var user = await CheckUserPassAsync(username, input.OldPassword);
            
            var ip = HttpContext.GetIP();
            var userAgent = HttpContext.GetUserAgent();

            _logger.LogUserChangePassword(user.Id, username, ip, userAgent);

            var salt = HashString.GetSalt();
            var hashPassword = HashString.HashPassword(input.NewPassword, salt);
            user.Password = hashPassword;
            user.Salt = salt;

            await _userService.UpdateUserAsync(user);
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
            if(user == null)
                throw new CustomException("Authentication", "NotAuthorized");

            var data = new UserDashboardViewModel
            {
                Id = user.Id,
                Name = user.Name,
                NeedNewPassword = false
                //NeedNewPassword = user.Password.IsNullOrEmpty()
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
