using DataLayer.Models.FormBuilder;
using Microsoft.AspNetCore.Mvc;
using ViewModels.ViewModels.FormBuilder;
using ViewModels;
using DataLayer.Models.WorkFlow;
using Services;
using ViewModels.ViewModels.WorkFlow;

namespace AutomationEngine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkFlowUserController : Controller
    {
        private readonly IWorkFlowUserService _WorkFlowUserService;

        public WorkFlowUserController(IWorkFlowUserService WorkFlowUserService)
        {
            _WorkFlowUserService = WorkFlowUserService;
        }
        // POST: api/form/create  
        [HttpPost("create")]
        public async Task<ResultViewModel> CreateWorkFlowUser([FromBody] WorkFlowUserDto WorkFlowUser)
        {
            var result = new WorkFlow_User()
            {
                UserId = WorkFlowUser.UserId,
                WorkFlowId = WorkFlowUser.Id,
                WorkFlowState = WorkFlowUser.WorkFlowState
            };

            await _WorkFlowUserService.InsertWorFlowUser(result);
            await _WorkFlowUserService.SaveChangesAsync();
            return (new ResultViewModel { Data = result, Message = "عملیات با موفقیت انجام شد", Status = true });
        }

        // POST: api/form/update  
        [HttpPost("update")]
        public async Task<ResultViewModel> UpdateWorkFlowUser([FromBody] WorkFlowUserDto WorkFlowUser)
        {
            var result = new WorkFlow_User()
            {
                Id = WorkFlowUser.Id,
                UserId = WorkFlowUser.UserId,
                WorkFlowId = WorkFlowUser.Id,
                WorkFlowState = WorkFlowUser.WorkFlowState
            };

            await _WorkFlowUserService.UpdateWorFlowUser(result);
            await _WorkFlowUserService.SaveChangesAsync();
            return (new ResultViewModel { Data = result, Message = "عملیات با موفقیت انجام شد", Status = true });
        }

        // POST: api/form/delete  
        [HttpPost("remove")]
        public async Task<ResultViewModel> RemoveWorkFlowUser([FromBody] int WorkFlowUserId)
        {
            await _WorkFlowUserService.DeleteWorFlowUser(WorkFlowUserId);
            _WorkFlowUserService.SaveChangesAsync();
            return (new ResultViewModel { Data = null, Message = "عملیات با موفقیت انجام شد", Status = true });
        }

        // GET: api/form/all  
        [HttpGet("all")]
        public async Task<ResultViewModel> GetAllWorkFlowUser()
        {
            var forms = await _WorkFlowUserService.GetAllWorFlowUsers();
            return (new ResultViewModel { Data = forms, Message = "عملیات با موفقیت انجام شد", Status = true });
        }

        // GET: api/form/{id}  
        [HttpGet("{WorkFlowUserId}")]
        public async Task<ResultViewModel> GetWorkFlowUser(int WorkFlowUserId)
        {
            var form = await _WorkFlowUserService.GetWorFlowUserById(WorkFlowUserId);
            return (new ResultViewModel { Data = form, Message = "عملیات با موفقیت انجام شد", Status = true });
        }
    }
}
