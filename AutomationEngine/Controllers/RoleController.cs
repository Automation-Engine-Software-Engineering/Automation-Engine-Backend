using DataLayer.Models.WorkFlows;
using Microsoft.AspNetCore.Mvc;
using Services;
using ViewModels.ViewModels.WorkFlow;
using ViewModels;
using Tools.AuthoraizationTools;
using ViewModels.ViewModels.RoleDtos;
using Newtonsoft.Json.Linq;

namespace AutomationEngine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : Controller
    {
        private readonly IRoleService _roleService;
        private readonly IUserService _userService;
        private readonly TokenGenerator _tokenGenerator;

        public RoleController(IRoleService roleService, IUserService userService, TokenGenerator tokenGenerator)
        {
            _roleService = roleService;
            _userService = userService;
            _tokenGenerator = tokenGenerator;
        }

        // POST: api/login/{userName}  
        [HttpPost("login/{userName}")]
        public async Task<ResultViewModel> Login(string userName, [FromBody] string password)
        {
            var ip = HttpContext.GetIP();
            var userAgent = HttpContext.GetUserAgent();
            var userIdRoleId = await _roleService.Login(userName, password);
            userIdRoleId.User.IP = ip;
            userIdRoleId.User.UserAgent = userAgent;
            await _userService.UpdateUser(userIdRoleId.User);
            await _userService.SaveChangesAsync();

            var accessToken = _tokenGenerator.GenerateAccessToken(userIdRoleId.User.Id.ToString(), userIdRoleId.RoleId.ToString());
            var refreshToken = _tokenGenerator.GenerateRefreshToken();

            var result = new TokenResultViewModel
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
            return (new ResultViewModel { Data = result, Message = "عملیات با موفقیت انجام شد.", Status = true, StatusCode = 200 });
        }

        // GET: api/GetWorkFlowsByRole/{roleId}  
        [HttpGet("GetWorkFlowsByRole/{roleId}")]
        public async Task<ResultViewModel> GetWorkFlowsByRole(int roleId)
        {

            var result = await _roleService.GetWorkFlowsByRole(roleId);
            return (new ResultViewModel { Data = result, Message = "عملیات با موفقیت انجام شد.", Status = true, StatusCode = 200 });
        }

        // GET: api/GetWorkFlowsByRole/{roleId}  
        [HttpGet("GetWorkFlowUsersByRole/{userId}")]
        public async Task<ResultViewModel> GetWorkFlowUsersByRole(int userId)
        {
            var result = await _roleService.GetWorkFlowUsersByRole(userId);
            return (new ResultViewModel { Data = result, Message = "عملیات با موفقیت انجام شد.", Status = true, StatusCode = 200 });
        }

        // GET: api/GetRoleByuser/{userId}  
        [HttpGet("GetRoleByuser/{userId}")]
        public async Task<ResultViewModel> GetRoleByuser(int userId)
        {
            var result = await _roleService.GetRoleByUser(userId);
            return (new ResultViewModel { Data = result, Message = "عملیات با موفقیت انجام شد.", Status = true, StatusCode = 200 });
        }
    }
}
