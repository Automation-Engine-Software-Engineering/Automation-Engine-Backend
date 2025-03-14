using DataLayer.Models.WorkFlow;
using Microsoft.AspNetCore.Mvc;
using Services;
using ViewModels.ViewModels.WorkFlow;
using ViewModels;

namespace AutomationEngine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : Controller
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        // POST: api/login/{userName}  
        [HttpPost("login/{userName}")]
        public async Task<ResultViewModel> Login(string userName, [FromBody] string password)
        {
            var result =await _roleService.login(userName, password);
            return (new ResultViewModel { Data = result.UserId , Message = "عملیات با موفقیت انجام شد.", Status = true, StatusCode = 200 });
        }

        // GET: api/GetWorkFlowsByRole/{roleId}  
        [HttpGet("GetWorkFlowsByRole/{roleId}")]
        public async Task<ResultViewModel> GetWorkFlowsByRole(int roleId)
        {
            var result =await _roleService.GetWorkFlowsByRole(roleId);
            return (new ResultViewModel { Data = result, Message = "عملیات با موفقیت انجام شد.", Status = true, StatusCode = 200 });
        }

        // GET: api/GetWorkFlowsByRole/{roleId}  
        [HttpGet("GetWorkFlowUsersByRole/{userId}")]
        public async Task<ResultViewModel> GetWorkFlowUsersByRole(int userId)
        {
            var result =await _roleService.GetWorkFlowUsersByRole(userId);
            return (new ResultViewModel { Data = result, Message = "عملیات با موفقیت انجام شد.", Status = true, StatusCode = 200 });
        }

        // GET: api/GetRoleByuser/{userId}  
        [HttpGet("GetRoleByuser/{userId}")]
        public async Task<ResultViewModel> GetRoleByuser(int userId)
        {
            var result =await _roleService.GetRoleByUser(userId);
            return (new ResultViewModel { Data = result, Message = "عملیات با موفقیت انجام شد.", Status = true, StatusCode = 200 });
        }
    }
}
