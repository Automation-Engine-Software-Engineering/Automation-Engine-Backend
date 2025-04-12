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
using Tools.AuthoraizationTools;
using Entities.Models.Enums;

namespace AutomationEngine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [CheckAccess]
    public class WorkflowUserController : Controller
    {
        private readonly IWorkflowUserService _WorkflowUserService;
        private readonly IWorkflowService _workflowService;
        private readonly TokenGenerator _tokenGenerator;

        public WorkflowUserController(IWorkflowUserService WorkflowUserService, IWorkflowService workflowService, TokenGenerator tokenGenerator)
        {
            _WorkflowUserService = WorkflowUserService;
            _workflowService = workflowService;
            _tokenGenerator = tokenGenerator;
        }
        // POST: api/form/create  
        [HttpPost("create")]
        public async Task<ResultViewModel> CreateWorkflowUser([FromBody] WorkflowUserDto workflowUser)
        {
            if (workflowUser == null)
                throw new CustomException<Workflow_User>(new ValidationDto<Workflow_User>(false, "UserWorkflow", "CorruptedUserWorkflow", null), 500);

            var workflow = await _workflowService.GetWorkflowByIdAsync(workflowUser.WorkflowId);

            var claims = await HttpContext.Authorize();
         
            var result = new Workflow_User()
            {
                UserId = claims.UserId,
                WorkflowId = workflowUser.WorkflowId,
                Workflow = workflow,
                WorkflowState = workflow.Nodes.FirstOrDefault(x => x.PreviousNodeId.IsNullOrEmpty()).Id
            };

            //is validation model
            if (workflowUser.Id != 0)
                throw new CustomException<Workflow_User>(new ValidationDto<Workflow_User>(false, "UserWorkflow", "CorruptedUserWorkflow", result), 500);

            var validationModel = await _WorkflowUserService.WorkflowValidation(result);
            if (!validationModel.IsSuccess)
                throw new CustomException<Workflow_User>(validationModel, 500);


            await _WorkflowUserService.InsertWorkflowUser(result);
            await _WorkflowUserService.SaveChangesAsync();
            return (new ResultViewModel { Data = result, Message = "عملیات با موفقیت انجام شد.", Status = true, StatusCode = 200 });
        }

        // POST: api/form/update  
        [HttpPost("update")]
        public async Task<ResultViewModel> UpdateWorkflowUser([FromBody] WorkflowUserDto workflowUser)
        {
            if (workflowUser == null)
                throw new CustomException<Workflow_User>(new ValidationDto<Workflow_User>(false, "UserWorkflow", "CorruptedUserWorkflow", null), 500);

            var workflow = await _workflowService.GetWorkflowByIdAsync(workflowUser.WorkflowId);

            var claims = await HttpContext.Authorize();
            var result = new Workflow_User()
            {
                Id = workflowUser.Id,
                UserId = claims.UserId,
                WorkflowId = workflowUser.WorkflowId,
                WorkflowState = workflow.Nodes.FirstOrDefault().Id
            };

            //is validation model
            if (workflowUser.Id == 0)
                throw new CustomException<Workflow_User>(new ValidationDto<Workflow_User>(false, "UserWorkflow", "CorruptedUserWorkflow", result), 500);

            var validationModel = await _WorkflowUserService.WorkflowValidation(result);
            if (!validationModel.IsSuccess)
                throw new CustomException<Workflow_User>(validationModel, 500);

            await _WorkflowUserService.UpdateWorkflowUser(result);
            await _WorkflowUserService.SaveChangesAsync();
            return (new ResultViewModel { Data = result, Message = "عملیات با موفقیت انجام شد.", Status = true, StatusCode = 200 });
        }

        // POST: api/form/delete  
        [HttpPost("remove")]
        public async Task<ResultViewModel> RemoveWorkflowUser([FromBody] int workflowUserId)
        {
            //is validation model
            if (workflowUserId == 0)
                throw new CustomException<int>(new ValidationDto<int>(false, "UserWorkflow", "CorruptedUserWorkflow", workflowUserId), 500);

            var fetchForm = await _WorkflowUserService.GetWorkflowUserById(workflowUserId);
            if (fetchForm == null)
                throw new CustomException<int>(new ValidationDto<int>(false, "UserWorkflow", "CorruptedUserWorkflow", workflowUserId), 500);

            var validationModel = await _WorkflowUserService.WorkflowValidation(fetchForm);
            if (!validationModel.IsSuccess)
                throw new CustomException<Workflow_User>(validationModel, 500);

            //initial action
            await _WorkflowUserService.DeleteWorkflowUser(workflowUserId);
            var saveResult = await _WorkflowUserService.SaveChangesAsync();

            if (!saveResult.IsSuccess)
                throw new CustomException<string>(saveResult, 500);

            return (new ResultViewModel { Data = fetchForm, Message = "عملیات با موفقیت انجام شد.", Status = true, StatusCode = 200 });
        }

        // GET: api/form/all  
        [HttpGet("all")]
        public async Task<ResultViewModel> GetAllWorkflowUser(int pageSize, int pageNumber)
        {
            if (pageSize > 100)
                pageSize = 100;
            if (pageNumber < 1)
                pageNumber = 1;

            var forms = await _WorkflowUserService.GetAllWorkflowUsers(pageSize, pageNumber);

            //is valid data
            if ((((pageSize * pageNumber) - forms.TotalCount) > pageSize) && (pageSize * pageNumber) > forms.TotalCount)
                throw new CustomException<ListDto<Workflow_User>>(new ValidationDto<ListDto<Workflow_User>>(false, "Form", "CorruptedInvalidPage", forms), 500);

            return (new ResultViewModel { Data = forms.Data, ListNumber = forms.ListNumber, ListSize = forms.ListSize, TotalCount = forms.TotalCount, Message = "عملیات با موفقیت انجام شد.", Status = true, StatusCode = 200 });
        }

        // GET: api/form/{id}  
        [HttpGet("{workflowUserId}")]
        public async Task<ResultViewModel> GetWorkflowUser(int workflowUserId)
        {                        //is validation model
            if (workflowUserId == 0)
                throw new CustomException<int>(new ValidationDto<int>(false, "UserWorkflow", "CorruptedUserWorkflow", workflowUserId), 500);

            //initial action
            var workflowUser = await _WorkflowUserService.GetWorkflowUserById(workflowUserId);
            if (workflowUser == null)
                throw new CustomException<int>(new ValidationDto<int>(false, "UserWorkflow", "CorruptedUserWorkflow", workflowUserId), 500);

            var validationModel = await _WorkflowUserService.WorkflowValidation(workflowUser);
            if (!validationModel.IsSuccess)
                throw new CustomException<Workflow_User>(validationModel, 500);

            var form = await _WorkflowUserService.GetWorkflowUserById(workflowUserId);
            return (new ResultViewModel { Data = form, Message = "عملیات با موفقیت انجام شد.", Status = true, StatusCode = 200 });
        }

        // GET: api/form/{id}  
        [HttpGet("workflow")]
        public async Task<ResultViewModel> GetWorkflowUserByUserAndWorkflowId(int workflowId)
        {                        //is validation model
            if (workflowId == 0)
                throw new CustomException<int>(new ValidationDto<int>(false, "UserWorkflow", "CorruptedUserWorkflow", workflowId), 500);

            var claims = await HttpContext.Authorize();
            //initial action
            var workflowUser = await _WorkflowUserService.GetWorkflowUserByWorkflowAndUserId(workflowId, claims.UserId);
            if (workflowUser == null)
                return (new ResultViewModel { Data = null, Message = "عملیات با موفقیت انجام شد.", Status = true, StatusCode = 200 });

            var validationModel = await _WorkflowUserService.WorkflowValidation(workflowUser);
            if (!validationModel.IsSuccess)
                throw new CustomException<Workflow_User>(validationModel, 500);

            return (new ResultViewModel { Data = workflowUser, Message = "عملیات با موفقیت انجام شد.", Status = true, StatusCode = 200 });
        }
    }
}
