using Microsoft.AspNetCore.Mvc;
using Services;
using ViewModels;
using ViewModels.ViewModels.Workflow;
using Entities.Models.Workflows;
using AutomationEngine.ControllerAttributes;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using Entities.Models.Enums;

namespace AutomationEngine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [CheckAccess]

    public class WorkflowController : Controller
    {
        private readonly IWorkflowService _workflowService;
        private readonly IWorkflowUserService _workflowUserService;

        public WorkflowController(IWorkflowService workflowService, IWorkflowUserService workflowUserService)
        {
            _workflowService = workflowService;
            _workflowUserService = workflowUserService;
        }

        // POST: api/form/create  
        [HttpPost("create")]
        public async Task<ResultViewModel> CreateWorkflow([FromBody] WorkflowDto workflow)
        {
            if (workflow == null)
                throw new CustomException<Workflow>(new ValidationDto<Workflow>(false, "Workflow", "CorruptedWorkflow", null), 500);

            //transfer model
            var result = new Workflow();
            result.Name = workflow.Name;
            result.Description = workflow.Description;

            //is validation model
            if (result.Id != 0)
                throw new CustomException<Workflow>(new ValidationDto<Workflow>(false, "Workflow", "CorruptedWorkflow", result), 500);

            if (!(await _workflowService.WorkflowValidationAsync(result)).IsSuccess)
                throw new CustomException<Workflow>(new ValidationDto<Workflow>(false, "Workflow", "CorruptedWorkflow", result), 500);

            await _workflowService.InsertWorkflowAsync(result);
            var saveResult = await _workflowService.SaveChangesAsync();
            if (!saveResult.IsSuccess)
                throw new CustomException<string>(saveResult, 500);

            return (new ResultViewModel { Data = result, Message = new ValidationDto<Workflow>(true, "Success", "Success", result).GetMessage(200), Status = true, StatusCode = 200 });
        }


        // POST: api/{workflowId}/insertNode  
        [HttpPost("setNodes")]
        public async Task<ResultViewModel> setNodes([FromBody] WorkflowDto workflow)
        {
            if (workflow == null)
                throw new CustomException<Workflow>(new ValidationDto<Workflow>(false, "Workflow", "CorruptedWorkflow", null), 500);

            //transfer model
            var result = await _workflowService.GetWorkflowByIdIncNodesAsync(workflow.Id);

            //is validation model

            if (!(await _workflowService.WorkflowValidationAsync(result)).IsSuccess)
                throw new CustomException<Workflow>(new ValidationDto<Workflow>(false, "Workflow", "CorruptedWorkflow", result), 500);

            if (result.Nodes.Count != 0)
            {
                await _workflowService.DeleteAllNodeOfWorkflowAsync(result.Id);
            }
            var saveResult = await _workflowService.SaveChangesAsync();
            if (!saveResult.IsSuccess)
                throw new CustomException<string>(saveResult, 500);

            var nodes = workflow.Nodes?.Select(x => new Node()
            {
                Id = x.Id,
                Icon = x.Data.Icon,
                Name = x.Data.Name,
                Type = x.Data.Type == 1 ? UnknownType.Form : UnknownType.Dynamic,
                X = x.Position.X,
                Y = x.Position.Y,
                Width = x.Position.Width,
                Height = x.Position.Height,
                FormId = x.Data.FormId == null ? null : x.Data.FormId,
                DllName = x.Data.DllName == null ? null : x.Data.DllName
            }).ToList();
            if (nodes != null && nodes.Any())
            {
                workflow.Edges?.ForEach(x =>
                {
                    var last = nodes?.FirstOrDefault(xx => xx.Id == x.Source);
                    if (nodes != null)
                        nodes.First(xx => xx.Id == x.Target).PreviousNodeId = last?.Id;
                });

                result.Nodes = nodes;
            }

            saveResult = await _workflowService.SaveChangesAsync();
            if (!saveResult.IsSuccess)
                throw new CustomException<string>(saveResult, 500);

            workflow.Edges.ForEach(x =>
            {
                var next = nodes.FirstOrDefault(xx => xx.Id == x.Target);
                nodes.FirstOrDefault(xx => xx.Id == x.Source).NextNodeId = next.Id;
            });

            result.Nodes = nodes;
            saveResult = await _workflowService.SaveChangesAsync();
            if (!saveResult.IsSuccess)
                throw new CustomException<string>(saveResult, 500);


            return (new ResultViewModel { Data = result, Message = new ValidationDto<Workflow>(true, "Success", "Success", result).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // POST: api/form/update  
        [HttpPost("update")]
        public async Task<ResultViewModel> UpdateWorkflow([FromBody] WorkflowDto workflow)
        {
            if (workflow == null)
                throw new CustomException<Workflow>(new ValidationDto<Workflow>(false, "Workflow", "CorruptedWorkflow", null), 500);

            var result = await _workflowService.GetWorkflowByIdAsync(workflow.Id);
            result.Name = workflow.Name;
            result.Description = workflow.Description;

            //is validation model
            if (result.Id == 0)
                throw new CustomException<Workflow>(new ValidationDto<Workflow>(false, "Workflow", "CorruptedWorkflow", result), 500);

            if (!(await _workflowService.WorkflowValidationAsync(result)).IsSuccess)
                throw new CustomException<Workflow>(new ValidationDto<Workflow>(false, "Workflow", "CorruptedWorkflow", result), 500);

            await _workflowService.UpdateWorkflowAsync(result);
            var saveResult = await _workflowService.SaveChangesAsync();
            if (!saveResult.IsSuccess)
                throw new CustomException<string>(saveResult, 500);

            return (new ResultViewModel { Data = result, Message = new ValidationDto<Workflow>(true, "Success", "Success", result).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // POST: api/form/delete  
        [HttpPost("remove")]
        public async Task<ResultViewModel> RemoveWorkflow([FromBody] int workflowId)
        {
            if (workflowId == 0)
                throw new CustomException<int>(new ValidationDto<int>(false, "Workflow", "WorkflowNotfound", workflowId), 500);

            var ModelExist = await _workflowService.IsWorkflowExistAsync(workflowId);
            if (ModelExist == false)
                throw new CustomException<int>(new ValidationDto<int>(false, "Workflow", "WorkflowNotfound", workflowId), 500);

            var fetchModel = await _workflowService.GetWorkflowByIdAsync(workflowId);
            if (!(await _workflowService.WorkflowValidationAsync(fetchModel)).IsSuccess)
                throw new CustomException<Workflow>(new ValidationDto<Workflow>(false, "Workflow", "CorruptedWorkflow", fetchModel), 500);

            await _workflowService.DeleteWorkflowAsync(workflowId);

            var saveResult = await _workflowService.SaveChangesAsync();
            if (!saveResult.IsSuccess)
                throw new CustomException<string>(saveResult, 500);

            return (new ResultViewModel { Data = fetchModel, Message = new ValidationDto<Workflow>(true, "Success", "Success", fetchModel).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // GET: api/form/all  
        [HttpGet("all")]
        public async Task<ResultViewModel> GetAllWorkflows(int pageSize, int pageNumber)
        {
            if (pageSize > 100)
                pageSize = 100;
            if (pageNumber < 1)
                pageNumber = 1;

            var workflows = await _workflowService.GetAllWorkflowsAsync(pageSize, pageNumber);
            //is valid data
            if ((((pageSize * pageNumber) - workflows.TotalCount) > pageSize) && (pageSize * pageNumber) > workflows.TotalCount)
                throw new CustomException<ListDto<Workflow>>(new ValidationDto<ListDto<Workflow>>(false, "Workflow", "CorruptedWorkflow", workflows), 500);

            return (new ResultViewModel { Data = workflows.Data, ListNumber = workflows.ListNumber, ListSize = workflows.ListSize, TotalCount = workflows.TotalCount, Message = new ValidationDto<ListDto<Workflow>>(true, "Success", "Success", workflows).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // GET: api/form/all  
        [HttpGet("nodes/all")]
        public async Task<ResultViewModel> GetAllNods(int pageSize, int pageNumber)
        {
            if (pageSize > 100)
                pageSize = 100;
            if (pageNumber < 1)
                pageNumber = 1;

            var workflows = await _workflowService.GetAllNodsAsync(pageSize, pageNumber);
            //is valid data
            if ((((pageSize * pageNumber) - workflows.TotalCount) > pageSize) && (pageSize * pageNumber) > workflows.TotalCount)
                throw new CustomException<ListDto<Node>>(new ValidationDto<ListDto<Node>>(false, "Workflow", "CorruptedWorkflow", workflows), 500);

            return (new ResultViewModel { Data = workflows.Data, ListNumber = workflows.ListNumber, ListSize = workflows.ListSize, TotalCount = workflows.TotalCount, Message = new ValidationDto<ListDto<Node>>(true, "Success", "Success", workflows).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // GET: api/form/{id}  
        [HttpGet("{workflowId}")]
        public async Task<ResultViewModel> GetWorkflow(int workflowId)
        {
            if (workflowId == 0)
                throw new CustomException<int>(new ValidationDto<int>(false, "Workflow", "WorkflowNotfound", workflowId), 500);

            var ModelExist = await _workflowService.IsWorkflowExistAsync(workflowId);
            if (ModelExist == false)
                throw new CustomException<int>(new ValidationDto<int>(false, "Workflow", "WorkflowNotfound", workflowId), 500);

            var fetchModel = await _workflowService.GetWorkflowByIdIncNodesAsync(workflowId);
            if (!(await _workflowService.WorkflowValidationAsync(fetchModel)).IsSuccess)
                throw new CustomException<Workflow>(new ValidationDto<Workflow>(false, "Workflow", "CorruptedWorkflow", fetchModel), 500);

            var dto = new WorkflowDto();
            dto.Id = fetchModel.Id;
            dto.Name = fetchModel.Name;
            dto.Description = fetchModel.Description;
            dto.Nodes = fetchModel.Nodes.Select(x => new NodeDto()
            {
                Id = x.Id,
                Data = new Data() { Icon = x.Icon, Name = x.Name, Type = x.Type == UnknownType.Form ? 1 : 2, FormId = x.FormId },
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

            return (new ResultViewModel { Data = dto, Message = new ValidationDto<Workflow>(true, "Success", "Success", fetchModel).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // GET: api/nodeState  
        [HttpGet("nodeState")]
        public async Task<ResultViewModel> GetNodeState(int WorkflowUserId)
        {
            if (WorkflowUserId == 0)
                throw new CustomException<int>(new ValidationDto<int>(false, "Workflow", "WorkflowNotfound", WorkflowUserId), 500);

            var workflowUser = await _workflowUserService.GetWorkflowUserById(WorkflowUserId);
            if (workflowUser == null)
                throw new CustomException<int>(new ValidationDto<int>(false, "Workflow", "WorkflowNotfound", WorkflowUserId), 500);

            var workflow = await _workflowService.GetWorkflowByIdIncNodesAsync(workflowUser.WorkflowId);
            if (workflow == null)
                throw new CustomException<int>(new ValidationDto<int>(false, "Workflow", "WorkflowNotfound", WorkflowUserId), 500);

            var node = workflow.Nodes.FirstOrDefault(n => n.Id == workflowUser.WorkflowState);

            return (new ResultViewModel { Data = node, Message = new ValidationDto<Node>(true, "Success", "Success", node).GetMessage(200), Status = true, StatusCode = 200 });
        }


        // GET: api/nodeMove  
        [HttpGet("nodeMove")]
        public async Task<ResultViewModel> NodeMove(int WorkflowUserId, int state, string? nodeId, int? newWorkflowUserId)
        {
            if (WorkflowUserId == 0)
                throw new CustomException<int>(new ValidationDto<int>(false, "Workflow", "WorkflowNotfound", WorkflowUserId), 500);

            var workflowUser = await _workflowUserService.GetWorkflowUserById(WorkflowUserId);
            if (workflowUser == null)
                throw new CustomException<int>(new ValidationDto<int>(false, "Workflow", "WorkflowNotfound", WorkflowUserId), 500);

            var workflow = await _workflowService.GetWorkflowByIdIncNodesAsync(workflowUser.WorkflowId);
            if (workflow == null)
                throw new CustomException<int>(new ValidationDto<int>(false, "Workflow", "WorkflowNotfound", WorkflowUserId), 500);

            var node =  await _workflowService.GetNodByIdAsync(workflowUser.WorkflowState);
            if (node == null)
                throw new CustomException<int>(new ValidationDto<int>(false, "Workflow", "NodeNotFound", WorkflowUserId), 500);

            var resultWorkflowUserId = WorkflowUserId;
            if (state == 1)
            {
                if (node.NextNodeId == null || node.NextNode == null)
                    throw new CustomException<int>(new ValidationDto<int>(false, "Workflow", "CorruptedWorkflowNextNode", WorkflowUserId), 500);

                workflowUser.WorkflowState = node.NextNode.Id;
            }
            else if (state == 2)
            {
                if (node.PreviousNodeId == null || node.PreviousNode == null)
                    throw new CustomException<int>(new ValidationDto<int>(false, "Workflow", "CorruptedWorkflowPreviousNode", WorkflowUserId), 500);

                workflowUser.WorkflowState = node.PreviousNode.Id;
            }
            else if (state == 3)
                await _workflowUserService.DeleteWorkflowUser(workflowUser.Id);
            else if (state == 4)
            {
                if (nodeId == null)
                {
                    throw new CustomException<int>(new ValidationDto<int>(false, "Workflow", "CorruptedWorkflowPreviousNode", WorkflowUserId), 500);
                }
                var linkNode = await _workflowService.GetNodByIdAsync(nodeId);
                if (linkNode == null)
                {
                    throw new CustomException<int>(new ValidationDto<int>(false, "Workflow", "CorruptedWorkflowPreviousNode", WorkflowUserId), 500);
                }

                if (newWorkflowUserId == null)
                {
                    workflowUser.WorkflowState = linkNode.Id;
                }
                else
                {
                    var workflowUserModel = await _workflowUserService.GetWorkflowUserById(newWorkflowUserId.Value);
                    workflowUserModel.WorkflowState = linkNode.Id;
                    resultWorkflowUserId = newWorkflowUserId.Value ;
                }
            }

            await _workflowUserService.SaveChangesAsync();
            return (new ResultViewModel { Data = resultWorkflowUserId, Message = new ValidationDto<Node>(true, "Success", "Success", node).GetMessage(200), Status = true, StatusCode = 200 });
        }
    }
}
