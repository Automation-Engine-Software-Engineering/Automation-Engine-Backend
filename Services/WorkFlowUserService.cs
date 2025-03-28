using DataLayer.Context;
using DataLayer.Models.WorkFlow;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
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
        Task<ListDto<WorkFlow_User>> GetAllWorFlowUsers(int pageSize, int pageNumber);
        Task<ValidationDto<WorkFlow_User>> WorkFlowValidation(WorkFlow_User workFlowUser);
        Task<ValidationDto<string>> SaveChangesAsync();
    }

    public class WorkFlowUserService : IWorkFlowUserService
    {
        private readonly DataLayer.Context.DbContext _context;
        public WorkFlowUserService(DataLayer.Context.DbContext context)
        {
            _context = context;
        }
        public async Task DeleteWorFlowUser(int id)
        {
            var fetchModel = await _context.WorkFlow_User.FirstOrDefaultAsync(x => x.Id == id);
            _context.WorkFlow_User.Remove(fetchModel);
        }

        public async Task<ListDto<WorkFlow_User>> GetAllWorFlowUsers(int pageSize, int pageNumber)
        {
            //create query
            var query = _context.WorkFlow_User;

            //get Value and count
            var count = query.Count();
            var result = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return new ListDto<WorkFlow_User>(result, count, pageSize, pageNumber);
        }

        public async Task<WorkFlow_User> GetWorFlowUserById(int id)
        {
            var fetchModel = await _context.WorkFlow_User.FirstAsync(x => x.Id == id);
            return fetchModel;
        }

        public async Task InsertWorFlowUser(WorkFlow_User workFlow)
        {
            await _context.WorkFlow_User.AddAsync(workFlow);
        }

        public async Task UpdateWorFlowUser(WorkFlow_User workFlow)
        {
            var result = await _context.WorkFlow_User.FirstAsync(x => x.Id == workFlow.Id);

            var fetchModel = new WorkFlow_User();
            fetchModel.WorkFlowState = workFlow.WorkFlowState;
            fetchModel.UserId = workFlow.UserId;
            fetchModel.WorkFlowId = workFlow.WorkFlowId;
            _context.Update(fetchModel);
        }

        public async Task<ValidationDto<WorkFlow_User>> WorkFlowValidation(WorkFlow_User workFlowUser)
        {
            if (workFlowUser == null) return new ValidationDto<WorkFlow_User>(false, "Form", "CorruptedForm", workFlowUser);
            if (workFlowUser.User == null) return new ValidationDto<WorkFlow_User>(false, "Form", "CorruptedForm", workFlowUser);
            if (workFlowUser.WorkFlowState == null) return new ValidationDto<WorkFlow_User>(false, "Form", "CorruptedForm", workFlowUser);
            if (workFlowUser.WorkFlow == null) return new ValidationDto<WorkFlow_User>(false, "Form", "CorruptedForm", workFlowUser);

            return new ValidationDto<WorkFlow_User>(true, "Success", "Success", workFlowUser);
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
    }
}
