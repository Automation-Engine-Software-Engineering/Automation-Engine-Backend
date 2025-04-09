using AutomationEngine.ControllerAttributes;
using DataLayer.Models.Enums;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using Microsoft.AspNetCore.Http;
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

        public AuthenticationController(IRoleService roleService, IUserService userService, TokenGenerator tokenGenerator, IConfiguration configuration,EncryptionTool encryptionTool)
        {
            _roleService = roleService;
            _userService = userService;
            _tokenGenerator = tokenGenerator;
            _configuration = configuration;
            _encryptionTool = encryptionTool;
        }

        // POST: api/Login/{userName}  
        [HttpPost("Login/{userName}")]

        public async Task<ResultViewModel> Login(string userName, [FromBody] string password)
        {
            var ip = HttpContext.GetIP();
            var userAgent = HttpContext.GetUserAgent();
            var userRoleId = await _roleService.Login(userName, password);

            string userId = userRoleId.User.Id.ToString();
            string? roleId = userRoleId.RoleId.ToString();
            var accessToken = _tokenGenerator.GenerateAccessToken(userId, roleId);
            var refreshToken = _tokenGenerator.GenerateRefreshToken(userId, roleId);

            userRoleId.User.IP = ip;
            userRoleId.User.UserAgent = userAgent;
            userRoleId.User.RefreshToken = refreshToken;
            await _userService.UpdateUser(userRoleId.User);
            await _userService.SaveChangesAsync();

            var issuer = _configuration["JWTSettings:Issuer"];
            var Secure = bool.Parse(_configuration["JWTSettings:Secure"]);
            var accessTokenLifetime = TimeSpan.Parse(_configuration["JWTSettings:AccessTokenExpireTimespan"]);
            var refreshTokenLifetime = TimeSpan.Parse(_configuration["JWTSettings:RefreshTokenExpireTimespan"]);


            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.Add(accessTokenLifetime),
                MaxAge = accessTokenLifetime,
                SameSite = SameSiteMode.None,
                HttpOnly = true,
                Secure = Secure,
                Domain = issuer,
                IsEssential = true,
                Path = "/api",
            };

            var encryptedAccessToken = _encryptionTool.EncryptCookie(accessToken);
            Response.Cookies.Append("access_token", encryptedAccessToken, cookieOptions);

            cookieOptions.Expires = DateTimeOffset.UtcNow.Add(refreshTokenLifetime);
            cookieOptions.MaxAge = refreshTokenLifetime;

            var encryptedRefreshToken = _encryptionTool.EncryptCookie(refreshToken);
            Response.Cookies.Append("refresh_token", encryptedRefreshToken, cookieOptions);

            var result = new TokenResultViewModel
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                NeedNewPassword = userRoleId.User.Password.IsNullOrEmpty(),
            };
            return (new ResultViewModel { Data = result, Message = new ValidationDto<TokenResultViewModel>(true, "Success", "Success", result).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // POST: api/RefreshToken
        [HttpPost("GenerateToken")]
        public async Task<ResultViewModel> GenerateToken()
        {
            var claims = await HttpContext.AuthorizeRefreshToken();
            var user = await _userService.GetUserById(claims.UserId);

            if (user.RefreshToken != claims.Token)
                throw new CustomException<string>(new ValidationDto<string>(false, "Authentication", "Login", claims.Token), 401);

            var newAccessToken = _tokenGenerator.GenerateAccessToken(claims.UserId.ToString(), claims.RoleId.ToString());

            return (new ResultViewModel { Data = newAccessToken, Message = new ValidationDto<string>(true, "Success", "Success", newAccessToken).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // POST: api/ChangePassword/{userName}  
        [HttpPost("ChangePassword/{userName}")]
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
        [HttpPost("User")]
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
            return (new ResultViewModel {Data = data, Message = "عملیات با موفقیت انجام شد.", Status = true, StatusCode = 200 });
        }
    }
}
