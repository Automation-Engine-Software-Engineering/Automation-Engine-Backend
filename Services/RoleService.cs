using DataLayer.Context;
using DataLayer.Models.MainEngine;
using DataLayer.Models.WorkFlow;
using FrameWork.ExeptionHandler.ExeptionModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IRoleService
    {
        public Task<(int UserId, int RoleId)> login(string userName, string password);
        public Task<List<WorkFlow>> GetWorkFlowsByRole(int roleId);
        public Task<List<WorkFlow_User>> GetWorkFlowUsersByRole(int userId);
        public Task<Role> GetRoleByUser(int userId);
    }
    public class RoleService : IRoleService
    {
        private readonly DataLayer.Context.DbContext _context;
        public RoleService(DataLayer.Context.DbContext context)
        {
            _context = context;
        }

        public async Task<Role> GetRoleByUser(int userId)
        {
            if (userId == null || userId == 0) throw new CustomException("فرد یافت نشد.");
            var roleUser = _context.Role_Users.FirstOrDefault(x => x.UserId == userId)
              ?? throw new CustomException("نقش مورد نظر یافت نشد.");

            var role = _context.Roles.FirstOrDefault(x => x.Id == roleUser.Id)
              ?? throw new CustomException("نقش مورد نظر یافت نشد.");

            return role;
        }

        public async Task<List<WorkFlow>> GetWorkFlowsByRole(int roleId)
        {
            if (roleId == null || roleId == 0) throw new CustomException("نقش یافت نشد.");

            var RoleWorkFlow = await _context.Role_WorkFlows.Where(x => x.RoleId == roleId).ToListAsync()
                ?? throw new CustomException("نقش یافت نشد.");

            var WorkFlows = _context.WorkFlow.Where(x => RoleWorkFlow.Any(xx => xx.WorkFlowId == x.Id)).ToList()
                ?? throw new CustomException("نقش یافت نشد.");

            return WorkFlows;
        }

        public async Task<List<WorkFlow_User>> GetWorkFlowUsersByRole(int userId)
        {
            if (userId == null || userId == 0) throw new CustomException("فرد یافت نشد.");
            var workFlowUser = await _context.WorkFlow_User.Where(x => x.UserId == userId).ToListAsync()
                ?? throw new CustomException("نقش یافت نشد.");

            return workFlowUser;
        }

        public async Task<(int UserId, int RoleId)> login(string userName, string password)
        {
            if (userName.IsNullOrEmpty() || password.IsNullOrEmpty()) throw new CustomException("اشتباه در تکمیل اطلاعات.");

            var result =  _context.User.FirstOrDefault(x => x.UserName == userName && x.Password == password)
                   ?? throw new CustomException("نام کاربری یا رمز عبور اشتباه است."); 

            var roleUser =  _context.Role_Users.FirstOrDefault(x => x.UserId == result.Id)
                  ?? throw new CustomException("نقش مورد نظر یافت نشد.");

            var role =  _context.Roles.FirstOrDefault(x => x.Id == roleUser.Id)
                  ?? throw new CustomException("نقش مورد نظر یافت نشد.");

            return (result.Id, role.Id);
        }
    }
}
