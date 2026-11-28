using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetProject.Domain.Features.Users
{
    public class CurrentUserDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "Guest";
        public List<string> Roles { get; set; } = new();
        public HashSet<string> Permissions { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public DateTime LoggedInAt { get; set; } = DateTime.UtcNow;

        public bool IsAuthenticated => Id > 0 && !string.IsNullOrWhiteSpace(UserName) && !Role.Equals("Guest", StringComparison.OrdinalIgnoreCase);
        public bool IsAdmin => IsAuthenticated && (Role.Equals("Admin", StringComparison.OrdinalIgnoreCase) || Role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) || Roles.Any(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase) || r.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase)));

        public bool HasPermission(string code) =>
            IsAdmin || Permissions.Contains("*") || Permissions.Contains(code);

        public bool HasRole(string roleName) =>
            Roles.Any(r => r.Equals(roleName, StringComparison.OrdinalIgnoreCase)) || Role.Equals(roleName, StringComparison.OrdinalIgnoreCase);
    }

    public class StoredUserAccount
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
