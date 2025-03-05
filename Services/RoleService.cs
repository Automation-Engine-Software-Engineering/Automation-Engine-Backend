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
        private readonly Context _context;
        public RoleService(Context context)
        {
            _context = context;
        }

        public async Task<Role> GetRoleByUser(int userId)
        {
            if (userId == null || userId == 0) throw new CostumExeption("فرد یافت نشد.");
            var roleUser = _context.Role_Users.FirstOrDefault(x => x.UserId == userId)
              ?? throw new CostumExeption("نقش مورد نظر یافت نشد.");

            var role = _context.Roles.FirstOrDefault(x => x.Id == roleUser.Id)
              ?? throw new CostumExeption("نقش مورد نظر یافت نشد.");

            return role;
        }

        public async Task<List<WorkFlow>> GetWorkFlowsByRole(int roleId)
        {
            if (roleId == null || roleId == 0) throw new CostumExeption("نقش یافت نشد.");

            var RoleWorkFlow =  _context.Role_WorkFlows.FirstOrDefault(x => x.RoleId == roleId)
                ?? throw new CostumExeption("نقش یافت نشد.");

            var WorkFlows =  _context.WorkFlow.Where(x => x.Id == RoleWorkFlow.WorkFlowId).ToList()
                ?? throw new CostumExeption("نقش یافت نشد.");

            return WorkFlows;
        }

        public async Task<List<WorkFlow_User>> GetWorkFlowUsersByRole(int userId)
        {
            if (userId == null || userId == 0) throw new CostumExeption("فرد یافت نشد.");
            var workFlowUser = await _context.WorkFlow_User.Where(x => x.UserId == userId).ToListAsync()
                ?? throw new CostumExeption("نقش یافت نشد.");

            return workFlowUser;
        }

        public async Task<(int UserId, int RoleId)> login(string userName, string password)
        {
            if (userName.IsNullOrEmpty() || password.IsNullOrEmpty()) throw new CostumExeption("اشتباه در تکمیل اطلاعات.");

            var result =  _context.User.FirstOrDefault(x => x.UserName == userName && x.Password == password)
                   ?? throw new CostumExeption("نام کاربری یا رمز عبور اشتباه است."); 

            var roleUser =  _context.Role_Users.FirstOrDefault(x => x.UserId == result.Id)
                  ?? throw new CostumExeption("نقش مورد نظر یافت نشد.");

            var role =  _context.Roles.FirstOrDefault(x => x.Id == roleUser.Id)
                  ?? throw new CostumExeption("نقش مورد نظر یافت نشد.");

            return (result.Id, role.Id);
        }
    }
}
