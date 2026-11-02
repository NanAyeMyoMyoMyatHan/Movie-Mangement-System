using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using DotNetProject.Database.AppDbModels;
using DotNetProject.Domain.Features.Users;

namespace DotNetProject.WebApi.Features.Users
{
    public class UserServices : IUserInterface
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public UserServices(IWebHostEnvironment env, AppDbContext db)
        {
            _env = env;
            _db = db;
        }
        public async Task<UserModel> GetUserByIdAsync(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null || user.IsDeleted)
            {
                throw new Exception("User not found for the given ID.");
            }
            return new UserModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Password = user.Password,
                DisplayName = user.DisplayName,
                CreatedDate = user.CreatedAt,
                IsDeleted = user.IsDeleted,
                
            };
        }
        public async Task<List<UserModel>> GetAllUsersAsync()
        {
            var users = await _db.Users.Where(u => !u.IsDeleted).ToListAsync();
            return users.Select(user => new UserModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Password = user.Password,
                DisplayName = user.DisplayName,
                CreatedDate = user.CreatedAt,
                IsDeleted = user.IsDeleted,
                
            }).ToList();
        }
        public async Task<UserCreateResponseModel> CreateUserAsync(UserCreateRequestModel model)
        {
            var user = new User
            {
                UserName = model.UserName,
                Email = model.Email,
                Password = model.Password,
                DisplayName = model.DisplayName,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return new UserCreateResponseModel
            {
                IsSuccess = true,
                Message = "User created successfully."
            };
        }
        public async Task<UserUpdateResponseModel> UpdateUserAsync(UserUpdateRequestModel model)
        {
            var user = await _db.Users.FindAsync(model.Id);
            if (user == null || user.IsDeleted)
            {
                return new UserUpdateResponseModel
                {
                    IsSuccess = false,
                    Message = "User not found for the given ID."
                };
            }
            user.UserName = model.UserName;
            user.Email = model.Email;
            user.Password = model.Password;
            user.DisplayName = model.DisplayName;
            await _db.SaveChangesAsync();
            return new UserUpdateResponseModel
            {
                IsSuccess = true,
                Message = "User updated successfully."
            };
        }
        public async Task<UserLoginResponseModel> LoginAsync(UserLoginRequestModel model)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u =>
                !u.IsDeleted &&
                (u.Email == model.Email || u.UserName == model.Email) &&
                u.Password == model.Password);

            if (user == null)
                return new UserLoginResponseModel { IsSuccess = false, Message = "Invalid email/username or password." };

            return new UserLoginResponseModel
            {
                IsSuccess = true,
                Message = "Login successful.",
                Id = user.Id,
                UserName = user.UserName,
                DisplayName = user.DisplayName,
                Email = user.Email
            };
        }
    }
}
