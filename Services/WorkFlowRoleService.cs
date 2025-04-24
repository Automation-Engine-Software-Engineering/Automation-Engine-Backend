using DataLayer.DbContext;
using Entities.Models.MainEngine;
using Entities.Models.Workflows;
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
using ViewModels.ViewModels.Workflow;

namespace Services
{
    public interface IWorkflowRoleService
    {
        Task InsertWorkflowRole(Role_Workflow workflow);
        Task<ListDto<Role_Workflow>> GetAllWorkflowRolesByRoleId(int RoleId, int pageSize, int pageNumber);
        Task InsertRangeWorkflowRole(List<Role_Workflow> workflows);
        Task UpdateWorkflowRole(Role_Workflow workflow);
        Task DeleteWorkflowRole(int id);
        Task<Role_Workflow> GetWorkflowRoleById(int id);
        Task<ListDto<Role_Workflow>> GetAllWorkflowRoles(int pageSize, int pageNumber);
        Task<ListDto<IsAccessModel>> GetRolesAccessByWorkflowId(int workflowId, int pageSize, int pageNumber);
        Task<ListDto<IsAccessModel>> GetWorkflowsAccessByRoleId(int RoleId, int pageSize, int pageNumber);
        Task<bool> ExistAllWorkflowRolesBuRoleId(int RoleId, int WorkflowId);
        Task<ValidationDto<Role_Workflow>> WorkflowRoleValidation(Role_Workflow workflowUser);
        Task ReplaceWorkflowRolesByRoleId(int roleId, List<int> workflowIds);
        Task ReplaceWorkflowRolesByWorkflowId(int workflowId, List<int> roleIds);
        Task<ValidationDto<string>> SaveChangesAsync();
    }

    public class WorkflowRoleService : IWorkflowRoleService
    {
        private readonly DataLayer.DbContext.Context _context;
        public WorkflowRoleService(DataLayer.DbContext.Context context)
        {
            _context = context;
        }

        public async Task DeleteWorkflowRole(int id)
        {
            var fetchModel = await _context.Role_Workflows.FirstOrDefaultAsync(x => x.Id == id);
            _context.Role_Workflows.Remove(fetchModel);
        }

        public async Task<ListDto<Role_Workflow>> GetAllWorkflowRoles(int pageSize, int pageNumber)
        {
            //create query
            var query = _context.Role_Workflows;

            //get Value and count
            var count = await query.CountAsync();
            var result = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return new ListDto<Role_Workflow>(result, count, pageSize, pageNumber);
        }


        public async Task<ListDto<Role_Workflow>> GetAllWorkflowRolesByRoleId(int RoleId, int pageSize, int pageNumber)
        {
            //create query
            var query = _context.Role_Workflows
            .Include(x => x.Workflow).Where(x => x.RoleId == RoleId);

            //get Value and count
            var count = await query.CountAsync();
            var result = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return new ListDto<Role_Workflow>(result, count, pageSize, pageNumber);
        }

        public async Task<Role_Workflow> GetWorkflowRoleById(int id)
        {
            var fetchModel = await _context.Role_Workflows.FirstAsync(x => x.Id == id);
            return fetchModel;
        }

        public async Task InsertWorkflowRole(Role_Workflow workflow)
        {
            await _context.Role_Workflows.AddAsync(workflow);
        }


        public async Task InsertRangeWorkflowRole(List<Role_Workflow> workflows)
        {
            await _context.Role_Workflows.AddRangeAsync(workflows);
        }
        public async Task ReplaceWorkflowRolesByRoleId(int roleId, List<int> workflowIds)
        {
            var roleWorkflows = await _context.Role_Workflows.Where(x => x.RoleId == roleId).ToListAsync();
            _context.Role_Workflows.RemoveRange(roleWorkflows);

            var newRoleWorkflows = workflowIds.Select(x => new Role_Workflow
            {
                RoleId = roleId,
                WorkflowId = x
            }).ToList();
            await InsertRangeWorkflowRole(newRoleWorkflows);
        }

        public async Task ReplaceWorkflowRolesByWorkflowId(int workflowId, List<int> roleIds)
        {
            var roleWorkflows = await _context.Role_Workflows.Where(x => x.WorkflowId == workflowId).ToListAsync();
            _context.Role_Workflows.RemoveRange(roleWorkflows);

            var newRoleWorkflows = roleIds.Select(x => new Role_Workflow
            {
                RoleId = x,
                WorkflowId = workflowId
            }).ToList();
            await InsertRangeWorkflowRole(newRoleWorkflows);
        }

        public async Task UpdateWorkflowRole(Role_Workflow workflow)
        {
            var result = await _context.Workflow_User.FirstAsync(x => x.Id == workflow.Id);

            var fetchModel = new Role_Workflow();
            fetchModel.WorkflowId = workflow.WorkflowId;
            fetchModel.RoleId = workflow.RoleId;
            _context.Update(fetchModel);
        }

        public async Task<ValidationDto<Role_Workflow>> WorkflowRoleValidation(Role_Workflow workflowUser)
        {
            if (workflowUser == null) return new ValidationDto<Role_Workflow>(false, "WorkflowRole", "CorruptedWorkflowRole", workflowUser);
            if (workflowUser.WorkflowId == 0) return new ValidationDto<Role_Workflow>(false, "WorkflowRole", "CorruptedWorkflow", workflowUser);
            if (workflowUser.RoleId == 0) return new ValidationDto<Role_Workflow>(false, "WorkflowRole", "CorruptedRole", workflowUser);

            return new ValidationDto<Role_Workflow>(true, "Success", "Success", workflowUser);
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

        public async Task<bool> ExistAllWorkflowRolesBuRoleId(int RoleId, int WorkflowId)
        {
            var result = await _context.Role_Workflows.AnyAsync(x => x.RoleId == RoleId && x.WorkflowId == WorkflowId);
            return result;
        }

        public async Task<ListDto<IsAccessModel>> GetRolesAccessByWorkflowId(int workflowId, int pageSize, int pageNumber)
        {
            var count = await _context.Roles.CountAsync();
            var Workflows = await _context.Roles.Include(x => x.role_Workflows)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .ToListAsync();

            var result = Workflows.Select(x => new IsAccessModel() { Id = x.Id, Name = x.Name, IsAccess = x.role_Workflows.Any(x => x.WorkflowId == workflowId) ? true : false }).ToList();

            var list = new ListDto<IsAccessModel>(result, count, pageSize = pageSize, pageNumber = pageNumber);

            return list;
        }


        public async Task<ListDto<IsAccessModel>> GetWorkflowsAccessByRoleId(int roleId, int pageSize, int pageNumber)
        {
            var count = await _context.Workflow.CountAsync();
            var Workflows = await _context.Workflow.Include(x => x.Role_Workflows)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .ToListAsync();

            var result = Workflows.Select(x => new IsAccessModel() { Id = x.Id, Name = x.Name, IsAccess = x.Role_Workflows.Any(x => x.RoleId == roleId) ? true : false }).ToList();

            var list = new ListDto<IsAccessModel>(result, count, pageSize = pageSize, pageNumber = pageNumber);

            return list;
        }
    }
}
