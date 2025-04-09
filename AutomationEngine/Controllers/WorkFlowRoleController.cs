using Entities.Models.FormBuilder;
using Microsoft.AspNetCore.Mvc;
using ViewModels.ViewModels.FormBuilder;
using ViewModels;
using Entities.Models.WorkFlows;
using Services;
using ViewModels.ViewModels.WorkFlow;
using AutomationEngine.ControllerAttributes;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using Microsoft.IdentityModel.Tokens;
using Tools.TextTools;
using Entities.Models.MainEngine;
using Tools.AuthoraizationTools;
using Entities.Models.Enums;

namespace AutomationEngine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [CheckAccess]
    public class WorkFlowRoleController : Controller
    {
        private readonly IWorkFlowRoleService _WorkFlowRoleService;
        private readonly TokenGenerator _tokenGenerator;
        public WorkFlowRoleController(IWorkFlowRoleService WorkFlowRoleService, IWorkFlowService workFlowService, TokenGenerator tokenGenerator)
        {
            _WorkFlowRoleService = WorkFlowRoleService;
            _tokenGenerator = tokenGenerator;
        }

        // POST: api/form/create  
        [HttpPost("create")]
        public async Task<ResultViewModel> CreateWorkFlowRole([FromBody] WorkFlowRoleDto workFlowRole)
        {
            if (workFlowRole == null)
                throw new CustomException<Role_WorkFlow>(new ValidationDto<Role_WorkFlow>(false, "WorkflowRole", "CorruptedWorkflowRole", null), 500);

            var result = new Role_WorkFlow()
            {
                WorkFlowId = workFlowRole.WorkFlowId,
                RoleId = workFlowRole.RoleId
            };

            //is validation model
            if (workFlowRole.Id != 0)
                throw new CustomException<Role_WorkFlow>(new ValidationDto<Role_WorkFlow>(false, "WorkflowRole", "CorruptedWorkflowRole", result), 500);

            var validationModel = await _WorkFlowRoleService.WorkFlowRoleValidation(result);
            if (!validationModel.IsSuccess)
                throw new CustomException<Role_WorkFlow>(validationModel, 500);


            await _WorkFlowRoleService.InsertWorFlowRole(result);
            await _WorkFlowRoleService.SaveChangesAsync();
            return (new ResultViewModel { Data = result, Message = new ValidationDto<Role_WorkFlow>(true, "Success", "Success", result).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // POST: api/form/create  
        [HttpPost("create/allbyroleid/{roleId}")]
        public async Task<ResultViewModel> CreateWorkFlowRoleAllByRoleId([FromBody] List<int> workFlowIds,int roleId)
        {
            if (workFlowIds == null)
                throw new CustomException<Role_WorkFlow>(new ValidationDto<Role_WorkFlow>(false, "WorkflowRole", "CorruptedWorkflowRole", null), 500);

            var workFlows = new List<Role_WorkFlow>();
            
            //is validation model
            foreach (var role in workFlows)
            {
                var validationModel = await _WorkFlowRoleService.WorkFlowRoleValidation(role);
                if (!validationModel.IsSuccess)
                    throw new CustomException<Role_WorkFlow>(validationModel, 500);
            }

            await _WorkFlowRoleService.ReplaceWorFlowRolesByRoleId(roleId,workFlowIds);
            await _WorkFlowRoleService.SaveChangesAsync();
            return (new ResultViewModel { Data = workFlows, Message = new ValidationDto<List<Role_WorkFlow>>(true, "Success", "Success", workFlows).GetMessage(200), Status = true, StatusCode = 200 });
        }
        // POST: api/form/create  
        [HttpPost("create/allbyworkflowid/{workflowId}")]
        public async Task<ResultViewModel> CreateWorkFlowRoleAllByWorkflowId([FromBody] List<int> roleIds,int workflowid)
        {
            if (roleIds == null)
                throw new CustomException<Role_WorkFlow>(new ValidationDto<Role_WorkFlow>(false, "WorkflowRole", "CorruptedWorkflowRole", null), 500);

            var workFlows = new List<Role_WorkFlow>();
            
            //is validation model
            foreach (var role in workFlows)
            {
                var validationModel = await _WorkFlowRoleService.WorkFlowRoleValidation(role);
                if (!validationModel.IsSuccess)
                    throw new CustomException<Role_WorkFlow>(validationModel, 500);
            }

            await _WorkFlowRoleService.ReplaceWorFlowRolesByWorkFlowId(workflowid,roleIds);
            await _WorkFlowRoleService.SaveChangesAsync();
            return (new ResultViewModel { Data = workFlows, Message = new ValidationDto<List<Role_WorkFlow>>(true, "Success", "Success", workFlows).GetMessage(200), Status = true, StatusCode = 200 });
        }
        // POST: api/form/update  
        [HttpPost("update")]
        public async Task<ResultViewModel> UpdateWorkFlowRole([FromBody] WorkFlowRoleDto workFlowRole)
        {
            if (workFlowRole == null)
                throw new CustomException<Role_WorkFlow>(new ValidationDto<Role_WorkFlow>(false, "WorkflowRole", "CorruptedWorkflowRole", null), 500);

            var workFlow = await _WorkFlowRoleService.GetWorFlowRoleById(workFlowRole.Id);

            var result = new Role_WorkFlow()
            {
                Id = workFlowRole.Id,
                WorkFlowId = workFlowRole.WorkFlowId,
                RoleId = workFlow.RoleId
            };

            //is validation model
            if (workFlowRole.Id == 0)
                throw new CustomException<Role_WorkFlow>(new ValidationDto<Role_WorkFlow>(false, "WorkflowRole", "CorruptedWorkflowRole", result), 500);

            var validationModel = await _WorkFlowRoleService.WorkFlowRoleValidation(result);
            if (!validationModel.IsSuccess)
                throw new CustomException<Role_WorkFlow>(validationModel, 500);


            await _WorkFlowRoleService.UpdateWorFlowRole(result);
            await _WorkFlowRoleService.SaveChangesAsync();
            return (new ResultViewModel { Data = result, Message = new ValidationDto<Role_WorkFlow>(true, "Success", "Success", result).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // POST: api/form/delete  
        [HttpPost("remove")]
        public async Task<ResultViewModel> RemoveWorkFlowRole([FromBody] int workFlowUserId)
        {
            //is validation model
            if (workFlowUserId == 0)
                throw new CustomException<int>(new ValidationDto<int>(false, "WorkflowRole", "CorruptedWorkflowRole", workFlowUserId), 500);

            var fetchForm = await _WorkFlowRoleService.GetWorFlowRoleById(workFlowUserId);
            if (fetchForm == null)
                throw new CustomException<int>(new ValidationDto<int>(false, "WorkflowRole", "CorruptedWorkflowRole", workFlowUserId), 500);

            var validationModel = await _WorkFlowRoleService.WorkFlowRoleValidation(fetchForm);
            if (!validationModel.IsSuccess)
                throw new CustomException<Role_WorkFlow>(validationModel, 500);

            //initial action
            await _WorkFlowRoleService.DeleteWorFlowRole(workFlowUserId);
            var saveResult = await _WorkFlowRoleService.SaveChangesAsync();

            if (!saveResult.IsSuccess)
                throw new CustomException<string>(saveResult, 500);

            return (new ResultViewModel { Data = fetchForm, Message = new ValidationDto<Role_WorkFlow>(true, "Success", "Success", fetchForm).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // GET: api/form/all  
        [HttpGet("all")]
        public async Task<ResultViewModel> GetAllWorkFlowRole(int pageSize, int pageNumber)
        {
            if (pageSize > 100)
                pageSize = 100;
            if (pageNumber < 1)
                pageNumber = 1;

            var forms = await _WorkFlowRoleService.GetAllWorFlowRoles(pageSize, pageNumber);

            //is valid data
            if ((((pageSize * pageNumber) - forms.TotalCount) > pageSize) && (pageSize * pageNumber) > forms.TotalCount)
                throw new CustomException<ListDto<Role_WorkFlow>>(new ValidationDto<ListDto<Role_WorkFlow>>(false, "Form", "CorruptedInvalidPage", forms), 500);

            return (new ResultViewModel { Data = forms.Data, ListNumber = forms.ListNumber, ListSize = forms.ListSize, TotalCount = forms.TotalCount, Message = new ValidationDto<ListDto<Role_WorkFlow>>(true, "Success", "Success", forms).GetMessage(200), Status = true, StatusCode = 200 });
        }


        // GET: api/form/all  
        [HttpGet("role/all")]
        public async Task<ResultViewModel> GetAllWorkFlowRoleAndRole(int RoleId, int pageSize, int pageNumber)
        {
            if (pageSize > 100)
                pageSize = 100;
            if (pageNumber < 1)
                pageNumber = 1;

            ListDto<WorkflowAccess> forms = await _WorkFlowRoleService.GetAllWorFlowRolesAndWorkflow(RoleId, pageSize, pageNumber);

            //is valid data
            if ((((pageSize * pageNumber) - forms.TotalCount) > pageSize) && (pageSize * pageNumber) > forms.TotalCount)
                throw new CustomException<ListDto<WorkflowAccess>>(new ValidationDto<ListDto<WorkflowAccess>>(false, "Form", "CorruptedInvalidPage", forms), 500);

            return (new ResultViewModel { Data = forms.Data, ListNumber = forms.ListNumber, ListSize = forms.ListSize, TotalCount = forms.TotalCount, Message = new ValidationDto<ListDto<WorkflowAccess>>(true, "Success", "Success", forms).GetMessage(200), Status = true, StatusCode = 200 });
        }

        
        // GET: api/form/all  
        [HttpGet("workflow/all")]
        public async Task<ResultViewModel> GetAllWorkFlowRoleAndWorkflow(int WorkflowId, int pageSize, int pageNumber)
        {
            if (pageSize > 100)
                pageSize = 100;
            if (pageNumber < 1)
                pageNumber = 1;

            ListDto<WorkflowAccess> forms = await _WorkFlowRoleService.GetAllWorFlowRolesAndRole(WorkflowId, pageSize, pageNumber);

            //is valid data
            if ((((pageSize * pageNumber) - forms.TotalCount) > pageSize) && (pageSize * pageNumber) > forms.TotalCount)
                throw new CustomException<ListDto<WorkflowAccess>>(new ValidationDto<ListDto<WorkflowAccess>>(false, "Form", "CorruptedInvalidPage", forms), 500);

            return (new ResultViewModel { Data = forms.Data, ListNumber = forms.ListNumber, ListSize = forms.ListSize, TotalCount = forms.TotalCount, Message = new ValidationDto<ListDto<WorkflowAccess>>(true, "Success", "Success", forms).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // GET: api/form/{id}  
        [HttpGet("{workFlowUserId}")]
        public async Task<ResultViewModel> GetWorkFlowRole(int workFlowRoleId)
        {                        //is validation model
            if (workFlowRoleId == 0)
                throw new CustomException<int>(new ValidationDto<int>(false, "WorkflowRole", "CorruptedWorkflowRole", workFlowRoleId), 500);

            //initial action
            var workflowUser = await _WorkFlowRoleService.GetWorFlowRoleById(workFlowRoleId);
            if (workflowUser == null)
                throw new CustomException<int>(new ValidationDto<int>(false, "WorkflowRole", "CorruptedWorkflowRole", workFlowRoleId), 500);

            var validationModel = await _WorkFlowRoleService.WorkFlowRoleValidation(workflowUser);
            if (!validationModel.IsSuccess)
                throw new CustomException<Role_WorkFlow>(validationModel, 500);

            var form = await _WorkFlowRoleService.GetWorFlowRoleById(workFlowRoleId);
            return (new ResultViewModel { Data = form, Message = new ValidationDto<Role_WorkFlow>(true, "Success", "Success", form).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // GET: api/form/{id}  
        [HttpGet("RoleWorkflow")]
        public async Task<ResultViewModel> GetRoleWorkflowByRoleId(int pageSize, int pageNumber)
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
            var RoleUser = await _WorkFlowRoleService.GetAllWorFlowRolesByRoleId(claims.RoleId, pageSize, pageNumber);
            if (RoleUser == null)
                throw new CustomException<ListDto<Role_WorkFlow>>(new ValidationDto<ListDto<Role_WorkFlow>>(false, "WorkflowRole", "CorruptedWorkflowRole", RoleUser), 500);

            var workflows = new List<Role_WorkFlow>();
            workflows = RoleUser.Data.ToList();

            //is valid data
            if ((((pageSize * pageNumber) - RoleUser.TotalCount) > pageSize) && (pageSize * pageNumber) > RoleUser.TotalCount)
                throw new CustomException<ListDto<Role_WorkFlow>>(new ValidationDto<ListDto<Role_WorkFlow>>(false, "Form", "CorruptedInvalidPage", RoleUser), 500);

            return (new ResultViewModel { Data = workflows, ListNumber = RoleUser.ListNumber, ListSize = RoleUser.ListSize, TotalCount = RoleUser.TotalCount, Message = new ValidationDto<ListDto<Role_WorkFlow>>(true, "Success", "Success", RoleUser).GetMessage(200), Status = true, StatusCode = 200 });
        }
    }
}
