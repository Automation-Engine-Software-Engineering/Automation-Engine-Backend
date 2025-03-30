using Microsoft.AspNetCore.Mvc;
using Services;
using ViewModels;
using ViewModels.ViewModels.WorkFlow;
using DataLayer.Models.WorkFlows;
using AutomationEngine.ControllerAttributes;
using DataLayer.Models.WorkFlow;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;

namespace AutomationEngine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [CheckAccess]

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
            if (workFlow == null)
                throw new CustomException<WorkFlow>(new ValidationDto<WorkFlow>(false, "Workflow", "CorruptedWorkflow", null), 500);

            //transfer model
            var result = new WorkFlow();
            result.Name = workFlow.Name;
            result.Description = workFlow.Description;

            //is validation model
            if (result.Id != 0)
                throw new CustomException<WorkFlow>(new ValidationDto<WorkFlow>(false, "Workflow", "CorruptedWorkflow", result), 500);

            if (!(await _workFlowService.WorkFlowValidationAsync(result)).IsSuccess)
                throw new CustomException<WorkFlow>(new ValidationDto<WorkFlow>(false, "Workflow", "CorruptedWorkflow", result), 500);

            await _workFlowService.InsertWorFlowAsync(result);
            var saveResult = await _workFlowService.SaveChangesAsync();
            if (!saveResult.IsSuccess)
                throw new CustomException<string>(saveResult, 500);

            return (new ResultViewModel { Data = result, Message = new ValidationDto<WorkFlow>(true, "Success", "Success", result).GetMessage(200), Status = true, StatusCode = 200 });
        }


        // POST: api/{workFlowId}/insertNode  
        [HttpPost("setNodes")]
        public async Task<ResultViewModel> setNodes([FromBody] WorkFlowDto workFlow)
        {
            if (workFlow == null)
                throw new CustomException<WorkFlow>(new ValidationDto<WorkFlow>(false, "Workflow", "CorruptedWorkflow", null), 500);

            //transfer model
            var result = await _workFlowService.GetWorFlowByIdIncNodesAsync(workFlow.Id);

            //is validation model

            if (!(await _workFlowService.WorkFlowValidationAsync(result)).IsSuccess)
                throw new CustomException<WorkFlow>(new ValidationDto<WorkFlow>(false, "Workflow", "CorruptedWorkflow", result), 500);

            if (result.Nodes.Count != 0)
            {
                await _workFlowService.DeleteAllNodeOfWorFlowAsync(result.Id);
            }
            var saveResult = await _workFlowService.SaveChangesAsync();
            if (!saveResult.IsSuccess)
                throw new CustomException<string>(saveResult, 500);

            var nodes = workFlow.Nodes.Select(x => new Node()
            {
                Id = x.Id,
                Icon = x.Data.Icon,
                Name = x.Data.Name,
                Type = x.Data.Type == 1 ? UnknownType.form : UnknownType.dynamic,
                X = x.Position.X,
                Y = x.Position.Y,
                Width = x.Position.Width,
                Height = x.Position.Height
            }).ToList();

            workFlow.Edges.ForEach(x =>
            {
                var last = nodes.FirstOrDefault(xx => xx.Id == x.Source);
                nodes.FirstOrDefault(xx => xx.Id == x.Target).LastNodeId = last.Id;
            });

            result.Nodes = nodes;
            saveResult = await _workFlowService.SaveChangesAsync();
            if (!saveResult.IsSuccess)
                throw new CustomException<string>(saveResult, 500);

            workFlow.Edges.ForEach(x =>
            {
                var next = nodes.FirstOrDefault(xx => xx.Id == x.Target);
                nodes.FirstOrDefault(xx => xx.Id == x.Source).NextNodeId = next.Id;
            });

            result.Nodes = nodes;
            saveResult = await _workFlowService.SaveChangesAsync();
            if (!saveResult.IsSuccess)
                throw new CustomException<string>(saveResult, 500);


            return (new ResultViewModel { Data = result, Message = new ValidationDto<WorkFlow>(true, "Success", "Success", result).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // POST: api/form/update  
        [HttpPost("update")]
        public async Task<ResultViewModel> UpdateWorkflow([FromBody] WorkFlowDto workFlow)
        {
            if (workFlow == null)
                throw new CustomException<WorkFlow>(new ValidationDto<WorkFlow>(false, "Workflow", "CorruptedWorkflow", null), 500);


            var result = await _workFlowService.GetWorFlowByIdAsync(workFlow.Id);
            result.Name = workFlow.Name;
            result.Description = workFlow.Description;

            //is validation model
            if (result.Id == 0)
                throw new CustomException<WorkFlow>(new ValidationDto<WorkFlow>(false, "Workflow", "CorruptedWorkflow", result), 500);

            if (!(await _workFlowService.WorkFlowValidationAsync(result)).IsSuccess)
                throw new CustomException<WorkFlow>(new ValidationDto<WorkFlow>(false, "Workflow", "CorruptedWorkflow", result), 500);

            await _workFlowService.UpdateWorFlowAsync(result);
            var saveResult = await _workFlowService.SaveChangesAsync();
            if (!saveResult.IsSuccess)
                throw new CustomException<string>(saveResult, 500);

            return (new ResultViewModel { Data = result, Message = new ValidationDto<WorkFlow>(true, "Success", "Success", result).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // POST: api/form/delete  
        [HttpPost("remove")]
        public async Task<ResultViewModel> RemoveWorkflow([FromBody] int workFlowId)
        {
            if (workFlowId == 0)
                throw new CustomException<int>(new ValidationDto<int>(false, "Workflow", "NoWorkflowFound", workFlowId), 500);

            var ModelExist = await _workFlowService.IsWorkflowExistAsync(workFlowId);
            if (ModelExist == false)
                throw new CustomException<int>(new ValidationDto<int>(false, "Workflow", "NoWorkflowFound", workFlowId), 500);

            var fetchModel = await _workFlowService.GetWorFlowByIdAsync(workFlowId);
            if (!(await _workFlowService.WorkFlowValidationAsync(fetchModel)).IsSuccess)
                throw new CustomException<WorkFlow>(new ValidationDto<WorkFlow>(false, "Workflow", "CorruptedWorkflow", fetchModel), 500);

            await _workFlowService.DeleteWorFlowAsync(workFlowId);

            var saveResult = await _workFlowService.SaveChangesAsync();
            if (!saveResult.IsSuccess)
                throw new CustomException<string>(saveResult, 500);

            return (new ResultViewModel { Data = null, Message = new ValidationDto<WorkFlow>(true, "Success", "Success", fetchModel).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // GET: api/form/all  
        [HttpGet("all")]
        public async Task<ResultViewModel> GetAllWorkflows(int pageSize, int pageNumber)
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
        public async Task<ResultViewModel> GetWorkflow(int workFlowId)
        {
            if (workFlowId == 0)
                throw new CustomException<int>(new ValidationDto<int>(false, "Workflow", "NoWorkflowFound", workFlowId), 500);

            var ModelExist = await _workFlowService.IsWorkflowExistAsync(workFlowId);
            if (ModelExist == false)
                throw new CustomException<int>(new ValidationDto<int>(false, "Workflow", "NoWorkflowFound", workFlowId), 500);

            var fetchModel = await _workFlowService.GetWorFlowByIdIncNodesAsync(workFlowId);
            if (!(await _workFlowService.WorkFlowValidationAsync(fetchModel)).IsSuccess)
                throw new CustomException<WorkFlow>(new ValidationDto<WorkFlow>(false, "Workflow", "CorruptedWorkflow", fetchModel), 500);

            var dto = new WorkFlowDto();
            dto.Id = fetchModel.Id;
            dto.Name = fetchModel.Name;
            dto.Description = fetchModel.Description;
            dto.Nodes = fetchModel.Nodes.Select(x => new NodeDto()
            {
                Id = x.Id,
                Data = new Data() { Icon = x.Icon, Name = x.Name, Type = x.Type == UnknownType.form ? 1 : 2 },
                Position = new position() { X = x.X, Y = x.Y, Width = x.Width, Height = x.Height },
                Type = "custom"
            }).ToList();
            dto.Edges = new List<EdgeDto>();
            fetchModel.Nodes.ForEach(x =>
            {
                if (x.NextNode != null)
                    dto.Edges.Add(new EdgeDto()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Source = x.Id,
                        Target = x.NextNodeId,
                        SourceHandle = "b",
                        TargetHandle = "a",
                        animated = true,
                        type = "step",
                        markerEnd = new markerENd()
                        {
                            color = "#0099a5",
                            height = 20,
                            width = 20,
                            type = "arrow"
                        },
                        style = new style()
                        {
                            stroke = "#0099a5",
                            strokeWidth = 2
                        }
                    });
            });

            return (new ResultViewModel { Data = dto, Message = new ValidationDto<WorkFlow>(true, "Success", "Success", fetchModel).GetMessage(200), Status = true, StatusCode = 200 });
        }
    }
}
