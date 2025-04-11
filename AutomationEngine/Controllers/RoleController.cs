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
        public async Task<ResultViewModel> CreateWorkflow([FromBody] RoleDto role)
        {
            if (role == null)
                throw new CustomException<Workflow>(new ValidationDto<Workflow>(false, "Workflow", "CorruptedWorkflow", null), 500);

            //transfer model
            var result = new Role();
            result.Name = role.Name;
            result.Description = role.Description;

            //is validation model
            if (result.Id != 0)
                throw new CustomException<Role>(new ValidationDto<Role>(false, "Role", "InvalidRole", result), 500);

            if (!(await _roleService.RoleValidationAsync(result)).IsSuccess)
                throw new CustomException<Role>(new ValidationDto<Role>(false, "Role", "InvalidRole", result), 500);

            await _roleService.InsertRoleAsync(result);
            var saveResult = await _roleService.SaveChangesAsync();
            if (!saveResult.IsSuccess)
                throw new CustomException<string>(saveResult, 500);

            return (new ResultViewModel { Data = result, Message = new ValidationDto<Role>(true, "Success", "Success", result).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // POST: api/form/update  
        [HttpPost("update")]
        public async Task<ResultViewModel> UpdateWorkflowUser([FromBody] RoleDto role)
        {
            if (role == null)
                throw new CustomException<Role_User>(new ValidationDto<Role_User>(false, "Role", "InvalidRole", null), 500);

            var workflow = await _roleService.GetRoleByIdAsync(role.Id);

            //transfer model
            var result = new Role();
            result.Id = workflow.Id;
            result.Name = role.Name;
            result.Description = role.Description;

            //is validation model
            if (role.Id == 0)
                throw new CustomException<Role>(new ValidationDto<Role>(false, "Role", "InvalidRole", result), 500);

            var validationModel = await _roleService.RoleValidationAsync(result);
            if (!validationModel.IsSuccess)
                throw new CustomException<Role>(validationModel, 500);


            await _roleService.UpdateRoleAsync(result);
            await _roleService.SaveChangesAsync();
            return (new ResultViewModel { Data = result, Message = new ValidationDto<Role>(true, "Success", "Success", result).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // POST: api/form/delete  
        [HttpPost("remove")]
        public async Task<ResultViewModel> RemoveWorkflowUser([FromBody] int roleId)
        {
            //is validation model
            if (roleId == 0)
                throw new CustomException<int>(new ValidationDto<int>(false, "Role", "InvalidRole", roleId), 500);

            var fetchForm = await _roleService.GetRoleByIdAsync(roleId);
            if (fetchForm == null)
                throw new CustomException<Role>(new ValidationDto<Role>(false, "Role", "InvalidRole", fetchForm), 500);

            var validationModel = await _roleService.RoleValidationAsync(fetchForm);
            if (!validationModel.IsSuccess)
                throw new CustomException<Role>(validationModel, 500);

            //initial action
            await _roleService.DeleteRoleAsync(roleId);
            var saveResult = await _roleService.SaveChangesAsync();

            if (!saveResult.IsSuccess)
                throw new CustomException<string>(saveResult, 500);

            return (new ResultViewModel { Data = fetchForm, Message = new ValidationDto<Role>(true, "Success", "Success", fetchForm).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // GET: api/form/all  
        [HttpGet("all")]
        public async Task<ResultViewModel> GetAllWorkflowUser(int pageSize, int pageNumber)
        {
            if (pageSize > 100)
                pageSize = 100;
            if (pageNumber < 1)
                pageNumber = 1;

            var forms = await _roleService.GetAllRolesAsync(pageSize, pageNumber);

            //is valid data
            if ((((pageSize * pageNumber) - forms.TotalCount) > pageSize) && (pageSize * pageNumber) > forms.TotalCount)
                throw new CustomException<ListDto<Role>>(new ValidationDto<ListDto<Role>>(false, "Role", "InvalidRole", forms), 500);

            return (new ResultViewModel { Data = forms.Data, ListNumber = forms.ListNumber, ListSize = forms.ListSize, TotalCount = forms.TotalCount, Message = new ValidationDto<ListDto<Role>>(true, "Success", "Success", forms).GetMessage(200), Status = true, StatusCode = 200 });
        }
    }
}
