using DataLayer.Context;
using DataLayer.Models.WorkFlow;
using FrameWork.Model.DTO;
using Microsoft.EntityFrameworkCore;
using Tools.TextTools;

namespace Services
{
    public interface IWorkFlowService
    {
        Task InsertWorFlowAsync(WorkFlow workFlow);
        Task UpdateWorFlowAsync(WorkFlow workFlow);
        Task DeleteWorFlowAsync(int id);
        Task<WorkFlow> GetWorFlowByIdAsync(int id);
        Task<ListDto<WorkFlow>> GetAllWorFlowsAsync(int pageSize, int pageNumber);
        Task<ValidationDto<WorkFlow>> WorkFlowValidationAsync(WorkFlow workFlow);
        Task<ValidationDto<string>> SaveChangesAsync();
    }
    public class WorkFlowService : IWorkFlowService
    {
        private readonly Context _context;
        public WorkFlowService(Context context)
        {
            _context = context;
        }

        public async Task DeleteWorFlowAsync(int id)
        {
            //initialize model
            var result = await _context.WorkFlow.FirstAsync(x => x.Id == id);


            //remove form
            _context.Remove(result);
        }

        public async Task<ListDto<WorkFlow>> GetAllWorFlowsAsync(int pageSize, int pageNumber)
        {
            //create query
            var query = _context.WorkFlow.Include(x => x.Nodes);

            //get Value and count
            var count = query.Count();
            var result = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return new ListDto<WorkFlow>(result, count, pageSize, pageNumber);

        }

        public async Task<WorkFlow> GetWorFlowByIdAsync(int id)
        {
            var result = await _context.WorkFlow.Include(x => x.Nodes).FirstAsync(x => x.Id == id);
            return result;
        }

        public async Task InsertWorFlowAsync(WorkFlow workFlow)
        {
            await _context.WorkFlow.AddAsync(workFlow);
        }

        public async Task UpdateWorFlowAsync(WorkFlow workFlow)
        {
            //initialize model
            var fetchModel = await _context.WorkFlow.FirstAsync(x => x.Id == workFlow.Id);

            //transfer model
            fetchModel.Name = workFlow.Name;
            fetchModel.Description = workFlow.Description;
            fetchModel.Nodes = workFlow.Nodes;

            _context.WorkFlow.Update(fetchModel);
        }

        public async Task<ValidationDto<WorkFlow>> WorkFlowValidationAsync(WorkFlow workFlow)
        {
            if (workFlow == null) return new ValidationDto<WorkFlow>(false, "Workflow", "CorruptedWorkflow", workFlow);
            if (workFlow.Name == null || !workFlow.Name.IsValidString()) return new ValidationDto<WorkFlow>(false, "Workflow", "CorruptedWorkflowName", workFlow);
            if (workFlow.Description == null || !workFlow.Description.IsValidString()) return new ValidationDto<WorkFlow>(false, "Workflow", "CorruptedWorkflowDes", workFlow);

            return new ValidationDto<WorkFlow>(true, "Success", "Success", workFlow);
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
    }
}
