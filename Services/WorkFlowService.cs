using DataLayer.DbContext;
using Entities.Models.Enums;
using Entities.Models.FormBuilder;
using Entities.Models.TableBuilder;
using Entities.Models.Workflows;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Tools.TextTools;

namespace Services
{
    public interface IWorkflowService
    {
        Task InsertWorkflowAsync(Workflow workflow);
        Task UpdateWorkflowAsync(Workflow workflow);
        Task DeleteWorkflowAsync(int id);
        Task DeleteAllNodeOfWorkflowAsync(int id);
        Task<Workflow> GetWorkflowByIdAsync(int id);
        Task<Workflow> GetWorkflowIncRolesById(int id);
        Task<Workflow> GetWorkflowByIdIncNodesAsync(int id);
        Task<ListDto<Workflow>> GetAllWorkflowsAsync(int pageSize, int pageNumber);
        CustomException WorkflowValidation(Workflow workflow);
        Task SaveChangesAsync();
        Task<ListDto<Node>> GetAllNodsAsync(int pageSize, int pageNumber);
        Task<bool> IsWorkflowExistAsync(int formId);
        Task<Node> GetNodByIdAsync(string NodeId);
    }
    public class WorkflowService : IWorkflowService
    {
        private readonly Context _context;
        public WorkflowService(Context context)
        {
            _context = context;
        }

        public async Task DeleteWorkflowAsync(int id)
        {
            //initialize model
            var result = await _context.Workflow.Include(x => x.Nodes).FirstAsync(x => x.Id == id);


            //remove form
            _context.Remove(result);
        }

        public async Task DeleteAllNodeOfWorkflowAsync(int id)
        {
            //initialize model
            var result = await _context.Workflow.Include(x => x.Nodes).FirstAsync(x => x.Id == id);
            result.Nodes.ForEach(x => { x.PreviousNodeId = null; x.NextNodeId = null; x.NextNode = null; x.PreviousNode = null; });
            await _context.SaveChangesAsync();

            _context.Node.RemoveRange(result.Nodes);

        }


        public async Task<ListDto<Workflow>> GetAllWorkflowsAsync(int pageSize, int pageNumber)
        {
            //create query
            var query = _context.Workflow.Include(x => x.Nodes);

            //get Value and count
            var count = await query.CountAsync();
            var result = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return new ListDto<Workflow>(result, count, pageSize, pageNumber);

        }



        public async Task<Workflow> GetWorkflowByIdAsync(int id)
        {
            var result = await _context.Workflow.Include(x => x.Nodes).FirstAsync(x => x.Id == id);
            return result;
        }

        public async Task<Workflow> GetWorkflowByIdIncNodesAsync(int id)
        {
            var result = await _context.Workflow
            .Include(x => x.Nodes)
            .ThenInclude(x => x.Form)
            .FirstAsync(x => x.Id == id);
            return result;
        }

        public async Task InsertWorkflowAsync(Workflow workflow)
        {
            await _context.Workflow.AddAsync(workflow);
        }

        public async Task UpdateWorkflowAsync(Workflow workflow)
        {
            //initialize model
            var fetchModel = await _context.Workflow.FirstAsync(x => x.Id == workflow.Id);

            //transfer model
            fetchModel.Name = workflow.Name;
            fetchModel.Description = workflow.Description;
            fetchModel.Nodes = workflow.Nodes;

            _context.Workflow.Update(fetchModel);
        }

        public CustomException WorkflowValidation(Workflow workflow)
        {
            var invalidValidation = new CustomException("Property", "CorruptedProperty", workflow);
            if (workflow == null) return invalidValidation;
            if (workflow.Description == null || !workflow.Description.IsValidString()) return invalidValidation;
            if (workflow.Name.IsNullOrEmpty() || !workflow.Name.IsValidString()) return invalidValidation;

            return new CustomException("Success", "Success", workflow);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task<bool> IsWorkflowExistAsync(int workflowId)
        {
            //check model exist
            var isExist = await _context.Workflow.AnyAsync(x => x.Id == workflowId);

            return isExist;
        }

        public async Task<Workflow> GetWorkflowIncRolesById(int id)
        {
            var result = await _context.Workflow.Include(x => x.Role_Workflows).FirstAsync(x => x.Id == id);
            return result;
        }

        public async Task<ListDto<Node>> GetAllNodsAsync(int pageSize, int pageNumber)
        {
            //create query
            var query = _context.Node;

            //get Value and count
            var count = await query.CountAsync();
            var result = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return new ListDto<Node>(result, count, pageSize, pageNumber);
        }

        public async Task<Node> GetNodByIdAsync(string NodeId)
        {
           var result = await _context.Node.Include(x => x.NextNode).FirstAsync(x => x.Id == NodeId);
            return result;
        }
    }
}
