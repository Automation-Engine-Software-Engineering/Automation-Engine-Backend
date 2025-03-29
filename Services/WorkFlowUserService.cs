using DataLayer.DbContext;
using DataLayer.Models.WorkFlows;
using FrameWork.ExeptionHandler.ExeptionModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace Services
{
    public interface IWorkFlowUserService
    {
        Task InsertWorFlowUser(WorkFlow_User workFlow);
        Task UpdateWorFlowUser(WorkFlow_User workFlow);
        Task DeleteWorFlowUser(int id);
        Task<WorkFlow_User> GetWorFlowUserById(int id);
        Task<List<WorkFlow_User>> GetAllWorFlowUsers();
        Task SaveChangesAsync();
    }

    public class WorkFlowUserService : IWorkFlowUserService
    {
        private readonly DataLayer.DbContext.Context _context;
        public WorkFlowUserService(DataLayer.DbContext.Context context)
        {
            _context = context;
        }
        public async Task DeleteWorFlowUser(int id)
        {
            if (id == 0) throw new CustomException("گردشکار معتبر نمی باشد");
            var feachModel = await _context.WorkFlow_User.FirstOrDefaultAsync(x => x.Id == id)
             ?? throw new CustomException("گردشکار معتبر نمی باشد.");

            _context.WorkFlow_User.Remove(feachModel);
        }

        public async Task<List<WorkFlow_User>> GetAllWorFlowUsers()
        {
            var feachModel = await _context.WorkFlow_User.ToListAsync()
                       ?? throw new CustomException("هیچ گردشکار یافت نشد.");

            return feachModel;
        }

        public async Task<WorkFlow_User> GetWorFlowUserById(int id)
        {
            if (id == 0) throw new CustomException("گردشکار معتبر نمی باشد");

            var feachModel = await _context.WorkFlow_User.FirstOrDefaultAsync(x => x.Id == id)
                  ?? throw new CustomException("گردشکار معتبر نمی باشد");

            return feachModel;
        }

        public async Task InsertWorFlowUser(WorkFlow_User workFlow)
        {
            await WorkFlowValidation(workFlow);

            await _context.WorkFlow_User.AddAsync(workFlow);
        }
        public async Task UpdateWorFlowUser(WorkFlow_User workFlow)
        {
            await WorkFlowValidation(workFlow);
            if (workFlow.Id == 0) throw new CustomException("گردشکار معتبر نمی باشد");

            var result = await _context.WorkFlow_User.FirstOrDefaultAsync(x => x.Id == workFlow.Id)
             ?? throw new CustomException("گردشکار یافت نشد.");

            var feachModel = new WorkFlow_User()
            {
                WorkFlowState = workFlow.WorkFlowState,
                UserId = workFlow.UserId,
                WorkFlowId = workFlow.WorkFlowId
            };
            _context.Update(feachModel);
        }

        public async Task<string> WorkFlowValidation(WorkFlow_User workFlowUser)
        {
            if (workFlowUser == null) throw new CustomException("اطلاعات گردشکار معتبر نمی باشد");
            if (string.IsNullOrEmpty(workFlowUser.WorkFlowState)) throw new CustomException("اطلاعات گردشکار معتبر نمی باشد");
            if (workFlowUser.WorkFlowId == 0 || workFlowUser.WorkFlowId == 0) throw new CustomException("اطلاعات گردشکار معتبر نمی باشد");
            if (workFlowUser.UserId == 0 || workFlowUser.UserId == 0) throw new CustomException("اطلاعات گردشکار معتبر نمی باشد");

            return "";
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
