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
            var cleanUserName = model.UserName.Trim();
            var cleanEmail = model.Email.Trim();

            var existing = await _db.Users.FirstOrDefaultAsync(u =>
                !u.IsDeleted &&
                (u.UserName == cleanUserName || u.Email == cleanEmail));

            if (existing != null)
            {
                return new UserCreateResponseModel
                {
                    IsSuccess = false,
                    Message = "Username or email is already registered."
                };
            }

            var user = new User
            {
                UserName = cleanUserName,
                DisplayName = string.IsNullOrWhiteSpace(model.DisplayName) ? cleanUserName : model.DisplayName.Trim(),
                Email = cleanEmail,
                Password = model.Password.Trim(),
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return new UserCreateResponseModel
            {
                IsSuccess = true,
                Message = "User registered successfully."
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
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
                return new UserLoginResponseModel { IsSuccess = false, Message = "Email/Username and password are required." };

            var input = model.Email.Trim();
            var inputNoSpace = input.Replace(" ", "");
            var pwd = model.Password.Trim();

            // 1. Direct EF Core SQL query matching UserName, Email, or DisplayName
            var user = await _db.Users.FirstOrDefaultAsync(u =>
                !u.IsDeleted &&
                (u.Email == input ||
                 u.UserName == input ||
                 (u.DisplayName != null && u.DisplayName == input) ||
                 u.UserName == inputNoSpace ||
                 (u.DisplayName != null && u.DisplayName == inputNoSpace)) &&
                u.Password == pwd);

            // 2. Case-insensitive/whitespace fallback candidate evaluation
            if (user == null)
            {
                var candidates = await _db.Users.Where(u => !u.IsDeleted && u.Password == pwd).ToListAsync();
                user = candidates.FirstOrDefault(u =>
                    u.Email.Equals(input, StringComparison.OrdinalIgnoreCase) ||
                    u.UserName.Equals(input, StringComparison.OrdinalIgnoreCase) ||
                    (u.DisplayName != null && u.DisplayName.Equals(input, StringComparison.OrdinalIgnoreCase)) ||
                    u.UserName.Replace(" ", "").Equals(inputNoSpace, StringComparison.OrdinalIgnoreCase) ||
                    (u.DisplayName != null && u.DisplayName.Replace(" ", "").Equals(inputNoSpace, StringComparison.OrdinalIgnoreCase)));
            }

            if (user == null)
                return new UserLoginResponseModel { IsSuccess = false, Message = "Invalid email/username or password." };

            bool isAdmin = user.UserName.Equals("admin", StringComparison.OrdinalIgnoreCase) ||
                           user.Email.Equals("admin@nangworld.com", StringComparison.OrdinalIgnoreCase);

            return new UserLoginResponseModel
            {
                IsSuccess = true,
                Message = "Login successful.",
                Id = user.Id,
                UserName = user.UserName,
                DisplayName = user.DisplayName ?? user.UserName,
                Email = user.Email,
                Role = isAdmin ? "Admin" : "User"
            };
        }

        public async Task<UserProfileDto> GetProfileAsync(int id)
        {
            var user = await _db.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

            if (user == null)
            {
                throw new Exception("User not found for the given ID.");
            }

            var roles = user.UserRoles
                .Where(ur => ur.Role != null && !string.IsNullOrWhiteSpace(ur.Role.RoleName))
                .Select(ur => ur.Role.RoleName)
                .Distinct()
                .ToList();

            if (!roles.Any())
            {
                bool isAdmin = user.UserName.Equals("admin", StringComparison.OrdinalIgnoreCase) ||
                               user.Email.Equals("admin@nangworld.com", StringComparison.OrdinalIgnoreCase);
                roles.Add(isAdmin ? "Admin" : "User");
            }

            return new UserProfileDto
            {
                Id = user.Id,
                UserName = user.UserName,
                DisplayName = user.DisplayName ?? user.UserName,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl,
                Bio = user.Bio,
                Role = roles.FirstOrDefault() ?? "User",
                Roles = roles,
                CreatedDate = user.CreatedAt
            };
        }

        public async Task<UserProfileUpdateResponseModel> UpdateProfileAsync(UserProfileUpdateRequestModel model)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == model.UserId && !u.IsDeleted);
            if (user == null)
            {
                return new UserProfileUpdateResponseModel
                {
                    IsSuccess = false,
                    Message = "User not found."
                };
            }

            if (!string.IsNullOrWhiteSpace(model.DisplayName))
            {
                user.DisplayName = model.DisplayName.Trim();
            }
            user.Bio = model.Bio?.Trim();

            await _db.SaveChangesAsync();

            var profile = await GetProfileAsync(user.Id);
            return new UserProfileUpdateResponseModel
            {
                IsSuccess = true,
                Message = "Profile updated successfully.",
                User = profile
            };
        }

        public async Task<string> UpdateAvatarAsync(int userId, string avatarUrl)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            user.AvatarUrl = avatarUrl;
            await _db.SaveChangesAsync();
            return avatarUrl;
        }

        public async Task<ChangePasswordResponseModel> ChangePasswordAsync(ChangePasswordRequestModel model)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == model.UserId && !u.IsDeleted);
            if (user == null)
            {
                return new ChangePasswordResponseModel
                {
                    IsSuccess = false,
                    Message = "User not found."
                };
            }

            if (user.Password != model.CurrentPassword.Trim())
            {
                return new ChangePasswordResponseModel
                {
                    IsSuccess = false,
                    Message = "Current password is incorrect."
                };
            }

            if (string.IsNullOrWhiteSpace(model.NewPassword) || model.NewPassword.Length < 6)
            {
                return new ChangePasswordResponseModel
                {
                    IsSuccess = false,
                    Message = "New password must be at least 6 characters long."
                };
            }

            if (model.NewPassword != model.ConfirmNewPassword)
            {
                return new ChangePasswordResponseModel
                {
                    IsSuccess = false,
                    Message = "New password and confirmation do not match."
                };
            }

            if (user.Password == model.NewPassword.Trim())
            {
                return new ChangePasswordResponseModel
                {
                    IsSuccess = false,
                    Message = "New password cannot be the same as your current password."
                };
            }

            user.Password = model.NewPassword.Trim();
            await _db.SaveChangesAsync();

            return new ChangePasswordResponseModel
            {
                IsSuccess = true,
                Message = "Password changed successfully."
            };
        }
    }
}
