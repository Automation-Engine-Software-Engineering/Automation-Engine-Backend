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
            var cookieOptions = new CookieOptions
            {
                SameSite = SameSiteMode.None,
                HttpOnly = false,
                Secure = false,
                IsEssential = false,
                //Path = "/api",
                Expires = DateTimeOffset.UtcNow.AddMinutes(15),
                MaxAge = TimeSpan.FromMinutes(15),
                //SameSite = SameSiteMode.Strict,
                //HttpOnly = true,
                //Secure = true,
                //Domain = issuer,
                //IsEssential = true,
                //Path = "/api",
            };

            var encryptedAccessToken = _encryptionTool.EncryptCookie(accessToken);
            Response.Cookies.Append("access_token", encryptedAccessToken, cookieOptions);

            cookieOptions.Expires = DateTimeOffset.UtcNow.AddMonths(1);
            cookieOptions.MaxAge = TimeSpan.FromDays(30);

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
        //TODO : کلیم ها رو از ولیدیشن اش بگیرم و سر کانتکست بزارمش و اینام ها رو تغییر بدم به کلاس برای کلیم ها 
        //محدود کردن تعداد لاگین‌های ناموفق: تنظیم Secure Flag برای تمام کوکی‌ها:
        //مطمئن شوید که تمام کوکی‌های سایت فقط از طریق HTTPS منتقل می‌شوند.
        //استفاده از Anti-CSRF Tokens:
        //در فرم‌ها از توکن‌های ضد CSRF برای جلوگیری از حملات Cross-Site استفاده کنید.

        // POST: api/RefreshToken
        [HttpPost("GenerateToken")]
        public async Task<ResultViewModel> GenerateToken()
        {
            var encryptedToken = HttpContext.Request.Cookies["refresh_token"];
            var refreshToken = _encryptionTool.DecryptCookie(encryptedToken);

            _tokenGenerator.ValidateToken(refreshToken, true);
            var userId = _tokenGenerator.GetClaimFromToken(refreshToken, ClaimsEnum.UserId.ToString());
            var role = _tokenGenerator.GetClaimFromToken(refreshToken, ClaimsEnum.RoleId.ToString());
            var user = await _userService.GetUserById(int.Parse(userId ?? "0"));

            if (user.RefreshToken != refreshToken)
                throw new CustomException<string>(new ValidationDto<string>(false, "Authentication", "Login", refreshToken), 401);

            var newAccessToken = _tokenGenerator.GenerateAccessToken(userId, role);

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
            var token = HttpContext.Request.Headers["Authorization"].ToString();
            var userId = _tokenGenerator.GetClaimFromToken(token, ClaimsEnum.UserId.ToString());
            var user = await _userService.GetUserById(int.Parse(userId ?? "0"));
            var data = new UserDashboardViewModel
            {
                Id = user.Id,
                Name = user.Name
            };
            return (new ResultViewModel {Data = data, Message = "عملیات با موفقیت انجام شد.", Status = true, StatusCode = 200 });
        }
    }
}
