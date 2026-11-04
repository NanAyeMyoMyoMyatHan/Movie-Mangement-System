using DotNetProject.Database.AppDbModels;
using DotNetProject.Domain.Features.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DotNetProject.Balzor.Components.Shared
{
    public class CurrentUserDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "Guest"; // "Admin", "User", "Guest"
        public DateTime LoggedInAt { get; set; } = DateTime.UtcNow;

        public bool IsAuthenticated => Id > 0 && !string.IsNullOrWhiteSpace(UserName) && !Role.Equals("Guest", StringComparison.OrdinalIgnoreCase);
        public bool IsAdmin => IsAuthenticated && Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
    }

    public class StoredUserAccount
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "User"; // "Admin", "User"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class AuthService
    {
        private readonly HttpClientService _api;
        private readonly IServiceProvider _serviceProvider;
        private CurrentUserDto _currentUser = new();
        private bool _isInitialized = false;

        private static readonly List<StoredUserAccount> _accounts = new();
        private static readonly object _accountsLock = new();

        public event Action? OnAuthStateChanged;

        public AuthService(HttpClientService api, IServiceProvider serviceProvider)
        {
            _api = api;
            _serviceProvider = serviceProvider;
            EnsureInitialized();
        }

        public CurrentUserDto CurrentUser
        {
            get
            {
                EnsureInitialized();
                return _currentUser;
            }
        }

        public bool IsAdmin() => CurrentUser.IsAdmin;
        public bool IsAuthenticated() => CurrentUser.IsAuthenticated;

        public string GetCurrentUserKey()
        {
            EnsureInitialized();
            if (CurrentUser.IsAuthenticated && CurrentUser.Id > 0)
            {
                var rawName = string.IsNullOrWhiteSpace(CurrentUser.UserName) ? "user" : CurrentUser.UserName.Trim().ToLowerInvariant();
                var clean = new string(rawName.Where(char.IsLetterOrDigit).ToArray());
                if (string.IsNullOrEmpty(clean)) clean = "user";
                return $"u_{CurrentUser.Id}_{clean}";
            }
            return "guest";
        }

        private static string GetBaseStorageDir()
        {
            string currentDir = Directory.GetCurrentDirectory();
            string targetDir;
            if (File.Exists(Path.Combine(currentDir, "Program.cs")))
            {
                targetDir = Path.Combine(currentDir, "Storage");
            }
            else
            {
                string baseDir = AppContext.BaseDirectory;
                string probe = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));
                if (File.Exists(Path.Combine(probe, "Program.cs")))
                {
                    targetDir = Path.Combine(probe, "Storage");
                }
                else
                {
                    targetDir = Path.Combine(baseDir, "Storage");
                }
            }

            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }
            return targetDir;
        }

        private static string GetSessionFilePath()
        {
            return Path.Combine(GetBaseStorageDir(), "auth_session.json");
        }

        private static string GetUsersFilePath()
        {
            return Path.Combine(GetBaseStorageDir(), "users.json");
        }

        private void EnsureInitialized()
        {
            if (_isInitialized) return;

            LoadLocalAccounts();

            // Always default to Guest on startup (No auto-login)
            _currentUser = new CurrentUserDto();
            _isInitialized = true;
        }

        private static void LoadLocalAccounts()
        {
            lock (_accountsLock)
            {
                _accounts.Clear();
                var usersPath = GetUsersFilePath();

                if (File.Exists(usersPath))
                {
                    try
                    {
                        var json = File.ReadAllText(usersPath);
                        var loaded = JsonSerializer.Deserialize<List<StoredUserAccount>>(json);
                        if (loaded != null && loaded.Any())
                        {
                            _accounts.AddRange(loaded);
                        }
                    }
                    catch { }
                }
            }
        }

        private static void SaveAccountsToFile()
        {
            try
            {
                var usersPath = GetUsersFilePath();
                var json = JsonSerializer.Serialize(_accounts, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(usersPath, json);
            }
            catch { }
        }

        private void SaveSession()
        {
            // In-memory session: do not write to file so project restarts always start clean as Guest
        }

        private void ClearSessionFile()
        {
            try
            {
                var path = GetSessionFilePath();
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch { }
        }

        public async Task<UserLoginResponseModel> LoginAsync(string usernameOrEmail, string password)
        {
            if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(password))
            {
                return new UserLoginResponseModel
                {
                    IsSuccess = false,
                    Message = "Username or Email and password are required."
                };
            }

            var cleanInput = usernameOrEmail.Trim();
            var inputNoSpace = cleanInput.Replace(" ", "").ToLowerInvariant();
            var cleanPwd = password.Trim();

            // 1. Direct Authentication against SQL Server Users table via AppDbContext
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetService<AppDbContext>();
                if (db != null)
                {
                    var users = await db.Users.Where(u => !u.IsDeleted).ToListAsync();
                    var matched = users.FirstOrDefault(u =>
                        (string.Equals(u.UserName?.Trim(), cleanInput, StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(u.DisplayName?.Trim(), cleanInput, StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(u.Email?.Trim(), cleanInput, StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(u.UserName?.Replace(" ", "").Trim(), inputNoSpace, StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(u.DisplayName?.Replace(" ", "").Trim(), inputNoSpace, StringComparison.OrdinalIgnoreCase)) &&
                        string.Equals(u.Password?.Trim(), cleanPwd, StringComparison.Ordinal));

                    if (matched != null)
                    {
                        var role = !string.IsNullOrWhiteSpace(matched.Role)
                            ? matched.Role.Trim()
                            : (matched.UserName.Equals("admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "User");

                        _currentUser = new CurrentUserDto
                        {
                            Id = matched.Id,
                            UserName = matched.UserName,
                            DisplayName = !string.IsNullOrWhiteSpace(matched.DisplayName) ? matched.DisplayName : matched.UserName,
                            Email = matched.Email,
                            Role = role,
                            LoggedInAt = DateTime.UtcNow
                        };

                        SaveSession();
                        OnAuthStateChanged?.Invoke();

                        return new UserLoginResponseModel
                        {
                            IsSuccess = true,
                            Message = $"{_currentUser.Role} login successful.",
                            Id = matched.Id,
                            UserName = matched.UserName,
                            DisplayName = _currentUser.DisplayName,
                            Email = matched.Email,
                            Role = _currentUser.Role
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuthService] Direct DB Login Exception: {ex.Message}");
            }

            // 2. Try WebApi Authentication
            try
            {
                var req = new UserLoginRequestModel
                {
                    Email = cleanInput,
                    Password = cleanPwd
                };

                var res = await _api.LoginUserAsync(req);
                if (res != null && res.IsSuccess)
                {
                    bool isAdmin = res.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                                   cleanInput.Equals("admin", StringComparison.OrdinalIgnoreCase) ||
                                   (res.UserName != null && res.UserName.Equals("admin", StringComparison.OrdinalIgnoreCase));

                    _currentUser = new CurrentUserDto
                    {
                        Id = res.Id,
                        UserName = res.UserName ?? cleanInput,
                        DisplayName = !string.IsNullOrWhiteSpace(res.DisplayName) ? res.DisplayName : (res.UserName ?? cleanInput),
                        Email = res.Email ?? cleanInput,
                        Role = isAdmin ? "Admin" : "User",
                        LoggedInAt = DateTime.UtcNow
                    };

                    SaveSession();
                    OnAuthStateChanged?.Invoke();
                    return res;
                }
            }
            catch { }

            // 3. Fallback to Local Persistent File (users.json)
            lock (_accountsLock)
            {
                var matchedUser = _accounts.FirstOrDefault(u =>
                    string.Equals(u.Password.Trim(), cleanPwd, StringComparison.Ordinal) &&
                    (
                        u.UserName.Equals(cleanInput, StringComparison.OrdinalIgnoreCase) ||
                        u.DisplayName.Equals(cleanInput, StringComparison.OrdinalIgnoreCase) ||
                        u.Email.Equals(cleanInput, StringComparison.OrdinalIgnoreCase) ||
                        u.UserName.Replace(" ", "").Equals(inputNoSpace, StringComparison.OrdinalIgnoreCase) ||
                        u.DisplayName.Replace(" ", "").Equals(inputNoSpace, StringComparison.OrdinalIgnoreCase) ||
                        u.Email.Replace(" ", "").Equals(inputNoSpace, StringComparison.OrdinalIgnoreCase)
                    ));

                if (matchedUser != null)
                {
                    bool isAdmin = matchedUser.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                                   matchedUser.UserName.Equals("admin", StringComparison.OrdinalIgnoreCase);

                    _currentUser = new CurrentUserDto
                    {
                        Id = matchedUser.Id,
                        UserName = matchedUser.UserName,
                        DisplayName = !string.IsNullOrWhiteSpace(matchedUser.DisplayName) ? matchedUser.DisplayName : matchedUser.UserName,
                        Email = matchedUser.Email,
                        Role = isAdmin ? "Admin" : "User",
                        LoggedInAt = DateTime.UtcNow
                    };

                    SaveSession();
                    OnAuthStateChanged?.Invoke();

                    return new UserLoginResponseModel
                    {
                        IsSuccess = true,
                        Message = $"{matchedUser.Role} login successful.",
                        Id = matchedUser.Id,
                        UserName = matchedUser.UserName,
                        DisplayName = _currentUser.DisplayName,
                        Email = matchedUser.Email,
                        Role = _currentUser.Role
                    };
                }
            }

            return new UserLoginResponseModel
            {
                IsSuccess = false,
                Message = "Invalid username or password. Please try again."
            };
        }

        public async Task<UserCreateResponseModel> RegisterAsync(string username, string displayName, string email, string password, string role = "User")
        {
            var cleanUName = username.Trim();
            var cleanDName = string.IsNullOrWhiteSpace(displayName) ? cleanUName : displayName.Trim();
            var cleanMail = email.Trim();
            var cleanPass = password.Trim();
            var cleanRole = string.IsNullOrWhiteSpace(role) ? "User" : role.Trim();

            // 1. Direct Insert into SQL Server Users table directly
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetService<AppDbContext>();
                if (db != null)
                {
                    var existing = await db.Users.FirstOrDefaultAsync(u =>
                        !u.IsDeleted &&
                        (u.UserName == cleanUName || u.Email == cleanMail));

                    if (existing != null)
                    {
                        return new UserCreateResponseModel
                        {
                            IsSuccess = false,
                            Message = "Username or email is already registered in database."
                        };
                    }

                    var dbUser = new DotNetProject.Database.AppDbModels.User
                    {
                        UserName = cleanUName,
                        DisplayName = cleanDName,
                        Email = cleanMail,
                        Password = cleanPass,
                        Role = cleanUName.Equals("admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : cleanRole,
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    db.Users.Add(dbUser);
                    await db.SaveChangesAsync();

                    // Also mirror into local persistent file
                    lock (_accountsLock)
                    {
                        _accounts.Add(new StoredUserAccount
                        {
                            Id = dbUser.Id,
                            UserName = cleanUName,
                            DisplayName = cleanDName,
                            Email = cleanMail,
                            Password = cleanPass,
                            Role = dbUser.Role,
                            CreatedAt = DateTime.UtcNow
                        });
                        SaveAccountsToFile();
                    }

                    return new UserCreateResponseModel
                    {
                        IsSuccess = true,
                        Message = "Account registered successfully in database."
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuthService] Direct DB Register Exception: {ex.Message}");
            }

            // 2. Fallback to WebApi registration if direct DB scope was unavailable
            try
            {
                var req = new UserCreateRequestModel
                {
                    UserName = cleanUName,
                    DisplayName = cleanDName,
                    Email = cleanMail,
                    Password = cleanPass
                };
                var apiRes = await _api.RegisterUserAsync(req);
                if (apiRes != null && apiRes.IsSuccess)
                {
                    return apiRes;
                }
            }
            catch { }

            // 3. Fallback to local storage registration
            lock (_accountsLock)
            {
                int nextId = _accounts.Any() ? _accounts.Max(a => a.Id) + 1 : 2000;
                var existing = _accounts.FirstOrDefault(a =>
                    a.UserName.Equals(cleanUName, StringComparison.OrdinalIgnoreCase) ||
                    a.Email.Equals(cleanMail, StringComparison.OrdinalIgnoreCase));

                if (existing != null)
                {
                    return new UserCreateResponseModel
                    {
                        IsSuccess = false,
                        Message = "Username or email is already registered."
                    };
                }

                _accounts.Add(new StoredUserAccount
                {
                    Id = nextId,
                    UserName = cleanUName,
                    DisplayName = cleanDName,
                    Email = cleanMail,
                    Password = cleanPass,
                    Role = cleanUName.Equals("admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : cleanRole,
                    CreatedAt = DateTime.UtcNow
                });
                SaveAccountsToFile();
            }

            return new UserCreateResponseModel
            {
                IsSuccess = true,
                Message = "Registration successful! You can now log in."
            };
        }

        // Methods for Admin User & Role Management
        public async Task<List<User>> GetAllUsersFromDbAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetService<AppDbContext>();
                if (db != null)
                {
                    return await db.Users
                        .OrderByDescending(u => u.Id)
                        .ToListAsync();
                }
            }
            catch { }

            return new List<User>();
        }

        public async Task<bool> UpdateUserRoleInDbAsync(int userId, string newRole)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetService<AppDbContext>();
                if (db != null)
                {
                    var user = await db.Users.FindAsync(userId);
                    if (user != null)
                    {
                        user.Role = newRole;
                        await db.SaveChangesAsync();

                        // Sync session if updating active user
                        if (_currentUser.Id == userId)
                        {
                            _currentUser.Role = newRole;
                            SaveSession();
                            OnAuthStateChanged?.Invoke();
                        }
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        public async Task<bool> ToggleUserDeletedInDbAsync(int userId)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetService<AppDbContext>();
                if (db != null)
                {
                    var user = await db.Users.FindAsync(userId);
                    if (user != null)
                    {
                        user.IsDeleted = !user.IsDeleted;
                        user.DeletedAt = user.IsDeleted ? DateTime.UtcNow : null;
                        await db.SaveChangesAsync();
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        public Task LogoutAsync()
        {
            _currentUser = new CurrentUserDto();
            ClearSessionFile();
            OnAuthStateChanged?.Invoke();
            return Task.CompletedTask;
        }
    }
}
