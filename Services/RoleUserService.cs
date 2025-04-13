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
        Task InsertRoleUserAsync(Role_User roleUser);
        Task UpdateRoleUserAsync(Role_User roleUser);
        Task DeleteRoleUserAsync(int id);
        Task<Role_User?> GetRoleUserByIdAsync(int userId );
        Task<ListDto<Role_User>>  GetRoleUserByUserIdAsync(int userId , int pageSize, int pageNumber);
        Task<ListDto<Role_User>> GetAllRoleUsersAsync(int pageSize, int pageNumber);
        ValidationDto<Role_User> RoleUserValidation(Role_User roleUser);
        Task<ValidationDto<string>> SaveChangesAsync();
    }

    public class RoleUserService : IRoleUserService
    {
        private readonly Context _context;

        public RoleUserService(Context context)
        {
            _context = context;
        }

        public async Task InsertRoleUserAsync(Role_User roleUser)
        {
            await _context.Role_Users.AddAsync(roleUser);
        }

        public async Task UpdateRoleUserAsync(Role_User roleUser)
        {
            var existingRoleUser = await _context.Role_Users.FirstAsync(x => x.Id == roleUser.Id);

            existingRoleUser.RoleId = roleUser.RoleId;
            existingRoleUser.UserId = roleUser.UserId;
            _context.Role_Users.Update(existingRoleUser);
        }

        public async Task DeleteRoleUserAsync(int id)
        {
            var roleUser = await _context.Role_Users.FirstAsync(x => x.Id == id);

            _context.Role_Users.Remove(roleUser);
        }

        public async Task<Role_User?> GetRoleUserByIdAsync(int id)
        {
            var roleUser = await _context.Role_Users.FirstOrDefaultAsync(x => x.Id == id);

            return roleUser;
        }

        public async Task<ListDto<Role_User>> GetAllRoleUsersAsync(int pageSize, int pageNumber)
        {
            var query = _context.Role_Users;

            var count = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new ListDto<Role_User>(items, count, pageSize, pageNumber);
        }

        public ValidationDto<Role_User> RoleUserValidation(Role_User roleUser)
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

        public async Task<ListDto<Role_User>>  GetRoleUserByUserIdAsync(int userId , int pageSize, int pageNumber)
        {
            var query = _context.Role_Users.Where(x => x.UserId == userId);
            var count = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new ListDto<Role_User>(items, count, pageSize, pageNumber);
        }
    }
}