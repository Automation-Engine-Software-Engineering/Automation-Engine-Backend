using DataLayer.DbContext;
using DataLayer.Models.MainEngine;
using DataLayer.Models.WorkFlows;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;
using ViewModels.ViewModels.WorkFlow;

namespace Services
{
    public interface IWorkFlowRoleService
    {
        Task InsertWorFlowRole(Role_WorkFlow workFlow);
        Task<ListDto<Role_WorkFlow>> GetAllWorFlowRolesByRoleId(int RoleId, int pageSize, int pageNumber);
        Task InsertRengeWorFlowRole(List<Role_WorkFlow> workFlows);
        Task UpdateWorFlowRole(Role_WorkFlow workFlow);
        Task DeleteWorFlowRole(int id);
        Task<Role_WorkFlow> GetWorFlowRoleById(int id);
        Task<ListDto<Role_WorkFlow>> GetAllWorFlowRoles(int pageSize, int pageNumber);
        Task<ListDto<WorkflowAccess>> GetAllWorFlowRolesAndRole(int workFlowId, int pageSize, int pageNumber);
        Task<ListDto<WorkflowAccess>> GetAllWorFlowRolesAndWorkflow(int RoleId, int pageSize, int pageNumber);
        Task<bool> ExistAllWorFlowRolesBuRoleId(int RoleId, int WorkFlowId);
        Task<ValidationDto<Role_WorkFlow>> WorkFlowRoleValidation(Role_WorkFlow workFlowUser);
        Task ReplaceWorFlowRolesByRoleId(int roleId,List<int> workFlowIds);
        Task ReplaceWorFlowRolesByWorkFlowId(int workFlowId,List<int> roleIds);
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


        public async Task<ListDto<Role_WorkFlow>> GetAllWorFlowRolesByRoleId(int RoleId, int pageSize, int pageNumber)
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


        public async Task InsertRengeWorFlowRole(List<Role_WorkFlow> workFlows)
        {
            await _context.Role_WorkFlows.AddRangeAsync(workFlows);
        }
        public async Task ReplaceWorFlowRolesByRoleId(int roleId,List<int> workFlowIds)
        {
            var roleWorkflows = await _context.Role_WorkFlows.Where(x=>x.RoleId == roleId).ToListAsync();
            _context.Role_WorkFlows.RemoveRange(roleWorkflows);

            var newRoleWorkflows = workFlowIds.Select(x=>new Role_WorkFlow{
                RoleId = roleId,
                WorkFlowId = x
            }).ToList();
            await InsertRengeWorFlowRole(newRoleWorkflows);
        }

        public async Task ReplaceWorFlowRolesByWorkFlowId(int workFlowId,List<int> roleIds)
        {
            var roleWorkflows = await _context.Role_WorkFlows.Where(x=>x.WorkFlowId == workFlowId).ToListAsync();
            _context.Role_WorkFlows.RemoveRange(roleWorkflows);

            var newRoleWorkflows = roleIds.Select(x=>new Role_WorkFlow{
                RoleId = x,
                WorkFlowId = workFlowId
            }).ToList();
            await InsertRengeWorFlowRole(newRoleWorkflows);
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
            if (workFlowUser.WorkFlowId == 0) return new ValidationDto<Role_WorkFlow>(false, "WorkflowRole", "CorruptedWorkflow", workFlowUser);
            if (workFlowUser.RoleId == 0 ) return new ValidationDto<Role_WorkFlow>(false, "WorkflowRole", "CorruptedRole", workFlowUser);

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

        public async Task<ListDto<WorkflowAccess>> GetAllWorFlowRolesAndRole(int workFlowId, int pageSize, int pageNumber)
        {
            var Workflows = await _context.Roles.Include(x => x.role_WorkFlows)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .ToListAsync();

            var result = Workflows.Select(x => new WorkflowAccess() { Id = x.Id, Name = x.Name, IsAccess = x.role_WorkFlows.Any(x => x.WorkFlowId == workFlowId) ? true : false }).ToList();

            var list = new ListDto<WorkflowAccess>(result, result.Count, pageSize = pageSize, pageNumber = pageNumber);

            return list;
        }

        
        public async Task<ListDto<WorkflowAccess>> GetAllWorFlowRolesAndWorkflow(int roleId, int pageSize, int pageNumber)
        {
            var Workflows = await _context.WorkFlow.Include(x => x.Role_WorkFlows)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .ToListAsync();

            var result = Workflows.Select(x => new WorkflowAccess() { Id = x.Id, Name = x.Name, IsAccess = x.Role_WorkFlows.Any(x => x.RoleId == roleId) ? true : false }).ToList();

            var list = new ListDto<WorkflowAccess>(result, result.Count, pageSize = pageSize, pageNumber = pageNumber);

            return list;
        }
    }
}
