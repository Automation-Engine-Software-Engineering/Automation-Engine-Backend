using DataLayer.Models.WorkFlows;
using Microsoft.AspNetCore.Mvc;
using Services;
using ViewModels.ViewModels.WorkFlow;
using ViewModels;
using Tools.AuthoraizationTools;
using ViewModels.ViewModels.RoleDtos;
using Newtonsoft.Json.Linq;
using DataLayer.Models.MainEngine;
using Tools.TextTools;
using AutomationEngine.ControllerAttributes;
using DataLayer.Models.Enums;
using ViewModels.ViewModels.AuthenticationDtos;

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

        // GET: api/GetWorkFlowsByRole/{roleId}  
        [HttpGet("GetWorkFlowsByRole/{roleId}")]
        [CheckAccess]
        public async Task<ResultViewModel> GetWorkFlowsByRole(int roleId)
        {

            var result = await _roleService.GetWorkFlowsByRole(roleId);
            return (new ResultViewModel { Data = result, Message = "عملیات با موفقیت انجام شد.", Status = true, StatusCode = 200 });
        }

        // GET: api/GetWorkFlowsByRole/{roleId}  
        [HttpGet("GetWorkFlowUsersByRole/{userId}")]
        [CheckAccess]
        public async Task<ResultViewModel> GetWorkFlowUsersByRole(int userId)
        {
            var result = await _roleService.GetWorkFlowUsersByRole(userId);
            return (new ResultViewModel { Data = result, Message = "عملیات با موفقیت انجام شد.", Status = true, StatusCode = 200 });
        }

        // GET: api/GetRoleByuser/{userId}  
        [HttpGet("GetRoleByuser/{userId}")]
        [CheckAccess]
        public async Task<ResultViewModel> GetRoleByuser(int userId)
        {
            var result = await _roleService.GetRoleByUser(userId);
            return (new ResultViewModel { Data = result, Message = "عملیات با موفقیت انجام شد.", Status = true, StatusCode = 200 });
        }
    }
}
