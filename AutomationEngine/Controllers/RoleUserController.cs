using Entities.Models.FormBuilder;
using Microsoft.AspNetCore.Mvc;
using ViewModels.ViewModels.FormBuilder;
using ViewModels;
using Entities.Models.Workflows;
using Services;
using ViewModels.ViewModels.Workflow;
using AutomationEngine.ControllerAttributes;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using Microsoft.IdentityModel.Tokens;
using Tools.TextTools;
using Entities.Models.MainEngine;
using Tools.AuthoraizationTools;
using Entities.Models.Enums;
using System.Numerics;

namespace AutomationEngine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [CheckAccess]
    public class RoleUserController : Controller
    {
        private readonly IRoleUserService _roleUserService;
        private readonly TokenGenerator _tokenGenerator;
        private readonly IRoleService _roleService;
        public RoleUserController(IRoleUserService roleUserService, TokenGenerator tokenGenerator, IRoleService roleService)
        {
            _roleUserService = roleUserService;
            _tokenGenerator = tokenGenerator;
            _roleService = roleService;
        }

        // POST: api/form/create  
        [HttpPost("create")]
        public async Task<ResultViewModel<Role_User?>> CreateRoleUser([FromBody] RoleUserDto roleUser)
        {
            if (roleUser == null)
                throw new CustomException("RoleUser", "CorruptedRoleUser");

            var workflow = await _roleUserService.GetRoleUserByIdAsync(roleUser.Id);

            var result = new Role_User()
            {
                UserId = roleUser.UserId,
                RoleId = roleUser.RoleId
            };

            //is validation model
            if (roleUser.Id != 0)
                throw new CustomException("RoleUser", "CorruptedRoleUser", result);

            var validationModel = _roleUserService.RoleUserValidation(result);
            if (!validationModel.IsSuccess)
                throw validationModel;


            await _roleUserService.InsertRoleUserAsync(result);
            await _roleUserService.SaveChangesAsync();
            return new ResultViewModel<Role_User?>(result);
        }

        // POST: api/form/update  
        [HttpPost("update")]
        public async Task<ResultViewModel<Role_User?>> UpdateRoleUser([FromBody] Role_User roleUser)
        {
            if (roleUser == null)
                throw new CustomException("RoleUser", "CorruptedRoleUser");

            var workflow = await _roleUserService.GetRoleUserByIdAsync(roleUser.Id);

            var result = new Role_User()
            {
                UserId = roleUser.UserId,
                RoleId = roleUser.RoleId
            };

            //is validation model
            if (roleUser.Id == 0)
                throw new CustomException("RoleUser", "CorruptedRoleUser", result);

            var validationModel = _roleUserService.RoleUserValidation(result);
            if (!validationModel.IsSuccess)
                throw validationModel;

            await _roleUserService.UpdateRoleUserAsync(result);
            await _roleUserService.SaveChangesAsync();
            return new ResultViewModel<Role_User?>(result);
        }

        // POST: api/form/delete  
        [HttpPost("remove")]
        public async Task<ResultViewModel<Role_User?>> RemoveRoleUser([FromBody] int roleUserId)
        {
            //is validation model
            if (roleUserId == 0)
                throw new CustomException("WorkflowRole", "CorruptedWorkflowRole", roleUserId);

            var fetchForm = await _roleUserService.GetRoleUserByIdAsync(roleUserId);
            if (fetchForm == null)
                throw new CustomException("WorkflowRole", "CorruptedWorkflowRole", fetchForm);

            var validationModel = _roleUserService.RoleUserValidation(fetchForm);
            if (!validationModel.IsSuccess)
                throw validationModel;

            //initial action
            await _roleUserService.DeleteRoleUserAsync(roleUserId);
            await _roleUserService.SaveChangesAsync();

            return new ResultViewModel<Role_User?>(fetchForm);
        }

        // GET: api/form/all  
        [HttpGet("all")]
        public async Task<ResultViewModel<IEnumerable<Role_User?>>> GetAllRoleUser(int pageSize, int pageNumber)
        {
            if (pageSize > 100)
                pageSize = 100;
            if (pageNumber < 1)
                pageNumber = 1;

            var forms = await _roleUserService.GetAllRoleUsersAsync(pageSize, pageNumber);

            //is valid data
            if ((((pageSize * pageNumber) - forms.TotalCount) > pageSize) && (pageSize * pageNumber) > forms.TotalCount)
                throw new CustomException("Form", "CorruptedInvalidPage", forms);

            return new ResultViewModel<IEnumerable<Role_User?>> { Data = forms.Data, ListNumber = forms.ListNumber, ListSize = forms.ListSize, TotalCount = forms.TotalCount };
        }

        // GET: api/form/{id}  
        [HttpGet("{roleUserId}")]
        public async Task<ResultViewModel<Role_User?>> GetRoleUser(int roleUserId)
        {                        //is validation model
            if (roleUserId == 0)
                throw new CustomException("UserWorkflow", "CorruptedUserWorkflow", roleUserId);

            //initial action
            var RoleUser = await _roleUserService.GetRoleUserByIdAsync(roleUserId);
            if (RoleUser == null)
                throw new CustomException("UserWorkflow", "CorruptedUserWorkflow", RoleUser);

            var validationModel = _roleUserService.RoleUserValidation(RoleUser);
            if (!validationModel.IsSuccess)
                throw validationModel;

            var form = await _roleUserService.GetRoleUserByIdAsync(roleUserId);
            return new ResultViewModel<Role_User?>(form);
        }

        // GET: api/form/{id}  
        [HttpGet("roleUserById")]
        public async Task<ResultViewModel<IEnumerable<Role_User?>>> GetRoleUserBuUserId(int pageSize, int pageNumber)
        {
            if (pageSize > 100)
                pageSize = 100;
            if (pageNumber < 1)
                pageNumber = 1;

            var claims = await HttpContext.Authorize();
           
            if (claims.UserId == 0)
                throw new CustomException("UserWorkflow", "CorruptedUserWorkflow", claims.UserId);

            //initial action
            var RoleUser = await _roleUserService.GetRoleUserByUserIdAsync(claims.UserId, pageSize, pageNumber);
            if (RoleUser == null)
                throw new CustomException("UserWorkflow", "CorruptedUserWorkflow", RoleUser);

            //is valid data
            if ((((pageSize * pageNumber) - RoleUser.TotalCount) > pageSize) && (pageSize * pageNumber) > RoleUser.TotalCount)
                throw new CustomException("Form", "CorruptedInvalidPage", RoleUser);

            return new ResultViewModel<IEnumerable<Role_User?>> { Data = RoleUser.Data, ListNumber = RoleUser.ListNumber, ListSize = RoleUser.ListSize, TotalCount = RoleUser.TotalCount };
        }

            // GET: api/form/all  
        [HttpGet("user")]
        public async Task<ResultViewModel<IEnumerable<IsAccessModel>?>> GetAllRoleUserAndUser(int roleId, int pageSize, int pageNumber)
        {
            if (pageSize > 100)
                pageSize = 100;
            if (pageNumber < 1)
                pageNumber = 1;

            ListDto<IsAccessModel> forms = await _roleService.GetAllUserForRoleAccessAsync(roleId, pageSize, pageNumber);

            //is valid data
            if ((((pageSize * pageNumber) - forms.TotalCount) > pageSize) && (pageSize * pageNumber) > forms.TotalCount)
                throw new CustomException("Form", "CorruptedInvalidPage", forms);

            return new ResultViewModel<IEnumerable<IsAccessModel>?> { Data = forms.Data, ListNumber = forms.ListNumber, ListSize = forms.ListSize, TotalCount = forms.TotalCount };
        }
    }
}
