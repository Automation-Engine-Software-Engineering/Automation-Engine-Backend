using Microsoft.AspNetCore.Mvc;
using Services;
using ViewModels;
using ViewModels.ViewModels.WorkFlow;
using DataLayer.Models.WorkFlow;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;

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
            var result = new WorkFlow();
            result.Name = workFlow.Name;
            result.Description = workFlow.Description;

            var nodes = workFlow.Nodes.Select(x => new Node()
            {
                Id = x.Id,
                Icon = x.Data.Icon,
                Name = x.Data.Name,
                Type = x.Type == 1 ? UnknownType.form : UnknownType.dynamic,
                X = x.Position.X,
                Y = x.Position.Y,
                Width = x.Position.Width,
                Height = x.Position.Height
            }).ToList();

            workFlow.Edges.ForEach(x =>
            {
                var last = nodes.FirstOrDefault(xx => xx.Id == x.Source);
                nodes.FirstOrDefault(xx => xx.Id == x.Target).LastNode = last;
                nodes.FirstOrDefault(xx => xx.Id == x.Target).LastNodeId = last.Id;

                var next = nodes.FirstOrDefault(xx => xx.Id == x.Target);
                nodes.FirstOrDefault(xx => xx.Id == x.Source).NextNode = next;
                nodes.FirstOrDefault(xx => xx.Id == x.Source).NextNodeId = next.Id;
            });

            result.Nodes = nodes;
            if ((await _workFlowService.WorkFlowValidationAsync(result)).IsSuccess)
                throw new CustomException<WorkFlow>(new ValidationDto<WorkFlow>(false, "Workflow", "CorruptedWorkflow", result), 500);

            await _workFlowService.InsertWorFlowAsync(result);
            await _workFlowService.SaveChangesAsync();
            return (new ResultViewModel { Data = result, Message = new ValidationDto<WorkFlow>(true, "Success", "Success", result).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // POST: api/form/update  
        [HttpPost("update")]
        public async Task<ResultViewModel> UpdateForm([FromBody] WorkFlowDto workFlow)
        {
            var result = new WorkFlow();
            result.Id = workFlow.Id;
            result.Name = workFlow.Name;
            result.Description = workFlow.Description;

            var nodes = workFlow.Nodes.Select(x => new Node()
            {
                Id = x.Id,
                Icon = x.Data.Icon,
                Name = x.Data.Name,
                Type = x.Type == 1 ? UnknownType.form : UnknownType.dynamic,
                X = x.Position.X,
                Y = x.Position.Y,
                Width = x.Position.Width,
                Height = x.Position.Height
            }).ToList();

            workFlow.Edges.ForEach(x =>
            {
                var last = nodes.FirstOrDefault(xx => xx.Id == x.Source);
                nodes.FirstOrDefault(xx => xx.Id == x.Target).LastNode = last;
                nodes.FirstOrDefault(xx => xx.Id == x.Target).LastNodeId = last.Id;

                var next = nodes.FirstOrDefault(xx => xx.Id == x.Target);
                nodes.FirstOrDefault(xx => xx.Id == x.Source).NextNode = next;
                nodes.FirstOrDefault(xx => xx.Id == x.Source).NextNodeId = next.Id;
            });

            result.Nodes = nodes;
            if ((await _workFlowService.WorkFlowValidationAsync(result)).IsSuccess)
                throw new CustomException<WorkFlow>(new ValidationDto<WorkFlow>(false, "Workflow", "CorruptedWorkflow", result), 500);

            await _workFlowService.UpdateWorFlowAsync(result);
            await _workFlowService.SaveChangesAsync();
            return (new ResultViewModel { Data = result, Message = new ValidationDto<WorkFlow>(true, "Success", "Success", result).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // POST: api/form/delete  
        [HttpPost("remove")]
        public async Task<ResultViewModel> RemoveForm([FromBody] int workFlowId)
        {
            if (workFlowId == 0)
                throw new CustomException<int>(new ValidationDto<int>(false, "Workflow", "NoWorkflowFound", workFlowId), 500);

            var ModelExist = await _workFlowService.IsWorkflowExistAsync(workFlowId);
            if (ModelExist == false)
                throw new CustomException<int>(new ValidationDto<int>(false, "Workflow", "NoWorkflowFound", workFlowId), 500);

            var fetchModel = await _workFlowService.GetWorFlowByIdAsync(workFlowId);
            if ((await _workFlowService.WorkFlowValidationAsync(fetchModel)).IsSuccess)
                throw new CustomException<WorkFlow>(new ValidationDto<WorkFlow>(false, "Workflow", "CorruptedWorkflow", fetchModel), 500);

            await _workFlowService.DeleteWorFlowAsync(workFlowId);
            _workFlowService.SaveChangesAsync();
            return (new ResultViewModel { Data = null, Message = new ValidationDto<WorkFlow>(true, "Success", "Success", fetchModel).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // GET: api/form/all  
        [HttpGet("all")]
        public async Task<ResultViewModel> GetAllForms(int pageSize, int pageNumber)
        {
            if (pageSize > 100)
                pageSize = 100;
            if (pageNumber < 1)
                pageNumber = 1;

            var workflows = await _workFlowService.GetAllWorFlowsAsync(pageSize, pageNumber);
            //is valid data
            if ((((pageSize * pageNumber) - workflows.TotalCount) > pageSize) && (pageSize * pageNumber) > workflows.TotalCount)
                throw new CustomException<ListDto<WorkFlow>>(new ValidationDto<ListDto<WorkFlow>>(false, "Workflow", "CorruptedWorkflow", workflows), 500);

            return (new ResultViewModel { Data = workflows.Data, ListNumber = workflows.ListNumber, ListSize = workflows.ListSize, TotalCount = workflows.TotalCount, Message = new ValidationDto<ListDto<WorkFlow>>(true, "Success", "Success", workflows).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // GET: api/form/{id}  
        [HttpGet("{workFlowId}")]
        public async Task<ResultViewModel> GetForm(int workFlowId)
        {
            if (workFlowId == 0)
                throw new CustomException<int>(new ValidationDto<int>(false, "Workflow", "NoWorkflowFound", workFlowId), 500);

            var ModelExist = await _workFlowService.IsWorkflowExistAsync(workFlowId);
            if (ModelExist == false)
                throw new CustomException<int>(new ValidationDto<int>(false, "Workflow", "NoWorkflowFound", workFlowId), 500);

            var fetchModel = await _workFlowService.GetWorFlowByIdAsync(workFlowId);
            if ((await _workFlowService.WorkFlowValidationAsync(fetchModel)).IsSuccess)
                throw new CustomException<WorkFlow>(new ValidationDto<WorkFlow>(false, "Workflow", "CorruptedWorkflow", fetchModel), 500);

            return (new ResultViewModel { Data = fetchModel, Message = new ValidationDto<WorkFlow>(true, "Success", "Success", fetchModel).GetMessage(200), Status = true, StatusCode = 200 });
        }
    }
}
