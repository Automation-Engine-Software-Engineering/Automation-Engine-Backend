using DataLayer.DbContext;
using Entities.Models.Enums;
using Entities.Models.Workflows;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace Services
{
    public interface IWorkflowUserService
    {
        Task InsertWorkflowUser(Workflow_User workflow);
        Task UpdateWorkflowUser(Workflow_User workflow);
        Task DeleteWorkflowUser(int id);
        Task<Workflow_User> GetWorkflowUserById(int id);
        Task<ListDto<Workflow_User>> GetAllWorkflowUsers(int pageSize, int pageNumber);
        CustomException WorkflowValidation(Workflow_User workflowUser);
        Task<Workflow_User> GetWorkflowUserByWorkflowAndUserId(int WorkflowId, int userId);
        Task SaveChangesAsync();
    }

    public class WorkflowUserService : IWorkflowUserService
    {
        private readonly Context _context;
        public WorkflowUserService(Context context)
        {
            _context = context;
        }
        public async Task DeleteWorkflowUser(int id)
        {
            var fetchModel = await _context.Workflow_User.FirstOrDefaultAsync(x => x.Id == id);
            _context.Workflow_User.Remove(fetchModel);
        }

        public async Task<ListDto<Workflow_User>> GetAllWorkflowUsers(int pageSize, int pageNumber)
        {
            //create query
            var query = _context.Workflow_User;

            //get Value and count
            var count = await query.CountAsync();
            var result = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return new ListDto<Workflow_User>(result, count, pageSize, pageNumber);
        }

        public async Task<Workflow_User> GetWorkflowUserById(int id)
        {
            var fetchModel = await _context.Workflow_User.Include(x => x.Workflow)
            .ThenInclude(x => x.Nodes)
            .FirstAsync(x => x.Id == id);
            return fetchModel;
        }

        public async Task<Workflow_User> GetWorkflowUserByWorkflowAndUserId(int WorkflowId, int userId)
        {
            var fetchModel = await _context.Workflow_User
            .Include(x => x.Workflow).FirstOrDefaultAsync(x => x.UserId == userId && x.WorkflowId == WorkflowId);
            return fetchModel;
        }

        public async Task InsertWorkflowUser(Workflow_User workflow)
        {
            await _context.Workflow_User.AddAsync(workflow);
        }

        public async Task UpdateWorkflowUser(Workflow_User workflow)
        {
            var result = await _context.Workflow_User.FirstAsync(x => x.Id == workflow.Id);

            var fetchModel = new Workflow_User();
            fetchModel.WorkflowState = workflow.WorkflowState;
            fetchModel.UserId = workflow.UserId;
            fetchModel.WorkflowId = workflow.WorkflowId;
            _context.Update(fetchModel);
        }

        public CustomException WorkflowValidation(Workflow_User workflowUser)
        {
            var invalidValidation = new CustomException("Property", "CorruptedProperty", workflowUser);
            if (workflowUser == null) return invalidValidation;
            if (workflowUser.UserId == 0) return invalidValidation;
            if (workflowUser.WorkflowState == null) return invalidValidation;
            if (workflowUser.Workflow == null) return invalidValidation;
            return new CustomException("Success", "Success", workflowUser);
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
