using DataLayer.Models.FormBuilder;
using Microsoft.AspNetCore.Mvc;
using Services;
using ViewModels.ViewModels.FormBuilder;
using ViewModels;
using ViewModels.ViewModels.WorkFlow;
using DataLayer.Models.WorkFlows;

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
                    Id = x.id ,
                    entityId = x.data.Type  != "table" ? null : x.data.TypeId,
                    formId = x.data.Type != "form" ? null : x.data.TypeId,
                    Icon = x.data.Icon,
                    Name = x.data.Name,
                    Type = x.data.Type != "form" ? UnknownType.table : UnknownType.form ,
                    X = x.position.X,
                    Y = x.data.TypeId
                }).ToList(),
                Edges = workFlow.Edges.Select(x => new Edge()
                {
                    Id =  x.id,
                    Source = x.Source,
                    SourceHandle = x.SourceHandle,
                    Target = x.Target,
                    TargetHandle = x.TargetHandle
                }).ToList()
            };

            await _workFlowService.InsertWorFlow(result);
            await _workFlowService.SaveChangesAsync();
            return (new ResultViewModel { Data = result, Message = "عملیات با موفقیت انجام شد.", Status = true, StatusCode = 200 });
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
                    Id = x.id,
                    entityId = x.data.Type != "table" ? null : x.data.TypeId,
                    formId = x.data.Type != "form" ? null : x.data.TypeId,
                    Icon = x.data.Icon,
                    Name = x.data.Name,
                    Type = x.data.Type != "form" ?  UnknownType.table : UnknownType.form,
                    X = x.position.X,
                    Y = x.data.TypeId
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
            return (new ResultViewModel { Data = result, Message = "عملیات با موفقیت انجام شد.", Status = true, StatusCode = 200 });
        }

        // POST: api/form/delete  
        [HttpPost("remove")]
        public async Task<ResultViewModel> RemoveForm([FromBody] int workFlowId)
        {
            await _workFlowService.DeleteWorFlow(workFlowId);
            _workFlowService.SaveChangesAsync();
            return (new ResultViewModel { Data = null, Message = "عملیات با موفقیت انجام شد.", Status = true, StatusCode = 200 });
        }

        // GET: api/form/all  
        [HttpGet("all")]
        public async Task<ResultViewModel> GetAllForms()
        {
            var forms = await _workFlowService.GetAllWorFlows();
            return (new ResultViewModel { Data = forms, Message = "عملیات با موفقیت انجام شد.", Status = true, StatusCode = 200 });
        }

        // GET: api/form/{id}  
        [HttpGet("{workFlowId}")]
        public async Task<ResultViewModel> GetForm(int workFlowId)
        {
            var form = await _workFlowService.GetWorFlowIncNodesIncEdgesById(workFlowId);
            return (new ResultViewModel { Data = form, Message = "عملیات با موفقیت انجام شد.", Status = true, StatusCode = 200 });
        }

        // GET: api/form/{id}  
        [HttpGet("{workFlowId}/value")]
        public async Task<ResultViewModel> GetWorkflowValueById(int workFlowId, int userId)
        {
            var form = await _workFlowService.GetWorFlowValueById(workFlowId , userId);
            return (new ResultViewModel { Data = form, Message = "عملیات با موفقیت انجام شد.", Status = true, StatusCode = 200 });
        }

        // GET: api/form/{id}  
        [HttpGet("{workFlowId}/next/value")]
        public async Task<ResultViewModel> GetNextWorkflowValueById(int workFlowId, int userId)
        {
            var form = await _workFlowService.GetNextWorFlowValueById(workFlowId,  userId);
            return (new ResultViewModel { Data = form, Message = "عملیات با موفقیت انجام شد.", Status = true, StatusCode = 200 });
        }

        // GET: api/form/{id}  
        [HttpGet("{workFlowId}/last/value")]
        public async Task<ResultViewModel> GetLastWorkflowValueById(int workFlowId, int userId)
        {
            var form = await _workFlowService.GetLastWorFlowValueById(workFlowId,  userId);
            return (new ResultViewModel { Data = form, Message = "عملیات با موفقیت انجام شد.", Status = true, StatusCode = 200 });
        }
    }
}
