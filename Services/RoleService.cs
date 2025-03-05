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
    }
    public class RoleService : IRoleService
    {
        private readonly Context _context;
        public RoleService(Context context)
        {
            _context = context;
        }

        public async Task<List<WorkFlow>> GetWorkFlowsByRole(int roleId)
        {
            if (roleId == null || roleId == 0) throw new CostumExeption("نقش یافت نشد.");

            var RoleWorkFlow = await _context.role_WorkFlows.FirstOrDefaultAsync(x => x.RoleId == roleId)
                ?? throw new CostumExeption("نقش یافت نشد.");

            var WorkFlows = await _context.WorkFlow.Where(x => x.Id == RoleWorkFlow.Id).ToListAsync()
                ?? throw new CostumExeption("نقش یافت نشد.");

            return WorkFlows;
        }

        public async Task<List<WorkFlow_User>> GetWorkFlowUsersByRole(int userId)
        {
            if (userId == null || userId == 0) throw new CostumExeption("نقش یافت نشد.");
            var workFlowUser = await _context.WorkFlow_User.Where(x => x.UserId == userId).ToListAsync()
                ?? throw new CostumExeption("نقش یافت نشد.");

            return workFlowUser;
        }

        public async Task<(int UserId, int RoleId)> login(string userName, string password)
        {
            if (userName.IsNullOrEmpty() || password.IsNullOrEmpty()) throw new CostumExeption("اشتباه در تکمیل اطلاعات.");

            var result = await _context.User.FirstOrDefaultAsync(x => x.UserName == userName && x.Password == password)
                   ?? throw new CostumExeption("نام کاربری یا رمز عبور اشتباه است."); 

            var roleUser = await _context.Role_Users.FirstOrDefaultAsync(x => x.UserId == result.Id)
                  ?? throw new CostumExeption("نقش مورد نظر یافت نشد.");

            var role = await _context.Roles.FirstOrDefaultAsync(x => x.Id == roleUser.Id)
                  ?? throw new CostumExeption("نقش مورد نظر یافت نشد.");

            return (result.Id, role.Id);
        }
    }
}
