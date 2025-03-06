using DataLayer.Models.FormBuilder;
using Microsoft.AspNetCore.Mvc;
using Services;
using ViewModels.ViewModels.FormBuilder;
using ViewModels;
using ViewModels.ViewModels.WorkFlow;
using DataLayer.Models.WorkFlow;

namespace AutomationEngine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class WorkFlowController : Controller
    {
        private readonly IWorkFlowService _workFlowService;

        public WorkFlowController(IWorkFlowService workFlowService)
        {
            _workFlowService = workFlowService;
        }

        // POST: api/form/create  
        [HttpPost("create")]
        public async Task<ResultViewModel> CreateWorkFLow([FromBody] WorkFlowDto workFlow)
        {
            var result = new WorkFlow()
            {
                Name = workFlow.Name,
                Description = workFlow.Description,
                Nodes = workFlow.Nodes.Select(x => new Node()
                {
                    Id = "" ,
                    entityId = x.entityId == 0 ? null : x.entityId,
                    formId = x.formId == 0 ? null : x.formId,
                    Icon = x.Icon,
                    Name = x.Name,
                    Type = x.Type,
                    X = x.X,
                    Y = x.Y
                }).ToList(),
                Edges = workFlow.Edges.Select(x => new Edge()
                {
                    Id = "",
                    Source = x.Source,
                    SourceHandle = x.SourceHandle,
                    Target = x.Target,
                    TargetHandle = x.TargetHandle
                }).ToList()
            };

            await _workFlowService.InsertWorFlow(result);
            await _workFlowService.SaveChangesAsync();
            return (new ResultViewModel { Data = result, Message = "عملیات با موفقیت انجام شد", Status = true });
        }

        // POST: api/form/update  
        [HttpPost("update")]
        public async Task<ResultViewModel> UpdateForm([FromBody] WorkFlowDto workFlow)
        {
            var result = new WorkFlow()
            {
                Id = workFlow.Id,
                Name = workFlow.Name,
                Description = workFlow.Description,
                Nodes = workFlow.Nodes.Select(x => new Node()
                {
                    entityId = x.entityId,
                    formId = x.formId,
                    Icon = x.Icon,
                    Name = x.Name,
                    Type = x.Type,
                    X = x.X,
                    Y = x.Y
                }).ToList(),
                Edges = workFlow.Edges.Select(x => new Edge()
                {
                    Source = x.Source,
                    SourceHandle = x.SourceHandle,
                    Target = x.Target,
                    TargetHandle = x.TargetHandle
                }).ToList()
            };

            await _workFlowService.UpdateWorFlow(result);
            await _workFlowService.SaveChangesAsync();
            return (new ResultViewModel { Data = result, Message = "عملیات با موفقیت انجام شد", Status = true });
        }

        // POST: api/form/delete  
        [HttpPost("remove")]
        public async Task<ResultViewModel> RemoveForm([FromBody] int workFlowId)
        {
            await _workFlowService.DeleteWorFlow(workFlowId);
            _workFlowService.SaveChangesAsync();
            return (new ResultViewModel { Data = null, Message = "عملیات با موفقیت انجام شد", Status = true });
        }

        // GET: api/form/all  
        [HttpGet("all")]
        public async Task<ResultViewModel> GetAllForms()
        {
            var forms = await _workFlowService.GetAllWorFlows();
            return (new ResultViewModel { Data = forms, Message = "عملیات با موفقیت انجام شد", Status = true });
        }

        // GET: api/form/{id}  
        [HttpGet("{workFlowId}")]
        public async Task<ResultViewModel> GetForm(int workFlowId)
        {
            var form = await _workFlowService.GetWorFlowById(workFlowId);
            return (new ResultViewModel { Data = form, Message = "عملیات با موفقیت انجام شد", Status = true });
        }

        // GET: api/form/{id}  
        [HttpGet("{workFlowId}/value")]
        public async Task<ResultViewModel> GetWorkflowValueById(int workFlowId, int userId)
        {
            var form = await _workFlowService.GetWorFlowValueById(workFlowId , userId);
            return (new ResultViewModel { Data = form, Message = "عملیات با موفقیت انجام شد", Status = true });
        }

        // GET: api/form/{id}  
        [HttpGet("{workFlowId}/next/value")]
        public async Task<ResultViewModel> GetNextWorkflowValueById(int workFlowId, int userId)
        {
            var form = await _workFlowService.GetNextWorFlowValueById(workFlowId,  userId);
            return (new ResultViewModel { Data = form, Message = "عملیات با موفقیت انجام شد", Status = true });
        }

        // GET: api/form/{id}  
        [HttpGet("{workFlowId}/last/value")]
        public async Task<ResultViewModel> GetLastWorkflowValueById(int workFlowId, int userId)
        {
            var form = await _workFlowService.GetLastWorFlowValueById(workFlowId,  userId);
            return (new ResultViewModel { Data = form, Message = "عملیات با موفقیت انجام شد", Status = true });
        }
    }
}
