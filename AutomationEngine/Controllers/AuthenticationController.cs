using AutomationEngine.ControllerAttributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services;
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

        public AuthenticationController(IRoleService roleService, IUserService userService, TokenGenerator tokenGenerator)
        {
            _roleService = roleService;
            _userService = userService;
            _tokenGenerator = tokenGenerator;
        }

        // POST: api/Login/{userName}  
        [HttpPost("Login/{userName}")]

        public async Task<ResultViewModel> Login(string userName, [FromBody] string password)
        {
            var ip = HttpContext.GetIP();
            var userAgent = HttpContext.GetUserAgent();
            var userRoleId = await _roleService.Login(userName, password);
            userRoleId.User.IP = ip;
            userRoleId.User.UserAgent = userAgent;
            await _userService.UpdateUser(userRoleId.User);
            await _userService.SaveChangesAsync();

            var accessToken = _tokenGenerator.GenerateAccessToken(userRoleId.User.Id.ToString(), userRoleId.RoleId.ToString());
            var refreshToken = _tokenGenerator.GenerateRefreshToken();

            var result = new TokenResultViewModel
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                NeedNewPassword = userRoleId.User.Password.IsNullOrEmpty(),
            };
            return (new ResultViewModel { Data = result, Message = "عملیات با موفقیت انجام شد.", Status = true, StatusCode = 200 });
        }


        // POST: api/RefreshToken
        [HttpPost("RefreshToken")]
        public async Task<ResultViewModel> GenerateRefreshToken(string userName, [FromBody] ChangePasswordInputModel input)
        {
            //TODO : Add refreshToken Generator
            return (new ResultViewModel { Message = "عملیات با موفقیت انجام شد.", Status = true, StatusCode = 200 });
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

            return (new ResultViewModel { Message = "عملیات با موفقیت انجام شد.", Status = true, StatusCode = 200 });
        }
    }
}
