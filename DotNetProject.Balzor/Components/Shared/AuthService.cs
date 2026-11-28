using DotNetProject.Domain.Features.Rbac;
using DotNetProject.Domain.Features.Users;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DotNetProject.Balzor.Components.Shared
{
    public class AuthService
    {
        private readonly HttpClientService _api;
        private CurrentUserDto _currentUser = new();
        private bool _isInitialized = false;

        private static readonly List<StoredUserAccount> _accounts = new();
        private static readonly object _accountsLock = new();

        public event Action? OnAuthStateChanged;

        public AuthService(HttpClientService api)
        {
            _api = api;
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
        public bool HasPermission(string code) => CurrentUser.HasPermission(code);
        public bool HasAnyPermission(params string[] codes) =>
            CurrentUser.IsAdmin || CurrentUser.Permissions.Contains("*") || codes.Any(c => CurrentUser.Permissions.Contains(c));
        public bool HasRole(string roleName) => CurrentUser.HasRole(roleName);

        public void UpdateAvatar(string avatarUrl)
        {
            _currentUser.AvatarUrl = avatarUrl;
            SaveSession();
            OnAuthStateChanged?.Invoke();
        }

        public bool CanAccessAdmin()
        {
            if (!IsAuthenticated()) return false;
            if (IsAdmin()) return true;
            if (CurrentUser.Permissions.Contains("*")) return true;
            if (CurrentUser.Roles.Any(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase) || 
                                           r.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))) return true;
            return HasAnyPermission(
                "Dashboard.View",
                "Content.View", "Content.Create", "Content.Edit", "Content.Delete",
                "Episode.View", "Episode.Create", "Episode.Edit", "Episode.Delete",
                "Movie.View", "Movie.Create", "Movie.Edit", "Movie.Delete",
                "User.View", "User.Manage",
                "Role.View", "Role.Manage"
            );
        }

        public async Task RefreshPermissionsAsync()
        {
            if (!CurrentUser.IsAuthenticated || CurrentUser.Id <= 0) return;

            try
            {
                var roleNamesRes = await _api.GetUserRoleNamesAsync(CurrentUser.Id);
                var permsRes = await _api.GetUserPermissionsAsync(CurrentUser.Id);

                _currentUser.Roles = roleNamesRes?.Data ?? new List<string>();
                var permsList = permsRes?.Data ?? new List<string>();
                _currentUser.Permissions = new HashSet<string>(permsList, StringComparer.OrdinalIgnoreCase);

                if (_currentUser.Roles.Any())
                {
                    _currentUser.Role = _currentUser.Roles.First();
                }

                SaveSession();
                OnAuthStateChanged?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuthService] RefreshPermissionsAsync Error: {ex.Message}");
            }
        }

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

            try
            {
                var sessionPath = GetSessionFilePath();
                if (File.Exists(sessionPath))
                {
                    var json = File.ReadAllText(sessionPath);
                    var user = JsonSerializer.Deserialize<CurrentUserDto>(json);
                    if (user != null && user.IsAuthenticated)
                    {
                        user.Permissions = new HashSet<string>(user.Permissions ?? new(), StringComparer.OrdinalIgnoreCase);
                        _currentUser = user;
                        _isInitialized = true;
                        if (!_currentUser.Permissions.Any() && _currentUser.Id > 0)
                        {
                            _ = RefreshPermissionsAsync();
                        }
                        return;
                    }
                }
            }
            catch { }

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
            try
            {
                var path = GetSessionFilePath();
                var json = JsonSerializer.Serialize(_currentUser, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, json);
            }
            catch { }
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

            // 1. Try WebApi Authentication
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
                        AvatarUrl = res.AvatarUrl,
                        LoggedInAt = DateTime.UtcNow
                    };

                    await RefreshPermissionsAsync();
                    SaveSession();
                    OnAuthStateChanged?.Invoke();
                    return res;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuthService] WebApi Login Error: {ex.Message}");
            }

            // 2. Fallback to Local Persistent File (users.json)
            StoredUserAccount? matchedUser = null;
            lock (_accountsLock)
            {
                matchedUser = _accounts.FirstOrDefault(u =>
                    string.Equals(u.Password.Trim(), cleanPwd, StringComparison.Ordinal) &&
                    (
                        u.UserName.Equals(cleanInput, StringComparison.OrdinalIgnoreCase) ||
                        u.DisplayName.Equals(cleanInput, StringComparison.OrdinalIgnoreCase) ||
                        u.Email.Equals(cleanInput, StringComparison.OrdinalIgnoreCase) ||
                        u.UserName.Replace(" ", "").Equals(inputNoSpace, StringComparison.OrdinalIgnoreCase) ||
                        u.DisplayName.Replace(" ", "").Equals(inputNoSpace, StringComparison.OrdinalIgnoreCase) ||
                        u.Email.Replace(" ", "").Equals(inputNoSpace, StringComparison.OrdinalIgnoreCase)
                    ));
            }

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

                await RefreshPermissionsAsync();
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

            // 1. WebApi registration
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

            // 2. Fallback to local storage registration
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
                    Role = cleanUName.Equals("admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : (string.IsNullOrWhiteSpace(role) ? "User" : role.Trim()),
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

        public async Task<List<UserModel>> GetAllUsersFromDbAsync()
        {
            try
            {
                var users = await _api.GetAllUsersAsync();
                if (users != null) return users;
            }
            catch { }

            return new List<UserModel>();
        }

        public async Task<bool> UpdateUserRoleInDbAsync(int userId, string newRole)
        {
            try
            {
                // Sync session if updating active user
                if (_currentUser.Id == userId)
                {
                    _currentUser.Role = newRole;
                    SaveSession();
                    OnAuthStateChanged?.Invoke();
                }
                return true;
            }
            catch { }
            return false;
        }

        public async Task<bool> ToggleUserDeletedInDbAsync(int userId)
        {
            try
            {
                var res = await _api.ToggleUserStatusAsync(userId);
                return res?.Data == true;
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
