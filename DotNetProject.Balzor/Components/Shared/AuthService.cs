using DotNetProject.Domain.Features.Users;
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

            LoadAccounts();

            try
            {
                var sessionPath = GetSessionFilePath();
                if (File.Exists(sessionPath))
                {
                    var json = File.ReadAllText(sessionPath);
                    var user = JsonSerializer.Deserialize<CurrentUserDto>(json);
                    if (user != null && user.IsAuthenticated)
                    {
                        _currentUser = user;
                        _isInitialized = true;
                        return;
                    }
                }
            }
            catch { }

            _currentUser = new CurrentUserDto();
            _isInitialized = true;
        }

        private static void LoadAccounts()
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

                bool needsSave = false;

                // Ensure Admin exists
                if (!_accounts.Any(a => a.UserName.Equals("admin", StringComparison.OrdinalIgnoreCase)))
                {
                    _accounts.Add(new StoredUserAccount
                    {
                        Id = 1005,
                        UserName = "admin",
                        DisplayName = "System Admin",
                        Email = "admin@nangworld.com",
                        Password = "admin123",
                        Role = "Admin"
                    });
                    needsSave = true;
                }

                // Ensure Nang Aye Han exists with the user's password
                if (!_accounts.Any(a => a.DisplayName.Equals("Nang Aye Han", StringComparison.OrdinalIgnoreCase) ||
                                        a.UserName.Equals("Nang Aye Han", StringComparison.OrdinalIgnoreCase) ||
                                        a.UserName.Equals("nangayehan", StringComparison.OrdinalIgnoreCase)))
                {
                    _accounts.Add(new StoredUserAccount
                    {
                        Id = 1001,
                        UserName = "Nang Aye Han",
                        DisplayName = "Nang Aye Han",
                        Email = "nanayemyomyomyathan@gmail.com",
                        Password = "Ayehan@182003",
                        Role = "User"
                    });
                    needsSave = true;
                }
                else
                {
                    // Ensure password matches the user's provided password
                    var userAcc = _accounts.FirstOrDefault(a => a.DisplayName.Equals("Nang Aye Han", StringComparison.OrdinalIgnoreCase) ||
                                                               a.UserName.Equals("Nang Aye Han", StringComparison.OrdinalIgnoreCase) ||
                                                               a.UserName.Equals("nangayehan", StringComparison.OrdinalIgnoreCase));
                    if (userAcc != null && userAcc.Password != "Ayehan@182003")
                    {
                        userAcc.Password = "Ayehan@182003";
                        needsSave = true;
                    }
                }

                // Ensure demo Nang exists
                if (!_accounts.Any(a => a.UserName.Equals("nang", StringComparison.OrdinalIgnoreCase)))
                {
                    _accounts.Add(new StoredUserAccount
                    {
                        Id = 1003,
                        UserName = "nang",
                        DisplayName = "Nang",
                        Email = "nang@gmail.com",
                        Password = "1234",
                        Role = "User"
                    });
                    needsSave = true;
                }

                // Ensure demo Su Su exists
                if (!_accounts.Any(a => a.UserName.Equals("susu", StringComparison.OrdinalIgnoreCase)))
                {
                    _accounts.Add(new StoredUserAccount
                    {
                        Id = 1002,
                        UserName = "susu",
                        DisplayName = "Su Su",
                        Email = "su@gmail.com",
                        Password = "susu",
                        Role = "User"
                    });
                    needsSave = true;
                }

                if (needsSave)
                {
                    SaveAccountsToFile();
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
                    Message = "Username and password are required."
                };
            }

            var cleanInput = usernameOrEmail.Trim();
            var cleanPwd = password.Trim();

            // 1. Try Authenticating via WebApi
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
                        UserName = res.UserName,
                        DisplayName = !string.IsNullOrWhiteSpace(res.DisplayName) ? res.DisplayName : res.UserName,
                        Email = res.Email,
                        Role = isAdmin ? "Admin" : "User",
                        LoggedInAt = DateTime.UtcNow
                    };

                    SaveSession();
                    OnAuthStateChanged?.Invoke();
                    return res;
                }
            }
            catch { }

            // 2. Local File-Persisted Storage Authentication
            lock (_accountsLock)
            {
                var inputNoSpaces = cleanInput.Replace(" ", "").ToLowerInvariant();

                var matchedUser = _accounts.FirstOrDefault(u =>
                    string.Equals(u.Password.Trim(), cleanPwd, StringComparison.Ordinal) &&
                    (
                        u.UserName.Equals(cleanInput, StringComparison.OrdinalIgnoreCase) ||
                        u.DisplayName.Equals(cleanInput, StringComparison.OrdinalIgnoreCase) ||
                        u.Email.Equals(cleanInput, StringComparison.OrdinalIgnoreCase) ||
                        u.UserName.Replace(" ", "").Equals(inputNoSpaces, StringComparison.OrdinalIgnoreCase) ||
                        u.DisplayName.Replace(" ", "").Equals(inputNoSpaces, StringComparison.OrdinalIgnoreCase) ||
                        u.Email.Replace(" ", "").Equals(inputNoSpaces, StringComparison.OrdinalIgnoreCase)
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

        public async Task<UserCreateResponseModel> RegisterAsync(string username, string displayName, string email, string password)
        {
            var cleanUName = username.Trim();
            var cleanDName = string.IsNullOrWhiteSpace(displayName) ? cleanUName : displayName.Trim();
            var cleanMail = email.Trim();
            var cleanPass = password.Trim();

            int nextId;
            lock (_accountsLock)
            {
                nextId = _accounts.Any() ? _accounts.Max(a => a.Id) + 1 : 2000;
                var existing = _accounts.FirstOrDefault(a =>
                    a.UserName.Equals(cleanUName, StringComparison.OrdinalIgnoreCase) ||
                    a.Email.Equals(cleanMail, StringComparison.OrdinalIgnoreCase) ||
                    a.DisplayName.Equals(cleanDName, StringComparison.OrdinalIgnoreCase));

                if (existing != null)
                {
                    existing.Password = cleanPass;
                    existing.DisplayName = cleanDName;
                    existing.Email = cleanMail;
                }
                else
                {
                    _accounts.Add(new StoredUserAccount
                    {
                        Id = nextId,
                        UserName = cleanUName,
                        DisplayName = cleanDName,
                        Email = cleanMail,
                        Password = cleanPass,
                        Role = cleanUName.Equals("admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "User",
                        CreatedAt = DateTime.UtcNow
                    });
                }

                SaveAccountsToFile();
            }

            // Also forward to WebApi in background if available
            try
            {
                var req = new UserCreateRequestModel
                {
                    UserName = cleanUName,
                    DisplayName = cleanDName,
                    Email = cleanMail,
                    Password = cleanPass
                };
                await _api.RegisterUserAsync(req);
            }
            catch { }

            return new UserCreateResponseModel
            {
                IsSuccess = true,
                Message = "Registration successful! You can now log in."
            };
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
