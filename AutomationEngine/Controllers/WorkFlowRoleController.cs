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

namespace AutomationEngine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [CheckAccess]
    public class WorkFlowRoleController : Controller
    {
        private readonly IWorkFlowRoleService _WorkFlowRoleService;
        public WorkFlowRoleController(IWorkFlowRoleService WorkFlowRoleService, IWorkFlowService workFlowService)
        {
            _WorkFlowRoleService = WorkFlowRoleService;
        }

        // POST: api/form/create  
        [HttpPost("create")]
        public async Task<ResultViewModel> CreateWorkFlowRole([FromBody] WorkFlowRoleDto workFlowRole)
        {
            if (workFlowRole == null)
                throw new CustomException<Role_WorkFlow>(new ValidationDto<Role_WorkFlow>(false, "WorkflowRole", "CorruptedWorkflowRole", null), 500);

            var workFlow = await _WorkFlowRoleService.GetWorFlowRoleById(workFlowRole.Id);

            var result = new Role_WorkFlow()
            {
                WorkFlowId = workFlowRole.WorkFlowId,
                RoleId = workFlow.RoleId
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

            return (new ResultViewModel { Data = forms, Message = new ValidationDto<ListDto<Role_WorkFlow>>(true, "Success", "Success", forms).GetMessage(200), Status = true, StatusCode = 200 });
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
        [HttpGet("RoleWorkflow/{RoleId}")]
        public async Task<ResultViewModel> GetRoleWorkflowBuRoleId(int RoleId, int pageSize, int pageNumber)
        {
            if (pageSize > 100)
                pageSize = 100;
            if (pageNumber < 1)
                pageNumber = 1;

            //is validation model
            if (RoleId == 0)
                throw new CustomException<int>(new ValidationDto<int>(false, "WorkflowRole", "CorruptedWorkflowRole", RoleId), 500);

            //initial action
            var RoleUser = await _WorkFlowRoleService.GetAllWorFlowRolesBuRoleId(RoleId, pageSize, pageNumber);
            if (RoleUser == null)
                throw new CustomException<ListDto<Role_WorkFlow>>(new ValidationDto<ListDto<Role_WorkFlow>>(false, "WorkflowRole", "CorruptedWorkflowRole", RoleUser), 500);

            //is valid data
            if ((((pageSize * pageNumber) - RoleUser.TotalCount) > pageSize) && (pageSize * pageNumber) > RoleUser.TotalCount)
                throw new CustomException<ListDto<Role_WorkFlow>>(new ValidationDto<ListDto<Role_WorkFlow>>(false, "Form", "CorruptedInvalidPage", RoleUser), 500);

            return (new ResultViewModel { Data = RoleUser, Message = new ValidationDto<ListDto<Role_WorkFlow>>(true, "Success", "Success", RoleUser).GetMessage(200), Status = true, StatusCode = 200 });
        }
    }
}
