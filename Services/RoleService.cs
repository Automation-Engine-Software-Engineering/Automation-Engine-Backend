using DataLayer.DbContext;
using Entities.Models.MainEngine;
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
using Tools.AuthoraizationTools;
using Tools.TextTools;
using ViewModels.ViewModels.Workflow;

namespace Services
{
    public interface IRoleService
    {
        Task<(User User, int? RoleId)> Login(string userName, string password);
        Task<List<Workflow>> GetWorkflowsByRole(int roleId);
        Task<List<Workflow_User>> GetWorkflowUsersByRole(int userId);
        Task<Role> GetRoleByUser(int userId);
        Task InsertRoleAsync(Role role);
        Task UpdateRoleAsync(Role role);
        Task DeleteRoleAsync(int id);
        Task<Role> GetRoleByIdAsync(int id);
        Task<ListDto<Role>> GetAllRolesAsync(int pageSize, int pageNumber);
        Task<ValidationDto<Role>> RoleValidationAsync(Role role);
        Task<ValidationDto<string>> SaveChangesAsync();
        Task<ListDto<WorkflowAccess>> GetAllUserForRoleAccess(int roleId, int pageSize, int pageNumber);
    }
    public class RoleService : IRoleService
    {
        private readonly Context _context;
        private readonly DynamicDbContext _dynamicContext;

        public RoleService(DataLayer.DbContext.Context context, DynamicDbContext dynamicContext)
        {
            _context = context;
            _dynamicContext = dynamicContext;
        }

        public async Task<Role> GetRoleByUser(int userId)
        {
            if (userId == 0) throw new CustomException("فرد یافت نشد.");
            var roleUser = await _context.Role_Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.UserId == userId)
              ?? throw new CustomException("نقش مورد نظر یافت نشد.");

            return roleUser.Role;
        }

        public async Task<List<Workflow>> GetWorkflowsByRole(int roleId)
        {
            if (roleId == 0) throw new CustomException("نقش یافت نشد.");

            var RoleWorkflow = await _context.Role_Workflows.Include(x => x.Workflow).Where(x => x.RoleId == roleId).ToListAsync()
                ?? throw new CustomException("نقش یافت نشد.");

            var Workflows = RoleWorkflow.Select(x => x.Workflow).ToList();

            return Workflows;
        }

        public async Task<List<Workflow_User>> GetWorkflowUsersByRole(int userId)
        {
            if (userId == 0) throw new CustomException("فرد یافت نشد.");
            var workflowUser = await _context.Workflow_User.Where(x => x.UserId == userId).ToListAsync()
                ?? throw new CustomException("نقش یافت نشد.");

            return workflowUser;
        }

        public async Task<(User User, int? RoleId)> Login(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password)) throw new CustomException("نام کاربری و رمز عبور نمی تواند خالی باشد");

            var user = await _dynamicContext.User.SingleOrDefaultAsync(x => x.UserName == userName)
                   ?? throw new CustomException<object>(new ValidationDto<object>(false, "Authentication", "Login", null), 401);

            if (!user.Password.IsNullOrEmpty())
            {
                var hashPassword = HashString.HashPassword(password, user.Salt);
                if (hashPassword != user.Password)
                    throw new CustomException<object>(new ValidationDto<object>(false, "Authentication", "Login", null), 401);
            }
            var roleUser = await _context.Role_Users.FirstOrDefaultAsync(x => x.UserId == user.Id);

            return (user, roleUser?.RoleId);
        }
        public async Task InsertRoleAsync(Role role)
        {
            await _context.Roles.AddAsync(role);
        }

        public async Task UpdateRoleAsync(Role role)
        {
            var existingRole = await _context.Roles.FirstOrDefaultAsync(x => x.Id == role.Id);

            existingRole.Name = role.Name;
            existingRole.Description = role.Description;
            _context.Roles.Update(existingRole);
        }

        public async Task DeleteRoleAsync(int id)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(x => x.Id == id);
            _context.Roles.Remove(role);
        }

        public async Task<Role> GetRoleByIdAsync(int id)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(x => x.Id == id);
            return role;
        }

        public async Task<ListDto<Role>> GetAllRolesAsync(int pageSize, int pageNumber)
        {
            var query = _context.Roles;

            var count = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new ListDto<Role>(items, count, pageSize, pageNumber);
        }

        public async Task<ValidationDto<Role>> RoleValidationAsync(Role role)
        {
            if (role == null) return new ValidationDto<Role>(false, "Role", "InvalidRole", role);
            if (string.IsNullOrWhiteSpace(role.Name)) return new ValidationDto<Role>(false, "Role", "InvalidName", role);
            if (string.IsNullOrWhiteSpace(role.Description)) return new ValidationDto<Role>(false, "Role", "InvalidDescription", role);
            return new ValidationDto<Role>(true, "Success", "ValidationPassed", role);
        }

        public async Task<ValidationDto<string>> SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                return new ValidationDto<string>(true, "Success", "ChangesSaved", null);
            }
            catch (Exception ex)
            {
                return new ValidationDto<string>(false, "Error", "SaveFailed", ex.Message);
            }
        }


        public async Task<ListDto<WorkflowAccess>> GetAllUserForRoleAccess(int roleId, int pageSize, int pageNumber)
        {
            var roles = await _context.Roles.Include(x => x.role_User)
            .FirstOrDefaultAsync(x => x.Id == roleId);

            var users = await _dynamicContext.User.Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .ToListAsync();


            var result = users.Select(x => new WorkflowAccess() { Id = x.Id, Name = x.Name, IsAccess =  roles.role_User.Any(xx => xx.UserId == x.Id) ? true : false  , UserName = x.UserName}).ToList();

            var list = new ListDto<WorkflowAccess>(result, result.Count, pageSize = pageSize, pageNumber = pageNumber);

            return list;
        }


    }
}
