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
using Tools.AuthoraizationTools;

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
        Task<User?> GetUserByUsernameAsync(string username);
        CustomException UserValidation(User user);
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
            var feachModel = await _context.User.FirstOrDefaultAsync(x => x.Id == id)
             ?? throw new CustomException("User", "UserNotFound");

            _context.User.Remove(feachModel);
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var feachModel = await _context.User.ToListAsync();

            return feachModel;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            var feachModel = await _context.User.FirstOrDefaultAsync(x => x.Id == id);

            return feachModel;
        }
        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            var feachModel = await _context.User.SingleOrDefaultAsync(x => x.UserName == username);
            return feachModel;
        }
        public async Task InsertUserAsync(User workflow)
        {
            await _context.User.AddAsync(workflow);
        }
        public async Task UpdateUserAsync(User user)
        {
            var result = await _context.User.FirstOrDefaultAsync(x => x.Id == user.Id)
             ?? throw new CustomException("User", "UserNotFound");

            result.Name = user.Name;
            result.UserName = user.UserName;
            result.UserAgent = user.UserAgent;
            result.Salt = user.Salt;
            result.RefreshToken = user.RefreshToken;
            result.Password = user.Password;
            result.IP = user.IP;
            _context.Update(result);
        }

        public CustomException UserValidation(User user)
        {
            var invalidUser = new CustomException("User", "CorruptedUser");
            if (user == null) throw invalidUser;
            if (string.IsNullOrEmpty(user.UserName)) throw invalidUser;
            if (string.IsNullOrEmpty(user.Name)) throw invalidUser;

            return new CustomException("Success", "Success");
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
