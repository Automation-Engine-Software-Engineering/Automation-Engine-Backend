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
        public async Task<ResultViewModel<object>> CreateWorkflowUser([FromBody] WorkflowUserDto workflowUser)
        {
            if (workflowUser == null)
                throw new CustomException("UserWorkflow", "CorruptedUserWorkflow");

            var workflow = await _workflowService.GetWorkflowByIdAsync(workflowUser.WorkflowId);

            var claims = await HttpContext.Authorize();

            if (workflow.Nodes?.Count == 0)
                throw new CustomException("UserWorkflow", "WorkflowNodeNotfound");
            var result = new Workflow_User()
            {
                UserId = claims.UserId,
                WorkflowId = workflowUser.WorkflowId,
                Workflow = workflow,
                WorkflowState = workflow.Nodes.FirstOrDefault(x => x.PreviousNodeId.IsNullOrEmpty()).Id
            };

            //is validation model
            if (workflowUser.Id != 0)
                throw new CustomException("UserWorkflow", "CorruptedUserWorkflow", result);

            var validationModel = _WorkflowUserService.WorkflowValidation(result);
            if (!validationModel.IsSuccess)
                throw validationModel;


            await _WorkflowUserService.InsertWorkflowUser(result);
            await _WorkflowUserService.SaveChangesAsync();
            return new ResultViewModel<object>(result);
        }

        // POST: api/form/update  
        [HttpPost("update")]
        public async Task<ResultViewModel<Workflow_User?>> UpdateWorkflowUser([FromBody] WorkflowUserDto workflowUser)
        {
            if (workflowUser == null)
                throw new CustomException("UserWorkflow", "CorruptedUserWorkflow");

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
                throw new CustomException("UserWorkflow", "CorruptedUserWorkflow", result);

            var validationModel = _WorkflowUserService.WorkflowValidation(result);
            if (!validationModel.IsSuccess)
                throw validationModel;

            await _WorkflowUserService.UpdateWorkflowUser(result);
            await _WorkflowUserService.SaveChangesAsync();
            return new ResultViewModel<Workflow_User?>(result);
        }

        // POST: api/form/delete  
        [HttpPost("remove")]
        public async Task<ResultViewModel<Workflow_User?>> RemoveWorkflowUser([FromBody] int workflowUserId)
        {
            //is validation model
            if (workflowUserId == 0)
                throw new CustomException("UserWorkflow", "CorruptedUserWorkflow", workflowUserId);

            var fetchForm = await _WorkflowUserService.GetWorkflowUserById(workflowUserId);
            if (fetchForm == null)
                throw new CustomException("UserWorkflow", "CorruptedUserWorkflow", workflowUserId);

            var validationModel = _WorkflowUserService.WorkflowValidation(fetchForm);
            if (!validationModel.IsSuccess) 
                throw validationModel;

            //initial action
            await _WorkflowUserService.DeleteWorkflowUser(workflowUserId);
            await _WorkflowUserService.SaveChangesAsync();

            return new ResultViewModel<Workflow_User?>(fetchForm);
        }

        // GET: api/form/all  
        [HttpGet("all")]
        public async Task<ResultViewModel<IEnumerable<Workflow_User?>>> GetAllWorkflowUser(int pageSize, int pageNumber)
        {
            if (pageSize > 100)
                pageSize = 100;
            if (pageNumber < 1)
                pageNumber = 1;

            var forms = await _WorkflowUserService.GetAllWorkflowUsers(pageSize, pageNumber);

            //is valid data
            if ((((pageSize * pageNumber) - forms.TotalCount) > pageSize) && (pageSize * pageNumber) > forms.TotalCount)
                throw new CustomException("Form", "CorruptedInvalidPage", forms);

            return new ResultViewModel<IEnumerable<Workflow_User?>> { Data = forms.Data, ListNumber = forms.ListNumber, ListSize = forms.ListSize, TotalCount = forms.TotalCount };
        }

        // GET: api/form/{id}  
        [HttpGet("{workflowUserId}")]
        public async Task<ResultViewModel<Workflow_User?>> GetWorkflowUser(int workflowUserId)
        {                        //is validation model
            if (workflowUserId == 0)
                throw new CustomException("UserWorkflow", "CorruptedUserWorkflow", workflowUserId);

            //initial action
            var workflowUser = await _WorkflowUserService.GetWorkflowUserById(workflowUserId);
            if (workflowUser == null)
                throw new CustomException("UserWorkflow", "CorruptedUserWorkflow", workflowUserId);

            var validationModel = _WorkflowUserService.WorkflowValidation(workflowUser);
            if (!validationModel.IsSuccess)
                throw validationModel;

            var form = await _WorkflowUserService.GetWorkflowUserById(workflowUserId);
            return new ResultViewModel<Workflow_User?>(form);
        }

        // GET: api/form/{id}  
        [HttpGet("workflow")]
        public async Task<ResultViewModel<Workflow_User?>> GetWorkflowUserByUserAndWorkflowId(int workflowId)
        {                        //is validation model
            if (workflowId == 0)
                throw new CustomException("UserWorkflow", "CorruptedUserWorkflow", workflowId);

            var claims = await HttpContext.Authorize();
            //initial action
            var workflowUser = await _WorkflowUserService.GetWorkflowUserByWorkflowAndUserId(workflowId, claims.UserId);
            if (workflowUser == null)
                return new ResultViewModel<Workflow_User?>();

            var validationModel = _WorkflowUserService.WorkflowValidation(workflowUser);
            if (!validationModel.IsSuccess)
                throw validationModel;

            return new ResultViewModel<Workflow_User?>(workflowUser);
        }
    }
}
