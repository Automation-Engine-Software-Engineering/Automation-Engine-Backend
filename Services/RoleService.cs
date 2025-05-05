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
        Task<List<Workflow>> GetWorkflowsByRoleAsync(int roleId);
        Task<List<Workflow_User>> GetWorkflowUsersByRoleAsync(int userId);
        Task<Role> GetRoleByUserAsync(int userId);
        Task InsertRoleAsync(Role role);
        Task UpdateRoleAsync(Role role);
        Task DeleteRoleAsync(int id);
        Task<Role?> GetRoleByIdAsync(int id);
        Task<ListDto<Role>> GetAllRolesAsync(int pageSize, int pageNumber);
        CustomException RoleValidation(Role role);
        Task SaveChangesAsync();
        Task<ListDto<IsAccessModel>> GetAllUserForRoleAccessAsync(int roleId, int pageSize, int pageNumber);
    }
    public class RoleService : IRoleService
    {
        private readonly Context _context;
        private readonly DynamicDbContext _dynamicContext;

        public RoleService(Context context, DynamicDbContext dynamicContext)
        {
            _context = context;
            _dynamicContext = dynamicContext;
        }

        public async Task<Role> GetRoleByUserAsync(int userId)
        {
            if (userId == 0) throw new CustomException("فرد یافت نشد.");
            var roleUser = await _context.Role_Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.UserId == userId)
              ?? throw new CustomException("نقش مورد نظر یافت نشد.");

            return roleUser.Role;
        }

        public async Task<List<Workflow>> GetWorkflowsByRoleAsync(int roleId)
        {
            if (roleId == 0) throw new CustomException("نقش یافت نشد.");

            var RoleWorkflow = await _context.Role_Workflows.Include(x => x.Workflow).Where(x => x.RoleId == roleId).ToListAsync()
                ?? throw new CustomException("نقش یافت نشد.");

            var Workflows = RoleWorkflow.Select(x => x.Workflow).ToList();

            return Workflows;
        }

        public async Task<List<Workflow_User>> GetWorkflowUsersByRoleAsync(int userId)
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
                   ?? throw new CustomException("Authentication", "Login");

            if (!user.Password.IsNullOrEmpty())
            {
                var hashPassword = HashString.HashPassword(password, user.Salt);
                if (hashPassword != user.Password)
                    throw new CustomException("Authentication", "Login");
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
            var existingRole = await _context.Roles.FirstAsync(x => x.Id == role.Id);

            existingRole.Name = role.Name;
            existingRole.Description = role.Description;
            _context.Roles.Update(existingRole);
        }

        public async Task DeleteRoleAsync(int id)
        {
            var role = await _context.Roles.FirstAsync(x => x.Id == id);
            _context.Roles.Remove(role);
        }

        public async Task<Role?> GetRoleByIdAsync(int id)
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

        public CustomException RoleValidation(Role role)
        {
            var invalidRole = new CustomException("Role", "InvalidRole", role);
            if (role == null) return invalidRole;
            if (string.IsNullOrWhiteSpace(role.Name)) return invalidRole;
            if (string.IsNullOrWhiteSpace(role.Description)) return invalidRole;
            return new CustomException("Success", "Success", role);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<ListDto<IsAccessModel>> GetAllUserForRoleAccessAsync(int roleId, int pageSize, int pageNumber)
        {
            var roles = await _context.Roles.Include(x => x.role_User)
            .FirstAsync(x => x.Id == roleId);

            var users = await _dynamicContext.User.Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .ToListAsync();

            var result = users.Select(x => new IsAccessModel() { Id = x.Id, Name = x.Name, IsAccess = roles.role_User.Any(xx => xx.UserId == x.Id), UserName = x.UserName }).ToList();

            var list = new ListDto<IsAccessModel>(result, result.Count, pageSize, pageNumber);

            return list;
        }


    }
}
