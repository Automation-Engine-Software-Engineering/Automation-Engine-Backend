using DataLayer.Models.FormBuilder;
using Microsoft.AspNetCore.Mvc;
using ViewModels.ViewModels.FormBuilder;
using ViewModels;
using DataLayer.Models.WorkFlows;
using Services;
using ViewModels.ViewModels.WorkFlow;
using AutomationEngine.ControllerAttributes;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using Microsoft.IdentityModel.Tokens;
using Tools.TextTools;
using DataLayer.Models.MainEngine;
using Tools.AuthoraizationTools;
using DataLayer.Models.Enums;

namespace AutomationEngine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [CheckAccess]
    public class RoleUserController : Controller
    {
        private readonly IRoleUserService _RoleUserService;
        private readonly TokenGenerator _tokenGenerator;
        private readonly IRoleService _roleService;
        public RoleUserController(IRoleUserService RoleUserService, TokenGenerator tokenGenerator, IRoleService roleService)
        {
            RoleUserService = _RoleUserService;
            _tokenGenerator = tokenGenerator;
            _roleService = roleService;
        }

        // POST: api/form/create  
        [HttpPost("create")]
        public async Task<ResultViewModel> CreateRoleUser([FromBody] RoleUserDto roleUser)
        {
            if (roleUser == null)
                throw new CustomException<Role_User>(new ValidationDto<Role_User>(false, "RoleUser", "CorruptedRoleUser", null), 500);

            var workFlow = await _RoleUserService.GetRoleUserById(roleUser.Id);

            var result = new Role_User()
            {
                UserId = roleUser.UserId,
                RoleId = roleUser.RoleId
            };

            //is validation model
            if (roleUser.Id != 0)
                throw new CustomException<Role_User>(new ValidationDto<Role_User>(false, "RoleUser", "CorruptedRoleUser", result), 500);

            var validationModel = await _RoleUserService.RoleUserValidation(result);
            if (!validationModel.IsSuccess)
                throw new CustomException<Role_User>(validationModel, 500);


            await _RoleUserService.InsertRoleUser(result);
            await _RoleUserService.SaveChangesAsync();
            return (new ResultViewModel { Data = result, Message = new ValidationDto<Role_User>(true, "Success", "Success", result).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // POST: api/form/update  
        [HttpPost("update")]
        public async Task<ResultViewModel> UpdateRoleUser([FromBody] Role_User roleUser)
        {
            if (roleUser == null)
                throw new CustomException<Role_User>(new ValidationDto<Role_User>(false, "RoleUser", "CorruptedRoleUser", null), 500);

            var workFlow = await _RoleUserService.GetRoleUserById(roleUser.Id);

            var result = new Role_User()
            {
                UserId = roleUser.UserId,
                RoleId = roleUser.RoleId
            };

            //is validation model
            if (roleUser.Id == 0)
                throw new CustomException<Role_User>(new ValidationDto<Role_User>(false, "RoleUser", "CorruptedRoleUser", result), 500);

            var validationModel = await _RoleUserService.RoleUserValidation(result);
            if (!validationModel.IsSuccess)
                throw new CustomException<Role_User>(validationModel, 500);


            await _RoleUserService.UpdateRoleUser(result);
            await _RoleUserService.SaveChangesAsync();
            return (new ResultViewModel { Data = result, Message = new ValidationDto<Role_User>(true, "Success", "Success", result).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // POST: api/form/delete  
        [HttpPost("remove")]
        public async Task<ResultViewModel> RemoveRoleUser([FromBody] int roleUserId)
        {
            //is validation model
            if (roleUserId == 0)
                throw new CustomException<int>(new ValidationDto<int>(false, "WorkflowRole", "CorruptedWorkflowRole", roleUserId), 500);

            var fetchForm = await _RoleUserService.GetRoleUserById(roleUserId);
            if (fetchForm == null)
                throw new CustomException<Role_User>(new ValidationDto<Role_User>(false, "WorkflowRole", "CorruptedWorkflowRole", fetchForm), 500);

            var validationModel = await _RoleUserService.RoleUserValidation(fetchForm);
            if (!validationModel.IsSuccess)
                throw new CustomException<Role_User>(validationModel, 500);

            //initial action
            await _RoleUserService.DeleteRoleUser(roleUserId);
            var saveResult = await _RoleUserService.SaveChangesAsync();

            if (!saveResult.IsSuccess)
                throw new CustomException<string>(saveResult, 500);

            return (new ResultViewModel { Data = fetchForm, Message = new ValidationDto<Role_User>(true, "Success", "Success", fetchForm).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // GET: api/form/all  
        [HttpGet("all")]
        public async Task<ResultViewModel> GetAllRoleUser(int pageSize, int pageNumber)
        {
            if (pageSize > 100)
                pageSize = 100;
            if (pageNumber < 1)
                pageNumber = 1;

            var forms = await _RoleUserService.GetAllRoleUsers(pageSize, pageNumber);

            //is valid data
            if ((((pageSize * pageNumber) - forms.TotalCount) > pageSize) && (pageSize * pageNumber) > forms.TotalCount)
                throw new CustomException<ListDto<Role_User>>(new ValidationDto<ListDto<Role_User>>(false, "Form", "CorruptedInvalidPage", forms), 500);

            return (new ResultViewModel {Data = forms.Data , ListNumber = forms.ListNumber , ListSize = forms.ListSize , TotalCount = forms.TotalCount, Message = new ValidationDto<ListDto<Role_User>>(true, "Success", "Success", forms).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // GET: api/form/{id}  
        [HttpGet("{roleUserId}")]
        public async Task<ResultViewModel> GetRoleUser(int roleUserId)
        {                        //is validation model
            if (roleUserId == 0)
                throw new CustomException<int>(new ValidationDto<int>(false, "UserWorkflow", "CorruptedUserWorkflow", roleUserId), 500);

            //initial action
            var RoleUser = await _RoleUserService.GetRoleUserById(roleUserId);
            if (RoleUser == null)
                throw new CustomException<Role_User>(new ValidationDto<Role_User>(false, "UserWorkflow", "CorruptedUserWorkflow", RoleUser), 500);

            var validationModel = await _RoleUserService.RoleUserValidation(RoleUser);
            if (!validationModel.IsSuccess)
                throw new CustomException<Role_User>(validationModel, 500);

            var form = await _RoleUserService.GetRoleUserById(roleUserId);
            return (new ResultViewModel { Data = form, Message = new ValidationDto<Role_User>(true, "Success", "Success", form).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // GET: api/form/{id}  
        [HttpGet("RoleUserById")]
        public async Task<ResultViewModel> GetRoleUserBuUserId(int pageSize, int pageNumber)
        {
            if (pageSize > 100)
                pageSize = 100;
            if (pageNumber < 1)
                pageNumber = 1;

            var claims = await HttpContext.Authorize();
           
            if (claims.UserId == 0)
                throw new CustomException<int>(new ValidationDto<int>(false, "UserWorkflow", "CorruptedUserWorkflow", claims.UserId), 500);

            //initial action
            var RoleUser = await _RoleUserService.GetRoleUserByUserId(claims.UserId, pageSize, pageNumber);
            if (RoleUser == null)
                throw new CustomException<ListDto<Role_User>>(new ValidationDto<ListDto<Role_User>>(false, "UserWorkflow", "CorruptedUserWorkflow", RoleUser), 500);

            //is valid data
            if ((((pageSize * pageNumber) - RoleUser.TotalCount) > pageSize) && (pageSize * pageNumber) > RoleUser.TotalCount)
                throw new CustomException<ListDto<Role_User>>(new ValidationDto<ListDto<Role_User>>(false, "Form", "CorruptedInvalidPage", RoleUser), 500);

            return (new ResultViewModel { Data = RoleUser.Data , ListNumber = RoleUser.ListNumber , ListSize = RoleUser.ListSize , TotalCount = RoleUser.TotalCount, Message = new ValidationDto<ListDto<Role_User>>(true, "Success", "Success", RoleUser).GetMessage(200), Status = true, StatusCode = 200 });
        }

            // GET: api/form/all  
        [HttpGet("User")]
        public async Task<ResultViewModel> GetAllROleUserAndUser(int roleId, int pageSize, int pageNumber)
        {
            if (pageSize > 100)
                pageSize = 100;
            if (pageNumber < 1)
                pageNumber = 1;

            ListDto<WorkflowAccess> forms = await _roleService.GetAllUserforRoleAccess(roleId, pageSize, pageNumber);

            //is valid data
            if ((((pageSize * pageNumber) - forms.TotalCount) > pageSize) && (pageSize * pageNumber) > forms.TotalCount)
                throw new CustomException<ListDto<WorkflowAccess>>(new ValidationDto<ListDto<WorkflowAccess>>(false, "Form", "CorruptedInvalidPage", forms), 500);

            return (new ResultViewModel { Data = forms.Data, ListNumber = forms.ListNumber, ListSize = forms.ListSize, TotalCount = forms.TotalCount, Message = new ValidationDto<ListDto<WorkflowAccess>>(true, "Success", "Success", forms).GetMessage(200), Status = true, StatusCode = 200 });
        }
    }
}
