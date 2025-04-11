using DataLayer.DbContext;
using Entities.Models.FormBuilder;
using Entities.Models.TableBuilder;
using Entities.Models.Workflows;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using Microsoft.EntityFrameworkCore;
using Tools.TextTools;

namespace Services
{
    public interface IWorkflowService
    {
        Task InsertWorFlowAsync(Workflow workflow);
        Task UpdateWorFlowAsync(Workflow workflow);
        Task DeleteWorFlowAsync(int id);
        Task DeleteAllNodeOfWorFlowAsync(int id);
        Task<Workflow> GetWorFlowByIdAsync(int id);
        Task<Workflow> GetWorFlowIncRolesById(int id);
        Task<Workflow> GetWorFlowByIdIncNodesAsync(int id);
        Task<ListDto<Workflow>> GetAllWorFlowsAsync(int pageSize, int pageNumber);
        Task<ValidationDto<Workflow>> WorkflowValidationAsync(Workflow workflow);
        Task<ValidationDto<string>> SaveChangesAsync();
        Task<bool> IsWorkflowExistAsync(int formId);
    }
    public class WorkflowService : IWorkflowService
    {
        private readonly DataLayer.DbContext.Context _context;
        public WorkflowService(DataLayer.DbContext.Context context)
        {
            _context = context;
        }

        public async Task DeleteWorFlowAsync(int id)
        {
            //initialize model
            var result = await _context.Workflow.Include(x => x.Nodes).FirstAsync(x => x.Id == id);


            //remove form
            _context.Remove(result);
        }

        public async Task DeleteAllNodeOfWorFlowAsync(int id)
        {
            //initialize model
            var result = await _context.Workflow.Include(x => x.Nodes).FirstAsync(x => x.Id == id);
            result.Nodes.ForEach(x => { x.LastNodeId = null; x.NextNodeId = null; x.NextNode = null; x.LastNode = null; });
            await _context.SaveChangesAsync();

            _context.Node.RemoveRange(result.Nodes);

        }


        public async Task<ListDto<Workflow>> GetAllWorFlowsAsync(int pageSize, int pageNumber)
        {
            //create query
            var query = _context.Workflow.Include(x => x.Nodes);

            //get Value and count
            var count = query.Count();
            var result = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return new ListDto<Workflow>(result, count, pageSize, pageNumber);

        }

        

        public async Task<Workflow> GetWorFlowByIdAsync(int id)
        {
            var result = await _context.Workflow.Include(x => x.Nodes).FirstAsync(x => x.Id == id);
            return result;
        }

        public async Task<Workflow> GetWorFlowByIdIncNodesAsync(int id)
        {
            var result = await _context.Workflow
            .Include(x => x.Nodes)
            .ThenInclude(x => x.Form)
            .FirstAsync(x => x.Id == id);
            return result;
        }

        public async Task InsertWorFlowAsync(Workflow workflow)
        {
            await _context.Workflow.AddAsync(workflow);
        }

        public async Task UpdateWorFlowAsync(Workflow workflow)
        {
            //initialize model
            var fetchModel = await _context.Workflow.FirstAsync(x => x.Id == workflow.Id);

            //transfer model
            fetchModel.Name = workflow.Name;
            fetchModel.Description = workflow.Description;
            fetchModel.Nodes = workflow.Nodes;

            _context.Workflow.Update(fetchModel);
        }

        public async Task<ValidationDto<Workflow>> WorkflowValidationAsync(Workflow workflow)
        {
            if (workflow == null) return new ValidationDto<Workflow>(false, "Workflow", "CorruptedWorkflow", workflow);
            if (workflow.Name == null || !workflow.Name.IsValidString()) return new ValidationDto<Workflow>(false, "Workflow", "CorruptedWorkflowName", workflow);
            if (workflow.Description == null || !workflow.Description.IsValidString()) return new ValidationDto<Workflow>(false, "Workflow", "CorruptedWorkflowDes", workflow);

            return new ValidationDto<Workflow>(true, "Success", "Success", workflow);
        }

        public async Task<ValidationDto<string>> SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                return new ValidationDto<string>(true, "Success", "Success", null);
            }
            catch (Exception ex)
            {
                return new ValidationDto<string>(false, "Workflow", "CorruptedWorkflow", ex.Message);
            }
        }
        public async Task<bool> IsWorkflowExistAsync(int workflowId)
        {
            //check model exist
            var isExist = await _context.Workflow.AnyAsync(x => x.Id == workflowId);

            //return model
            return isExist;
        }

        public async Task<Workflow> GetWorFlowIncRolesById(int id)
        {
            var result = await _context.Workflow.Include(x => x.Role_Workflows).FirstAsync(x => x.Id == id);
            return result;
        }
    }
}
