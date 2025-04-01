using AutomationEngine.ControllerAttributes;
using DataLayer.Models.Enums;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Services;
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

            string userId = userRoleId.User.Id.ToString();
            string? roleId = userRoleId.RoleId.ToString();
            var accessToken = _tokenGenerator.GenerateAccessToken(userId, roleId);
            var refreshToken = _tokenGenerator.GenerateRefreshToken(userId, roleId);

            userRoleId.User.IP = ip;
            userRoleId.User.UserAgent = userAgent;
            userRoleId.User.RefreshToken = refreshToken;
            await _userService.UpdateUser(userRoleId.User);
            await _userService.SaveChangesAsync();


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
        public async Task<ResultViewModel> GenerateToken([FromBody] string refreshToken)
        {
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
    }
}
