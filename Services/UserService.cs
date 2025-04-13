using DataLayer.DbContext;
using Entities.Models.MainEngine;
using Entities.Models.Workflows;
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
        Task InsertUserAsync(User workflow);
        Task UpdateUserAsync(User workflow);
        Task DeleteUserAsync(int id);
        Task<User?> GetUserByIdAsync(int id);
        Task<List<User>> GetAllUsersAsync();
        Task SaveChangesAsync();
    }

    public class UserService : IUserService
    {
        private readonly DynamicDbContext _context;
        public UserService(DynamicDbContext context)
        {
            _context = context;
        }
        public async Task DeleteUserAsync(int id)
        {
            if (id == 0) throw new CustomException("کاربر معتبر نمی باشد");
            var feachModel = await _context.User.FirstOrDefaultAsync(x => x.Id == id)
             ?? throw new CustomException("کاربر معتبر نمی باشد.");

            _context.User.Remove(feachModel);
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var feachModel = await _context.User.ToListAsync()
                       ?? throw new CustomException("هیچ کاربر یافت نشد.");

            return feachModel;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            var feachModel = await _context.User.FirstAsync(x => x.Id == id);

            return feachModel;
        }

        public async Task InsertUserAsync(User workflow)
        {
            UserValidation(workflow);

            await _context.User.AddAsync(workflow);
        }
        public async Task UpdateUserAsync(User user)
        {
            UserValidation(user);
            if (user.Id == 0) throw new CustomException("کاربر معتبر نمی باشد");

            var result = await _context.User.FirstOrDefaultAsync(x => x.Id == user.Id)
             ?? throw new CustomException("کاربر یافت نشد.");

            result.Name = user.Name;
            result.UserName = user.UserName;
            result.UserAgent = user.UserAgent;
            result.Salt = user.Salt;
            result.RefreshToken = user.RefreshToken;
            result.Password = user.Password;
            result.IP = user.IP;
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
