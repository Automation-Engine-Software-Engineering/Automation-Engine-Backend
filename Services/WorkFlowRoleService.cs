using DataLayer.DbContext;
using DataLayer.Models.MainEngine;
using DataLayer.Models.WorkFlows;
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
    public interface IWorkFlowRoleService
    {
        Task InsertWorFlowRole(Role_WorkFlow workFlow);
        Task UpdateWorFlowRole(Role_WorkFlow workFlow);
        Task DeleteWorFlowRole(int id);
        Task<Role_WorkFlow> GetWorFlowRoleById(int id);
        Task<ListDto<Role_WorkFlow>> GetAllWorFlowRoles(int pageSize, int pageNumber);
        Task<ListDto<Role_WorkFlow>> GetAllWorFlowRolesBuRoleId(int RoleId, int pageSize, int pageNumber);
        Task<bool> ExistAllWorFlowRolesBuRoleId(int RoleId, int WorkFlowId);
        Task<ValidationDto<Role_WorkFlow>> WorkFlowRoleValidation(Role_WorkFlow workFlowUser);
        Task<ValidationDto<string>> SaveChangesAsync();
    }

    public class WorkFlowRoleService : IWorkFlowRoleService
    {
        private readonly DataLayer.DbContext.Context _context;
        public WorkFlowRoleService(DataLayer.DbContext.Context context)
        {
            _context = context;
        }

        public async Task DeleteWorFlowRole(int id)
        {
            var fetchModel = await _context.Role_WorkFlows.FirstOrDefaultAsync(x => x.Id == id);
            _context.Role_WorkFlows.Remove(fetchModel);
        }

        public async Task<ListDto<Role_WorkFlow>> GetAllWorFlowRoles(int pageSize, int pageNumber)
        {
            //create query
            var query = _context.Role_WorkFlows;

            //get Value and count
            var count = query.Count();
            var result = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return new ListDto<Role_WorkFlow>(result, count, pageSize, pageNumber);
        }


        public async Task<ListDto<Role_WorkFlow>> GetAllWorFlowRolesBuRoleId(int RoleId, int pageSize, int pageNumber)
        {
            //create query
            var query = _context.Role_WorkFlows
            .Include(x => x.WorkFlow).Where(x => x.RoleId == RoleId);

            //get Value and count
            var count = query.Count();
            var result = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return new ListDto<Role_WorkFlow>(result, count, pageSize, pageNumber);
        }

        public async Task<Role_WorkFlow> GetWorFlowRoleById(int id)
        {
            var fetchModel = await _context.Role_WorkFlows.FirstAsync(x => x.Id == id);
            return fetchModel;
        }

        public async Task InsertWorFlowRole(Role_WorkFlow workFlow)
        {
            await _context.Role_WorkFlows.AddAsync(workFlow);
        }

        public async Task UpdateWorFlowRole(Role_WorkFlow workFlow)
        {
            var result = await _context.WorkFlow_User.FirstAsync(x => x.Id == workFlow.Id);

            var fetchModel = new Role_WorkFlow();
            fetchModel.WorkFlowId = workFlow.WorkFlowId;
            fetchModel.RoleId = workFlow.RoleId;
            _context.Update(fetchModel);
        }

        public async Task<ValidationDto<Role_WorkFlow>> WorkFlowRoleValidation(Role_WorkFlow workFlowUser)
        {
            if (workFlowUser == null) return new ValidationDto<Role_WorkFlow>(false, "WorkflowRole", "CorruptedWorkflowRole", workFlowUser);
            if (workFlowUser.WorkFlowId == 0 || workFlowUser.WorkFlow == null) return new ValidationDto<Role_WorkFlow>(false, "WorkflowRole", "CorruptedWorkflow", workFlowUser);
            if (workFlowUser.RoleId == 0 || workFlowUser.Role == null) return new ValidationDto<Role_WorkFlow>(false, "WorkflowRole", "CorruptedRole", workFlowUser);

            return new ValidationDto<Role_WorkFlow>(true, "Success", "Success", workFlowUser);
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
                return new ValidationDto<string>(false, "Form", "CorruptedForm", ex.Message);
            }
        }

        public async Task<bool> ExistAllWorFlowRolesBuRoleId(int RoleId, int WorkFlowId)
        {
            var result = await _context.Role_WorkFlows.AnyAsync(x => x.RoleId == RoleId && x.WorkFlowId == WorkFlowId);
            return result;
        }
    }
}
