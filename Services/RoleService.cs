using DataLayer.Context;
using DataLayer.Models.MainEngine;
using DataLayer.Models.WorkFlows;
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
        public Task<(int UserId, int RoleId)> Login(string userName, string password);
        public Task<List<WorkFlow>> GetWorkFlowsByRole(int roleId);
        public Task<List<WorkFlow_User>> GetWorkFlowUsersByRole(int userId);
        public Task<Role> GetRoleByUser(int userId);
    }
    public class RoleService : IRoleService
    {
        private readonly Context _context;
        private readonly DynamicDbContext _dynamicContext;

        public RoleService(Context context,DynamicDbContext dynamicContext)
        {
            _context = context;
            _dynamicContext = dynamicContext;
        }

        public async Task<Role> GetRoleByUser(int userId)
        {
            if (userId == 0) throw new CustomException("فرد یافت نشد.");
            var roleUser = await _context.Role_Users.Include(x=>x.Role).FirstOrDefaultAsync(x => x.UserId == userId)
              ?? throw new CustomException("نقش مورد نظر یافت نشد.");

            return roleUser.Role;
        }

        public async Task<List<WorkFlow>> GetWorkFlowsByRole(int roleId)
        {
            if (roleId == 0) throw new CustomException("نقش یافت نشد.");

            var RoleWorkFlow = await _context.Role_WorkFlows.Include(x=>x.WorkFlow).Where(x => x.RoleId == roleId).ToListAsync()
                ?? throw new CustomException("نقش یافت نشد.");

            var WorkFlows = RoleWorkFlow.Select(x => x.WorkFlow).ToList();

            return WorkFlows;
        }

        public async Task<List<WorkFlow_User>> GetWorkFlowUsersByRole(int userId)
        {
            if (userId == 0) throw new CustomException("فرد یافت نشد.");
            var workFlowUser = await _context.WorkFlow_User.Where(x => x.UserId == userId).ToListAsync()
                ?? throw new CustomException("نقش یافت نشد.");

            return workFlowUser;
        }

        public async Task<(int UserId, int RoleId)> Login(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password)) throw new CustomException("اشتباه در تکمیل اطلاعات.");

            var result = await _dynamicContext.User.FirstOrDefaultAsync(x => x.UserName == userName && x.Password == password)
                   ?? throw new CustomException("نام کاربری یا رمز عبور اشتباه است."); 

            var roleUser = await _context.Role_Users.FirstOrDefaultAsync(x => x.UserId == result.Id)
                  ?? throw new CustomException("نقش مورد نظر یافت نشد.");
            
            return (result.Id, roleUser.RoleId);
        }
    }
}
