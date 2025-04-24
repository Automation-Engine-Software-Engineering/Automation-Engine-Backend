using DataLayer.DbContext;
using Entities.Models.MainEngine;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace Services
{
    public interface IRoleUserService
    {
        Task InsertRoleUser(Role_User roleUser);
        Task UpdateRoleUser(Role_User roleUser);
        Task DeleteRoleUser(int id);
        Task<Role_User> GetRoleUserById(int userId);
        Task<ListDto<Role_User>> GetRoleUserByUserId(int userId, int pageSize, int pageNumber);
        Task<ListDto<Role_User>> GetAllRoleUsers(int pageSize, int pageNumber);
        Task<ValidationDto<Role_User>> RoleUserValidation(Role_User roleUser);
        Task InsertRangeUserRole(List<Role_User> Users);
        Task ReplaceUserRolesByRoleId(int RoleId, List<int> UserIds);
        Task<ValidationDto<string>> SaveChangesAsync();
    }

    public class RoleUserService : IRoleUserService
    {
        private readonly DataLayer.DbContext.Context _context;

        public RoleUserService(DataLayer.DbContext.Context context)
        {
            _context = context;
        }

        public async Task InsertRoleUser(Role_User roleUser)
        {
            await _context.Role_Users.AddAsync(roleUser);
        }

        public async Task UpdateRoleUser(Role_User roleUser)
        {
            var existingRoleUser = await _context.Role_Users.FirstOrDefaultAsync(x => x.Id == roleUser.Id);

            existingRoleUser.RoleId = roleUser.RoleId;
            existingRoleUser.UserId = roleUser.UserId;
            _context.Role_Users.Update(existingRoleUser);
        }

        public async Task InsertRangeUserRole(List<Role_User> Users)
        {
            await _context.Role_Users.AddRangeAsync(Users);
        }
        public async Task ReplaceUserRolesByRoleId(int RoleId, List<int> UserIds)
        {
            var roleWorkflows = await _context.Role_Users.Where(x => x.RoleId == RoleId).ToListAsync();
            _context.Role_Users.RemoveRange(roleWorkflows);

            var newRoleWorkflows = UserIds.Select(x => new Role_User
            {
                RoleId = RoleId,
                UserId = x
            }).ToList();
            await InsertRangeUserRole(newRoleWorkflows);
        }

        public async Task DeleteRoleUser(int id)
        {
            var roleUser = await _context.Role_Users.FirstOrDefaultAsync(x => x.Id == id);

            _context.Role_Users.Remove(roleUser);
        }

        public async Task<Role_User> GetRoleUserById(int id)
        {
            var roleUser = await _context.Role_Users.FirstOrDefaultAsync(x => x.Id == id);

            return roleUser;
        }

        public async Task<ListDto<Role_User>> GetAllRoleUsers(int pageSize, int pageNumber)
        {
            var query = _context.Role_Users;

            var count = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new ListDto<Role_User>(items, count, pageSize, pageNumber);
        }

        public async Task<ValidationDto<Role_User>> RoleUserValidation(Role_User roleUser)
        {
            if (roleUser == null)
            {
                return new ValidationDto<Role_User>(false, "RoleUser", "InvalidRoleUser", roleUser);
            }

            if (roleUser.RoleId == 0 || roleUser.Role == null)
            {
                return new ValidationDto<Role_User>(false, "RoleUser", "InvalidRole", roleUser);
            }

            if (roleUser.UserId == 0)
            {
                return new ValidationDto<Role_User>(false, "RoleUser", "InvalidUser", roleUser);
            }

            return new ValidationDto<Role_User>(true, "Success", "ValidationPassed", roleUser);
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

        public async Task<ListDto<Role_User>> GetRoleUserByUserId(int userId, int pageSize, int pageNumber)
        {
            var query = _context.Role_Users.Where(x => x.UserId == userId);
            var count = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new ListDto<Role_User>(items, count, pageSize, pageNumber);
        }
    }
}