using Entities.Models.Workflows;
using Microsoft.AspNetCore.Mvc;
using Services;
using ViewModels.ViewModels.Workflow;
using ViewModels;
using Tools.AuthoraizationTools;
using ViewModels.ViewModels.RoleDtos;
using Newtonsoft.Json.Linq;
using Entities.Models.MainEngine;
using Tools.TextTools;
using AutomationEngine.ControllerAttributes;
using Entities.Models.Enums;
using ViewModels.ViewModels.AuthenticationDtos;
using FrameWork.Model.DTO;
using FrameWork.ExeptionHandler.ExeptionModel;

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

        // POST: api/form/create  
        [HttpPost("create")]
        public async Task<ResultViewModel<Role?>> CreateWorkflow([FromBody] RoleDto role)
        {
            if (role == null)
                throw new CustomException("Workflow", "CorruptedWorkflow");

            //transfer model
            var result = new Role();
            result.Name = role.Name;
            result.Description = role.Description;

            //is validation model
            if (result.Id != 0)
                throw new CustomException("Role", "InvalidRole", result);

            if (!(_roleService.RoleValidation(result)).IsSuccess)
                throw new CustomException("Role", "InvalidRole", result);

            await _roleService.InsertRoleAsync(result);
            await _roleService.SaveChangesAsync();

            return new ResultViewModel<Role?>(result);
        }

        // POST: api/form/update  
        [HttpPost("update")]
        public async Task<ResultViewModel<Role?>> UpdateWorkflowUser([FromBody] RoleDto role)
        {
            if (role == null)
                throw new CustomException("Role", "InvalidRole");

            var workflow = await _roleService.GetRoleByIdAsync(role.Id);

            //transfer model
            var result = new Role();
            result.Id = workflow.Id;
            result.Name = role.Name;
            result.Description = role.Description;

            //is validation model
            if (role.Id == 0)
                throw new CustomException("Role", "InvalidRole", result);

            var validationModel = _roleService.RoleValidation(result);
            if (!validationModel.IsSuccess) throw validationModel;


            await _roleService.UpdateRoleAsync(result);
            await _roleService.SaveChangesAsync();
            return new ResultViewModel<Role?>(result);
        }

        // POST: api/form/delete  
        [HttpPost("remove")]
        public async Task<ResultViewModel<Role?>> RemoveWorkflowUser([FromBody] int roleId)
        {
            //is validation model
            if (roleId == 0)
                throw new CustomException("Role", "InvalidRole", roleId);

            var fetchForm = await _roleService.GetRoleByIdAsync(roleId);
            if (fetchForm == null)
                throw new CustomException("Role", "InvalidRole", fetchForm);

            var validationModel = _roleService.RoleValidation(fetchForm);
            if (!validationModel.IsSuccess) throw validationModel;

            //initial action
            await _roleService.DeleteRoleAsync(roleId);
            await _roleService.SaveChangesAsync();

            return new ResultViewModel<Role?>(fetchForm);
        }

        // GET: api/form/all  
        [HttpGet("all")]
        public async Task<ResultViewModel<IEnumerable<Role?>>> GetAllWorkflowUser(int pageSize, int pageNumber)
        {
            if (pageSize > 100)
                pageSize = 100;
            if (pageNumber < 1)
                pageNumber = 1;

            var forms = await _roleService.GetAllRolesAsync(pageSize, pageNumber);

            //is valid data
            if ((((pageSize * pageNumber) - forms.TotalCount) > pageSize) && (pageSize * pageNumber) > forms.TotalCount)
                throw new CustomException("Role", "InvalidRole", forms);

            return new ResultViewModel<IEnumerable<Role?>> { Data = forms.Data, ListNumber = forms.ListNumber, ListSize = forms.ListSize, TotalCount = forms.TotalCount };
        }
    }
}
