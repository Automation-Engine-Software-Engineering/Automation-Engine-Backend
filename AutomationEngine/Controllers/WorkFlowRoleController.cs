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
                throw new CustomException("WorkflowRole", "CorruptedWorkflowRole");

            var result = new Role_Workflow()
            {
                WorkflowId = workflowRole.WorkflowId,
                RoleId = workflowRole.RoleId
            };

            //is validation model
            if (workflowRole.Id != 0)
                throw new CustomException("WorkflowRole", "CorruptedWorkflowRole", result);

            var validationModel = _WorkflowRoleService.WorkflowRoleValidation(result);
            if (!validationModel.IsSuccess)
                throw validationModel;

            await _WorkflowRoleService.InsertWorkflowRole(result);
            await _WorkflowRoleService.SaveChangesAsync();
            return new ResultViewModel<Role_Workflow?>(result);
        }

        // POST: api/form/create  
        [HttpPost("create/allByRoleId/{roleId}")]
        public async Task<ResultViewModel<List<Role_Workflow>?>> CreateWorkflowRoleAllByRoleId([FromBody] List<int> workflowIds, int roleId)
        {
            if (workflowIds == null)
                throw new CustomException("WorkflowRole", "CorruptedWorkflowRole");

            var workflows = new List<Role_Workflow>();

            //is validation model
            foreach (var role in workflows)
            {
                var validationModel = _WorkflowRoleService.WorkflowRoleValidation(role);
                if (!validationModel.IsSuccess)
                    throw validationModel;
            }

            await _WorkflowRoleService.ReplaceWorkflowRolesByRoleId(roleId, workflowIds);
            await _WorkflowRoleService.SaveChangesAsync();
            return new ResultViewModel<List<Role_Workflow>?>(workflows);
        }
        // POST: api/form/create  
        [HttpPost("create/allByWorkflowId/{workflowId}")]
        public async Task<ResultViewModel<List<Role_Workflow>?>> CreateWorkflowRoleAllByWorkflowId([FromBody] List<int> roleIds, int workflowid)
        {
            if (roleIds == null)
                throw new CustomException("WorkflowRole", "CorruptedWorkflowRole");

            var workflows = new List<Role_Workflow>();

            //is validation model
            foreach (var role in workflows)
            {
                var validationModel = _WorkflowRoleService.WorkflowRoleValidation(role);
                if (!validationModel.IsSuccess)
                    throw validationModel;
            }

            await _WorkflowRoleService.ReplaceWorkflowRolesByWorkflowId(workflowid, roleIds);
            await _WorkflowRoleService.SaveChangesAsync();
            return new ResultViewModel<List<Role_Workflow>?>(workflows);
        }
        // POST: api/form/update  
        [HttpPost("update")]
        public async Task<ResultViewModel<Role_Workflow?>> UpdateWorkflowRole([FromBody] WorkflowRoleDto workflowRole)
        {
            if (workflowRole == null)
                throw new CustomException("WorkflowRole", "CorruptedWorkflowRole");

            var workflow = await _WorkflowRoleService.GetWorkflowRoleById(workflowRole.Id);

            var result = new Role_Workflow()
            {
                Id = workflowRole.Id,
                WorkflowId = workflowRole.WorkflowId,
                RoleId = workflow.RoleId
            };

            //validation model
            if (workflowRole.Id == 0)
                throw new CustomException("WorkflowRole", "CorruptedWorkflowRole", result);

            var validationModel = _WorkflowRoleService.WorkflowRoleValidation(result);
            if (!validationModel.IsSuccess)
                throw validationModel;

            await _WorkflowRoleService.UpdateWorkflowRole(result);
            await _WorkflowRoleService.SaveChangesAsync();
            return new ResultViewModel<Role_Workflow?>(result);
        }

        // POST: api/form/delete  
        [HttpPost("remove")]
        public async Task<ResultViewModel<Role_Workflow?>> RemoveWorkflowRole([FromBody] int workflowUserId)
        {
            //is validation model
            if (workflowUserId == 0)
                throw new CustomException("WorkflowRole", "CorruptedWorkflowRole", workflowUserId);

            var fetchForm = await _WorkflowRoleService.GetWorkflowRoleById(workflowUserId);
            if (fetchForm == null)
                throw new CustomException("WorkflowRole", "CorruptedWorkflowRole", workflowUserId);

            var validationModel = _WorkflowRoleService.WorkflowRoleValidation(fetchForm);
            if (!validationModel.IsSuccess)
                throw validationModel;

            //initial action
            await _WorkflowRoleService.DeleteWorkflowRole(workflowUserId);
            await _WorkflowRoleService.SaveChangesAsync();

            return new ResultViewModel<Role_Workflow?>(fetchForm);
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
                throw new CustomException("Form", "CorruptedInvalidPage", forms);

            return new ResultViewModel<IEnumerable<Role_Workflow>?>
            {
                Data = forms.Data,
                ListNumber = forms.ListNumber,
                ListSize = forms.ListSize,
                TotalCount = forms.TotalCount
            };
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
                throw new CustomException("Form", "CorruptedInvalidPage", forms);

            return new ResultViewModel<IEnumerable<IsAccessModel>?>
            {
                Data = forms.Data,
                ListNumber = forms.ListNumber,
                ListSize = forms.ListSize,
                TotalCount = forms.TotalCount
            };
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
                throw new CustomException("Form", "CorruptedInvalidPage", forms);

            return new ResultViewModel<IEnumerable<IsAccessModel>?>
            {
                Data = forms.Data,
                ListNumber = forms.ListNumber,
                ListSize = forms.ListSize,
                TotalCount = forms.TotalCount
            };
        }

        // GET: api/form/{id}  
        [HttpGet("{workflowUserId}")]
        public async Task<ResultViewModel<Role_Workflow?>> GetWorkflowRole(int workflowRoleId)
        {                        //is validation model
            if (workflowRoleId == 0)
                throw new CustomException("WorkflowRole", "CorruptedWorkflowRole", workflowRoleId);

            //initial action
            var workflowUser = await _WorkflowRoleService.GetWorkflowRoleById(workflowRoleId);
            if (workflowUser == null)
                throw new CustomException("WorkflowRole", "CorruptedWorkflowRole", workflowRoleId);

            var validationModel = _WorkflowRoleService.WorkflowRoleValidation(workflowUser);
            if (!validationModel.IsSuccess)
                throw validationModel;

            var form = await _WorkflowRoleService.GetWorkflowRoleById(workflowRoleId);
            return new ResultViewModel<Role_Workflow?>(form);
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
                throw new CustomException("WorkflowRole", "CorruptedWorkflowRole", claims.RoleId);

            //initial action
            var RoleUser = await _WorkflowRoleService.GetAllWorkflowRolesByRoleId(claims.RoleId, pageSize, pageNumber);
            if (RoleUser == null)
                throw new CustomException("WorkflowRole", "CorruptedWorkflowRole", RoleUser);

            var workflows = new List<Role_Workflow>();
            workflows = RoleUser.Data.ToList();

            //is valid data
            if ((((pageSize * pageNumber) - RoleUser.TotalCount) > pageSize) && (pageSize * pageNumber) > RoleUser.TotalCount)
                throw new CustomException("Form", "CorruptedInvalidPage", RoleUser);

            return new ResultViewModel<List<Role_Workflow>?>
            {
                Data = workflows,
                ListNumber = RoleUser.ListNumber,
                ListSize = RoleUser.ListSize,
                TotalCount = RoleUser.TotalCount
            };
        }
    }
}
