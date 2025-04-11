using AutomationEngine.ControllerAttributes;
using Azure.Core;
using Entities.Models.Enums;
using Entities.Models.MainEngine;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
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

        public async Task<ResultViewModel> Login(string userName, [FromBody] string password)
        {
            var ip = HttpContext.GetIP();
            var userAgent = HttpContext.GetUserAgent();
            var userRoleId = await _roleService.Login(userName, password);

            var tokens = GenerateTokens(userRoleId.User, userRoleId.RoleId);
            userRoleId.User.IP = ip;
            userRoleId.User.UserAgent = userAgent;
            userRoleId.User.RefreshToken = tokens.RefreshToken;

            await _userService.UpdateUser(userRoleId.User);
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

            return (new ResultViewModel { Data = result, Message = new ValidationDto<TokenResultViewModel>(true, "Success", "Success", result).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // POST: api/generateToken
        [HttpGet("generateToken")]
        public async Task<ResultViewModel> GenerateToken()
        {
            var claims = await HttpContext.AuthorizeRefreshToken();
            var user = await _userService.GetUserById(claims.UserId);

            if (user.RefreshToken != claims.Token)
                throw new CustomException<string>(new ValidationDto<string>(false, "Authentication", "Login", claims.Token), 401);

            var tokens = GenerateTokens(user, claims.RoleId);

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

            return (new ResultViewModel { Data = result, Message = new ValidationDto<string>(true, "Success", "Success", tokens.AccessToken).GetMessage(200), Status = true, StatusCode = 200 });
        }
        [HttpPost("logout")]
        public async Task<ResultViewModel> Logout()
        {
            var claims = await HttpContext.AuthorizeRefreshToken();

            if (string.IsNullOrEmpty(claims.Token) || claims.UserId == 0)
                throw new CustomException<string>(new ValidationDto<string>(false, "Authentication", "Login", claims.Token), 401);

            var user = await _userService.GetUserById(claims.UserId);
            if (user != null)
            {
                user.RefreshToken = null;

                await _userService.UpdateUser(user);
                await _userService.SaveChangesAsync();
            }
            Response.Cookies.Delete("access_token");
            Response.Cookies.Delete("refresh_token");
            return new ResultViewModel();
        }

        // POST: api/ChangePassword/{userName}  
        [HttpPost("changePassword/{userName}")]
        [CheckAccess]
        public async Task<ResultViewModel> ChangePassword(string userName, [FromBody] ChangePasswordInputModel input)
        {
            var userIdRoleId = await _roleService.Login(userName, input.OldPassword);

            var salt = HashString.GetSalt();
            var hashPassword = HashString.HashPassword(input.NewPassword, salt);
            userIdRoleId.User.Password = hashPassword;
            userIdRoleId.User.Salt = salt;

            await _userService.UpdateUser(userIdRoleId.User);
            await _userService.SaveChangesAsync();

            return (new ResultViewModel { Data = input, Message = new ValidationDto<ChangePasswordInputModel>(true, "Success", "Success", input).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // POST: api/ChangePassword/{userName}  
        [HttpGet("user")]
        [CheckAccess]
        public async Task<ResultViewModel> GetUser()
        {
            var claims = await HttpContext.Authorize();
            var user = await _userService.GetUserById(claims.UserId);
            var data = new UserDashboardViewModel
            {
                Id = user.Id,
                Name = user.Name
            };
            return (new ResultViewModel { Data = data, Message = "عملیات با موفقیت انجام شد.", Status = true, StatusCode = 200 });
        }
        private (string AccessToken, string RefreshToken) GenerateTokens(User User, int? RoleId)
        {
            string userId = User.Id.ToString();
            string? roleId = RoleId.ToString();
            var accessToken = _tokenGenerator.GenerateAccessToken(userId, roleId);
            var refreshToken = _tokenGenerator.GenerateRefreshToken(userId, roleId);

            SetCookies(accessToken,refreshToken);
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
