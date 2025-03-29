using DataLayer.DbContext;
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
using Tools;

namespace Services
{
    public interface IUserService
    {
        Task InsertUser(User workFlow);
        Task UpdateUser(User workFlow);
        Task DeleteUser(int id);
        Task<User> GetUserById(int id);
        Task<List<User>> GetAllUsers();
        Task SaveChangesAsync();
    }

    public class UserService : IUserService
    {
        private readonly DynamicDbContext _context;
        public UserService(DynamicDbContext context)
        {
            _context = context;
        }
        public async Task DeleteUser(int id)
        {
            if (id == 0) throw new CustomException("کاربر معتبر نمی باشد");
            var feachModel = await _context.User.FirstOrDefaultAsync(x => x.Id == id)
             ?? throw new CustomException("کاربر معتبر نمی باشد.");

            _context.User.Remove(feachModel);
        }

        public async Task<List<User>> GetAllUsers()
        {
            var feachModel = await _context.User.ToListAsync()
                       ?? throw new CustomException("هیچ کاربر یافت نشد.");

            return feachModel;
        }

        public async Task<User> GetUserById(int id)
        {
            if (id == 0) throw new CustomException("کاربر معتبر نمی باشد");

            var feachModel = await _context.User.FirstOrDefaultAsync(x => x.Id == id)
                  ?? throw new CustomException("کاربر معتبر نمی باشد");

            return feachModel;
        }

        public async Task InsertUser(User workFlow)
        {
            UserValidation(workFlow);

            await _context.User.AddAsync(workFlow);
        }
        public async Task UpdateUser(User user)
        {
            UserValidation(user);
            if (user.Id == 0) throw new CustomException("کاربر معتبر نمی باشد");

            var result = await _context.User.FirstOrDefaultAsync(x => x.Id == user.Id)
             ?? throw new CustomException("کاربر یافت نشد.");

            result.Name = user.Name;
            result.UserName = user.Name;
            result.RefreshToken = user.Name;
            result.Password = user.Password;
            _context.Update(result);
        }

        public string UserValidation(User user)
        {
            if (user == null) throw new CustomException("اطلاعات کاربر معتبر نمی باشد");
            if (string.IsNullOrEmpty(user.UserName)) throw new CustomException("اطلاعات کاربر معتبر نمی باشد");
            if (string.IsNullOrEmpty(user.Name)) throw new CustomException("اطلاعات کاربر معتبر نمی باشد");
            
            return "";
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
