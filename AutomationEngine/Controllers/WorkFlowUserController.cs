using DataLayer.Models.FormBuilder;
using Microsoft.AspNetCore.Mvc;
using ViewModels.ViewModels.FormBuilder;
using ViewModels;
using DataLayer.Models.WorkFlow;
using Services;
using ViewModels.ViewModels.WorkFlow;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using Microsoft.IdentityModel.Tokens;

namespace AutomationEngine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkFlowUserController : Controller
    {
        private readonly IWorkFlowUserService _WorkFlowUserService;
        private readonly IWorkFlowService _workFlowService;

        public WorkFlowUserController(IWorkFlowUserService WorkFlowUserService, IWorkFlowService workFlowService)
        {
            _WorkFlowUserService = WorkFlowUserService;
            _workFlowService = workFlowService;
        }
        // POST: api/form/create  
        [HttpPost("create")]
        public async Task<ResultViewModel> CreateWorkFlowUser([FromBody] WorkFlowUserDto workFlowUser)
        {
            if (workFlowUser == null)
                throw new CustomException<WorkFlow_User>(new ValidationDto<WorkFlow_User>(false, "UserWorkflow", "CorruptedUserWorkflow", null), 500);

            var workFlow = await _workFlowService.GetWorFlowByIdAsync(workFlowUser.WorkFlowId);

            var result = new WorkFlow_User()
            {
                UserId = workFlowUser.UserId,
                WorkFlowId = workFlowUser.WorkFlowId,
                WorkFlowState = workFlow.Nodes.FirstOrDefault(x => x.LastNodeId.IsNullOrEmpty()).Id
            };

            //is validation model
            if (workFlowUser.Id != 0)
                throw new CustomException<WorkFlow_User>(new ValidationDto<WorkFlow_User>(false, "UserWorkflow", "CorruptedUserWorkflow", result), 500);

            var validationModel = await _WorkFlowUserService.WorkFlowValidation(result);
            if (!validationModel.IsSuccess)
                throw new CustomException<WorkFlow_User>(validationModel, 500);


            await _WorkFlowUserService.InsertWorFlowUser(result);
            await _WorkFlowUserService.SaveChangesAsync();
            return (new ResultViewModel { Data = result, Message = "عملیات با موفقیت انجام شد.", Status = true, StatusCode = 200 });
        }

        // POST: api/form/update  
        [HttpPost("update")]
        public async Task<ResultViewModel> UpdateWorkFlowUser([FromBody] WorkFlowUserDto workFlowUser)
        {
            if (workFlowUser == null)
                throw new CustomException<WorkFlow_User>(new ValidationDto<WorkFlow_User>(false, "UserWorkflow", "CorruptedUserWorkflow", null), 500);

            var workFlow = await _workFlowService.GetWorFlowByIdAsync(workFlowUser.WorkFlowId);

            var result = new WorkFlow_User()
            {
                Id = workFlowUser.Id,
                UserId = workFlowUser.UserId,
                WorkFlowId = workFlowUser.WorkFlowId,
                WorkFlowState = workFlow.Nodes.FirstOrDefault().Id
            };

            //is validation model
            if (workFlowUser.Id == 0)
                throw new CustomException<WorkFlow_User>(new ValidationDto<WorkFlow_User>(false, "UserWorkflow", "CorruptedUserWorkflow", result), 500);

            var validationModel = await _WorkFlowUserService.WorkFlowValidation(result);
            if (!validationModel.IsSuccess)
                throw new CustomException<WorkFlow_User>(validationModel, 500);

            await _WorkFlowUserService.UpdateWorFlowUser(result);
            await _WorkFlowUserService.SaveChangesAsync();
            return (new ResultViewModel { Data = result, Message = "عملیات با موفقیت انجام شد.", Status = true, StatusCode = 200 });
        }

        // POST: api/form/delete  
        [HttpPost("remove")]
        public async Task<ResultViewModel> RemoveWorkFlowUser([FromBody] int workFlowUserId)
        {
            //is validation model
            if (workFlowUserId == 0)
                throw new CustomException<int>(new ValidationDto<int>(false, "UserWorkflow", "CorruptedUserWorkflow", workFlowUserId), 500);

            var fetchForm = await _WorkFlowUserService.GetWorFlowUserById(workFlowUserId);
            if (fetchForm == null)
                throw new CustomException<int>(new ValidationDto<int>(false, "UserWorkflow", "CorruptedUserWorkflow", workFlowUserId), 500);

            var validationModel = await _WorkFlowUserService.WorkFlowValidation(fetchForm);
            if (!validationModel.IsSuccess)
                throw new CustomException<WorkFlow_User>(validationModel, 500);

            //initial action
            await _WorkFlowUserService.DeleteWorFlowUser(workFlowUserId);
            var saveResult = await _WorkFlowUserService.SaveChangesAsync();

            if (!saveResult.IsSuccess)
                throw new CustomException<string>(saveResult, 500);

            return (new ResultViewModel { Data = null, Message = "عملیات با موفقیت انجام شد.", Status = true, StatusCode = 200 });
        }

        // GET: api/form/all  
        [HttpGet("all")]
        public async Task<ResultViewModel> GetAllWorkFlowUser(int pageSize, int pageNumber)
        {
            if (pageSize > 100)
                pageSize = 100;
            if (pageNumber < 1)
                pageNumber = 1;

            var forms = await _WorkFlowUserService.GetAllWorFlowUsers(pageSize, pageNumber);

            //is valid data
            if ((((pageSize * pageNumber) - forms.TotalCount) > pageSize) && (pageSize * pageNumber) > forms.TotalCount)
                throw new CustomException<ListDto<WorkFlow_User>>(new ValidationDto<ListDto<WorkFlow_User>>(false, "Form", "CorruptedInvalidPage", forms), 500);

            return (new ResultViewModel { Data = forms, Message = "عملیات با موفقیت انجام شد.", Status = true, StatusCode = 200 });
        }

        // GET: api/form/{id}  
        [HttpGet("{workFlowUserId}")]
        public async Task<ResultViewModel> GetWorkFlowUser(int workFlowUserId)
        {                        //is validation model
            if (workFlowUserId == 0)
                throw new CustomException<int>(new ValidationDto<int>(false, "UserWorkflow", "CorruptedUserWorkflow", workFlowUserId), 500);

            //initial action
            var workflowUser = await _WorkFlowUserService.GetWorFlowUserById(workFlowUserId);
            if (workflowUser == null)
                throw new CustomException<int>(new ValidationDto<int>(false, "UserWorkflow", "CorruptedUserWorkflow", workFlowUserId), 500);

            var validationModel = await _WorkFlowUserService.WorkFlowValidation(workflowUser);
            if (!validationModel.IsSuccess)
                throw new CustomException<WorkFlow_User>(validationModel, 500);

            var form = await _WorkFlowUserService.GetWorFlowUserById(workFlowUserId);
            return (new ResultViewModel { Data = form, Message = "عملیات با موفقیت انجام شد.", Status = true, StatusCode = 200 });
        }
    }
}
