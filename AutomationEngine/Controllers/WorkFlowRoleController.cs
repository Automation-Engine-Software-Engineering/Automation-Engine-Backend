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
using Microsoft.AspNetCore.Identity;

namespace AutomationEngine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [CheckAccess]
    public class WorkflowRoleController : Controller
    {
        private readonly IWorkflowRoleService _WorkflowRoleService;
        private readonly TokenGenerator _tokenGenerator;
        public WorkflowRoleController(IWorkflowRoleService WorkflowRoleService, IWorkflowService workflowService, TokenGenerator tokenGenerator)
        {
            _WorkflowRoleService = WorkflowRoleService;
            _tokenGenerator = tokenGenerator;
        }

        // POST: api/form/create  
        [HttpPost("create")]
        public async Task<ResultViewModel<Role_Workflow?>> CreateWorkflowRole([FromBody] WorkflowRoleDto workflowRole)
        {
            if (workflowRole == null)
                throw new CustomException<Role_Workflow>(new ValidationDto<Role_Workflow>(false, "WorkflowRole", "CorruptedWorkflowRole", null), 500);

            var result = new Role_Workflow()
            {
                WorkflowId = workflowRole.WorkflowId,
                RoleId = workflowRole.RoleId
            };

            //is validation model
            if (workflowRole.Id != 0)
                throw new CustomException<Role_Workflow>(new ValidationDto<Role_Workflow>(false, "WorkflowRole", "CorruptedWorkflowRole", result), 500);

            var validationModel = await _WorkflowRoleService.WorkflowRoleValidation(result);
            if (!validationModel.IsSuccess)
                throw new CustomException<Role_Workflow>(validationModel, 500);


            await _WorkflowRoleService.InsertWorkflowRole(result);
            await _WorkflowRoleService.SaveChangesAsync();
            return (new ResultViewModel<Role_Workflow?> { Data = result, Message = new ValidationDto<Role_Workflow>(true, "Success", "Success", result).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // POST: api/form/create  
        [HttpPost("create/allByRoleId/{roleId}")]
        public async Task<ResultViewModel<List<Role_Workflow>?>> CreateWorkflowRoleAllByRoleId([FromBody] List<int> workflowIds,int roleId)
        {
            if (workflowIds == null)
                throw new CustomException<Role_Workflow>(new ValidationDto<Role_Workflow>(false, "WorkflowRole", "CorruptedWorkflowRole", null), 500);

            var workflows = new List<Role_Workflow>();
            
            //is validation model
            foreach (var role in workflows)
            {
                var validationModel = await _WorkflowRoleService.WorkflowRoleValidation(role);
                if (!validationModel.IsSuccess)
                    throw new CustomException<Role_Workflow>(validationModel, 500);
            }

            await _WorkflowRoleService.ReplaceWorkflowRolesByRoleId(roleId,workflowIds);
            await _WorkflowRoleService.SaveChangesAsync();
            return (new ResultViewModel<List<Role_Workflow>?> { Data = workflows, Message = new ValidationDto<List<Role_Workflow>>(true, "Success", "Success", workflows).GetMessage(200), Status = true, StatusCode = 200 });
        }
        // POST: api/form/create  
        [HttpPost("create/allByWorkflowId/{workflowId}")]
        public async Task<ResultViewModel<List<Role_Workflow>?>>  CreateWorkflowRoleAllByWorkflowId([FromBody] List<int> roleIds,int workflowid)
        {
            if (roleIds == null)
                throw new CustomException<Role_Workflow>(new ValidationDto<Role_Workflow>(false, "WorkflowRole", "CorruptedWorkflowRole", null), 500);

            var workflows = new List<Role_Workflow>();
            
            //is validation model
            foreach (var role in workflows)
            {
                var validationModel = await _WorkflowRoleService.WorkflowRoleValidation(role);
                if (!validationModel.IsSuccess)
                    throw new CustomException<Role_Workflow>(validationModel, 500);
            }

            await _WorkflowRoleService.ReplaceWorkflowRolesByWorkflowId(workflowid,roleIds);
            await _WorkflowRoleService.SaveChangesAsync();
            return (new ResultViewModel<List<Role_Workflow>?> { Data = workflows, Message = new ValidationDto<List<Role_Workflow>>(true, "Success", "Success", workflows).GetMessage(200), Status = true, StatusCode = 200 });
        }
        // POST: api/form/update  
        [HttpPost("update")]
        public async Task<ResultViewModel<Role_Workflow?>> UpdateWorkflowRole([FromBody] WorkflowRoleDto workflowRole)
        {
            if (workflowRole == null)
                throw new CustomException<Role_Workflow>(new ValidationDto<Role_Workflow>(false, "WorkflowRole", "CorruptedWorkflowRole", null), 500);

            var workflow = await _WorkflowRoleService.GetWorkflowRoleById(workflowRole.Id);

            var result = new Role_Workflow()
            {
                Id = workflowRole.Id,
                WorkflowId = workflowRole.WorkflowId,
                RoleId = workflow.RoleId
            };

            //validation model
            if (workflowRole.Id == 0)
                throw new CustomException<Role_Workflow>(new ValidationDto<Role_Workflow>(false, "WorkflowRole", "CorruptedWorkflowRole", result), 500);

            var validationModel = await _WorkflowRoleService.WorkflowRoleValidation(result);
            if (!validationModel.IsSuccess)
                throw new CustomException<Role_Workflow>(validationModel, 500);


            await _WorkflowRoleService.UpdateWorkflowRole(result);
            await _WorkflowRoleService.SaveChangesAsync();
            return (new ResultViewModel<Role_Workflow?> { Data = result, Message = new ValidationDto<Role_Workflow>(true, "Success", "Success", result).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // POST: api/form/delete  
        [HttpPost("remove")]
        public async Task<ResultViewModel<Role_Workflow?>> RemoveWorkflowRole([FromBody] int workflowUserId)
        {
            //is validation model
            if (workflowUserId == 0)
                throw new CustomException<int>(new ValidationDto<int>(false, "WorkflowRole", "CorruptedWorkflowRole", workflowUserId), 500);

            var fetchForm = await _WorkflowRoleService.GetWorkflowRoleById(workflowUserId);
            if (fetchForm == null)
                throw new CustomException<int>(new ValidationDto<int>(false, "WorkflowRole", "CorruptedWorkflowRole", workflowUserId), 500);

            var validationModel = await _WorkflowRoleService.WorkflowRoleValidation(fetchForm);
            if (!validationModel.IsSuccess)
                throw new CustomException<Role_Workflow>(validationModel, 500);

            //initial action
            await _WorkflowRoleService.DeleteWorkflowRole(workflowUserId);
            var saveResult = await _WorkflowRoleService.SaveChangesAsync();

            if (!saveResult.IsSuccess)
                throw new CustomException<string>(saveResult, 500);

            return (new ResultViewModel<Role_Workflow?> { Data = fetchForm, Message = new ValidationDto<Role_Workflow>(true, "Success", "Success", fetchForm).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // GET: api/form/all  
        [HttpGet("all")]
        public async Task<ResultViewModel<IEnumerable<Role_Workflow>?>> GetAllWorkflowRole(int pageSize, int pageNumber)
        {
            if (pageSize > 100)
                pageSize = 100;
            if (pageNumber < 1)
                pageNumber = 1;

            var forms = await _WorkflowRoleService.GetAllWorkflowRoles(pageSize, pageNumber);

            //is valid data
            if ((((pageSize * pageNumber) - forms.TotalCount) > pageSize) && (pageSize * pageNumber) > forms.TotalCount)
                throw new CustomException<ListDto<Role_Workflow>>(new ValidationDto<ListDto<Role_Workflow>>(false, "Form", "CorruptedInvalidPage", forms), 500);

            return (new ResultViewModel<IEnumerable<Role_Workflow>?> { Data = forms.Data, ListNumber = forms.ListNumber, ListSize = forms.ListSize, TotalCount = forms.TotalCount, Message = new ValidationDto<ListDto<Role_Workflow>>(true, "Success", "Success", forms).GetMessage(200), Status = true, StatusCode = 200 });
        }


        // GET: api/form/all  
        [HttpGet("role/all")]
        public async Task<ResultViewModel<IEnumerable<IsAccessModel>?>> GetAllWorkflowRoleAndRole(int roleId, int pageSize, int pageNumber)
        {
            if (pageSize > 100)
                pageSize = 100;
            if (pageNumber < 1)
                pageNumber = 1;

            ListDto<IsAccessModel> forms = await _WorkflowRoleService.GetWorkflowsAccessByRoleId(roleId, pageSize, pageNumber);

            //is valid data
            if ((((pageSize * pageNumber) - forms.TotalCount) > pageSize) && (pageSize * pageNumber) > forms.TotalCount)
                throw new CustomException<ListDto<IsAccessModel>>(new ValidationDto<ListDto<IsAccessModel>>(false, "Form", "CorruptedInvalidPage", forms), 500);

            return (new ResultViewModel<IEnumerable<IsAccessModel>?> { Data = forms.Data, ListNumber = forms.ListNumber, ListSize = forms.ListSize, TotalCount = forms.TotalCount, Message = new ValidationDto<ListDto<IsAccessModel>>(true, "Success", "Success", forms).GetMessage(200), Status = true, StatusCode = 200 });
        }

        
        // GET: api/form/all  
        [HttpGet("workflow/all")]
        public async Task<ResultViewModel<IEnumerable<IsAccessModel>?>> GetAllWorkflowRoleAndWorkflow(int WorkflowId, int pageSize, int pageNumber)
        {
            if (pageSize > 100)
                pageSize = 100;
            if (pageNumber < 1)
                pageNumber = 1;

            ListDto<IsAccessModel> forms = await _WorkflowRoleService.GetRolesAccessByWorkflowId(WorkflowId, pageSize, pageNumber);

            //is valid data
            if ((((pageSize * pageNumber) - forms.TotalCount) > pageSize) && (pageSize * pageNumber) > forms.TotalCount)
                throw new CustomException<ListDto<IsAccessModel>>(new ValidationDto<ListDto<IsAccessModel>>(false, "Form", "CorruptedInvalidPage", forms), 500);

            return (new ResultViewModel<IEnumerable<IsAccessModel>?> { Data = forms.Data, ListNumber = forms.ListNumber, ListSize = forms.ListSize, TotalCount = forms.TotalCount, Message = new ValidationDto<ListDto<IsAccessModel>>(true, "Success", "Success", forms).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // GET: api/form/{id}  
        [HttpGet("{workflowUserId}")]
        public async Task<ResultViewModel<Role_Workflow?>> GetWorkflowRole(int workflowRoleId)
        {                        //is validation model
            if (workflowRoleId == 0)
                throw new CustomException<int>(new ValidationDto<int>(false, "WorkflowRole", "CorruptedWorkflowRole", workflowRoleId), 500);

            //initial action
            var workflowUser = await _WorkflowRoleService.GetWorkflowRoleById(workflowRoleId);
            if (workflowUser == null)
                throw new CustomException<int>(new ValidationDto<int>(false, "WorkflowRole", "CorruptedWorkflowRole", workflowRoleId), 500);

            var validationModel = await _WorkflowRoleService.WorkflowRoleValidation(workflowUser);
            if (!validationModel.IsSuccess)
                throw new CustomException<Role_Workflow>(validationModel, 500);

            var form = await _WorkflowRoleService.GetWorkflowRoleById(workflowRoleId);
            return (new ResultViewModel<Role_Workflow?> { Data = form, Message = new ValidationDto<Role_Workflow>(true, "Success", "Success", form).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // GET: api/form/{id}  
        [HttpGet("RoleWorkflow")]
        public async Task<ResultViewModel<List<Role_Workflow>?>> GetRoleWorkflowByRoleId(int pageSize, int pageNumber)
        {
            if (pageSize > 100)
                pageSize = 100;
            if (pageNumber < 1)
                pageNumber = 1;

            //is validation model
            var claims = await HttpContext.Authorize();
           
            if (claims.RoleId == 0)
                throw new CustomException<int>(new ValidationDto<int>(false, "WorkflowRole", "CorruptedWorkflowRole", claims.RoleId), 500);

            //initial action
            var RoleUser = await _WorkflowRoleService.GetAllWorkflowRolesByRoleId(claims.RoleId, pageSize, pageNumber);
            if (RoleUser == null)
                throw new CustomException<ListDto<Role_Workflow>>(new ValidationDto<ListDto<Role_Workflow>>(false, "WorkflowRole", "CorruptedWorkflowRole", RoleUser), 500);

            var workflows = new List<Role_Workflow>();
            workflows = RoleUser.Data.ToList();

            //is valid data
            if ((((pageSize * pageNumber) - RoleUser.TotalCount) > pageSize) && (pageSize * pageNumber) > RoleUser.TotalCount)
                throw new CustomException<ListDto<Role_Workflow>>(new ValidationDto<ListDto<Role_Workflow>>(false, "Form", "CorruptedInvalidPage", RoleUser), 500);

            return (new ResultViewModel<List<Role_Workflow>?> { Data = workflows, ListNumber = RoleUser.ListNumber, ListSize = RoleUser.ListSize, TotalCount = RoleUser.TotalCount, Message = new ValidationDto<ListDto<Role_Workflow>>(true, "Success", "Success", RoleUser).GetMessage(200), Status = true, StatusCode = 200 });
        }
    }
}
