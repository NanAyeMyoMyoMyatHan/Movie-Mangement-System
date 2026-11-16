using DotNetProject.Database.AppDbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetProject.Balzor.Components.Shared
{
    public class RoleDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsSystemRole { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public int UserCount { get; set; }
        public List<string> PermissionCodes { get; set; } = new();
        public List<int> PermissionIds { get; set; } = new();
    }

    public class PermissionDto
    {
        public int PermissionId { get; set; }
        public string PermissionCode { get; set; } = string.Empty;
        public string PermissionName { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsGranted { get; set; }
    }

    public class SaveRoleRequest
    {
        public int? RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public List<int> PermissionIds { get; set; } = new();
    }

    public class UserRoleAssignmentDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<int> AssignedRoleIds { get; set; } = new();
        public List<string> AssignedRoleNames { get; set; } = new();
    }

    public class RbacService
    {
        private readonly IServiceProvider _serviceProvider;

        public RbacService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private sealed class DbScope : IDisposable, IAsyncDisposable
        {
            private readonly IServiceScope _scope;
            public AppDbContext Db { get; }

            public DbScope(IServiceProvider sp)
            {
                _scope = sp.CreateScope();
                Db = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
            }

            public void Dispose() => _scope.Dispose();

            public ValueTask DisposeAsync()
            {
                if (_scope is IAsyncDisposable ad) return ad.DisposeAsync();
                _scope.Dispose();
                return ValueTask.CompletedTask;
            }
        }

        public async Task<List<RoleDto>> GetRolesAsync()
        {
            using var dbScope = new DbScope(_serviceProvider);
            var db = dbScope.Db;
            var roles = await db.Roles
                .AsNoTracking()
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .Include(r => r.UserRoles)
                .OrderBy(r => r.IsSystemRole ? 0 : 1)
                .ThenBy(r => r.RoleId)
                .ToListAsync();

            return roles.Select(r => new RoleDto
            {
                RoleId = r.RoleId,
                RoleName = r.RoleName,
                Description = r.Description,
                IsSystemRole = r.IsSystemRole,
                IsActive = r.IsActive,
                CreatedAt = r.CreatedAt,
                UserCount = r.UserRoles.Count,
                PermissionCodes = r.RolePermissions.Select(rp => rp.Permission.PermissionCode).ToList(),
                PermissionIds = r.RolePermissions.Select(rp => rp.PermissionId).ToList()
            }).ToList();
        }

        public async Task<List<PermissionDto>> GetAllPermissionsAsync()
        {
            using var dbScope = new DbScope(_serviceProvider);
            var db = dbScope.Db;
            var perms = await db.Permissions
                .AsNoTracking()
                .OrderBy(p => p.Module)
                .ThenBy(p => p.PermissionCode)
                .ToListAsync();

            return perms.Select(p => new PermissionDto
            {
                PermissionId = p.PermissionId,
                PermissionCode = p.PermissionCode,
                PermissionName = p.PermissionName,
                Module = p.Module,
                Description = p.Description
            }).ToList();
        }

        public async Task<Common.Result<RoleDto>> SaveRoleAsync(SaveRoleRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.RoleName))
                return Common.Result<RoleDto>.Error("Role name is required.");

            using var dbScope = new DbScope(_serviceProvider);
            var db = dbScope.Db;

            var cleanName = req.RoleName.Trim();
            Role? role;

            if (req.RoleId.HasValue && req.RoleId.Value > 0)
            {
                role = await db.Roles
                    .Include(r => r.RolePermissions)
                    .FirstOrDefaultAsync(r => r.RoleId == req.RoleId.Value);

                if (role == null)
                    return Common.Result<RoleDto>.Error("Role not found.");

                // If role name is changing, check uniqueness
                if (!role.RoleName.Equals(cleanName, StringComparison.OrdinalIgnoreCase))
                {
                    if (role.IsSystemRole)
                        return Common.Result<RoleDto>.Error("System role names cannot be renamed.");

                    bool exists = await db.Roles.AnyAsync(r => r.RoleName == cleanName && r.RoleId != role.RoleId);
                    if (exists)
                        return Common.Result<RoleDto>.Error($"Role name '{cleanName}' already exists.");

                    role.RoleName = cleanName;
                }

                role.Description = req.Description?.Trim();
                role.IsActive = req.IsActive;

                // Update Permissions (clear existing, insert new)
                db.RolePermissions.RemoveRange(role.RolePermissions);
                if (req.PermissionIds != null && req.PermissionIds.Any())
                {
                    foreach (var permId in req.PermissionIds.Distinct())
                    {
                        db.RolePermissions.Add(new RolePermission
                        {
                            RoleId = role.RoleId,
                            PermissionId = permId,
                            AssignedAt = DateTime.UtcNow
                        });
                    }
                }
            }
            else
            {
                // Create new role
                bool exists = await db.Roles.AnyAsync(r => r.RoleName == cleanName);
                if (exists)
                    return Common.Result<RoleDto>.Error($"Role '{cleanName}' already exists.");

                role = new Role
                {
                    RoleName = cleanName,
                    Description = req.Description?.Trim(),
                    IsSystemRole = false,
                    IsActive = req.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                db.Roles.Add(role);
                await db.SaveChangesAsync();

                if (req.PermissionIds != null && req.PermissionIds.Any())
                {
                    foreach (var permId in req.PermissionIds.Distinct())
                    {
                        db.RolePermissions.Add(new RolePermission
                        {
                            RoleId = role.RoleId,
                            PermissionId = permId,
                            AssignedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            await db.SaveChangesAsync();

            return Common.Result<RoleDto>.Success(new RoleDto
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                Description = role.Description,
                IsSystemRole = role.IsSystemRole,
                IsActive = role.IsActive
            }, "Role saved successfully.");
        }

        public async Task<Common.Result<bool>> DeleteRoleAsync(int roleId)
        {
            using var dbScope = new DbScope(_serviceProvider);
            var db = dbScope.Db;
            var role = await db.Roles
                .Include(r => r.UserRoles)
                .FirstOrDefaultAsync(r => r.RoleId == roleId);

            if (role == null)
                return Common.Result<bool>.Error("Role not found.");

            if (role.IsSystemRole)
                return Common.Result<bool>.Error("System roles (SuperAdmin, Admin, User) cannot be deleted.");

            if (role.UserRoles.Any())
                return Common.Result<bool>.Error($"Cannot delete role '{role.RoleName}' because {role.UserRoles.Count} user(s) are currently assigned to it. Please reassign those users first.");

            db.Roles.Remove(role);
            await db.SaveChangesAsync();

            return Common.Result<bool>.Success(true, $"Role '{role.RoleName}' deleted successfully.");
        }

        public async Task<List<int>> GetUserRoleIdsAsync(int userId)
        {
            using var dbScope = new DbScope(_serviceProvider);
            var db = dbScope.Db;
            return await db.UserRoles
                .AsNoTracking()
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync();
        }

        public async Task<List<string>> GetUserRoleNamesAsync(int userId)
        {
            using var dbScope = new DbScope(_serviceProvider);
            var db = dbScope.Db;
            return await db.UserRoles
                .AsNoTracking()
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Role.RoleName)
                .ToListAsync();
        }

        public async Task<HashSet<string>> GetUserPermissionsAsync(int userId)
        {
            using var dbScope = new DbScope(_serviceProvider);
            var db = dbScope.Db;
            var roleIds = await db.UserRoles
                .AsNoTracking()
                .Where(ur => ur.UserId == userId && ur.Role.IsActive)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            if (!roleIds.Any())
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // If user has SuperAdmin role, grant all permissions plus wildcard
            bool isSuperAdmin = await db.Roles
                .AnyAsync(r => roleIds.Contains(r.RoleId) && r.RoleName == "SuperAdmin");

            if (isSuperAdmin)
            {
                var allPerms = await db.Permissions.Select(p => p.PermissionCode).ToListAsync();
                var set = new HashSet<string>(allPerms, StringComparer.OrdinalIgnoreCase) { "*" };
                return set;
            }

            var permissions = await db.RolePermissions
                .AsNoTracking()
                .Where(rp => roleIds.Contains(rp.RoleId))
                .Select(rp => rp.Permission.PermissionCode)
                .Distinct()
                .ToListAsync();

            return new HashSet<string>(permissions, StringComparer.OrdinalIgnoreCase);
        }

        public async Task<Common.Result<bool>> AssignUserRolesAsync(int userId, List<int> roleIds)
        {
            using var dbScope = new DbScope(_serviceProvider);
            var db = dbScope.Db;
            var user = await db.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return Common.Result<bool>.Error("User not found.");

            // Clear old roles
            db.UserRoles.RemoveRange(user.UserRoles);

            if (roleIds != null && roleIds.Any())
            {
                foreach (var rId in roleIds.Distinct())
                {
                    db.UserRoles.Add(new UserRole
                    {
                        UserId = userId,
                        RoleId = rId,
                        AssignedAt = DateTime.UtcNow
                    });
                }

                // Update fallback User.Role string column to primary role for legacy compatibility
                var primaryRole = await db.Roles.FirstOrDefaultAsync(r => r.RoleId == roleIds.First());
                if (primaryRole != null)
                {
                    user.Role = primaryRole.RoleName;
                }
            }
            else
            {
                // Fallback to standard User role if no role selected
                var defaultRole = await db.Roles.FirstOrDefaultAsync(r => r.RoleName == "User");
                if (defaultRole != null)
                {
                    db.UserRoles.Add(new UserRole
                    {
                        UserId = userId,
                        RoleId = defaultRole.RoleId,
                        AssignedAt = DateTime.UtcNow
                    });
                    user.Role = "User";
                }
            }

            await db.SaveChangesAsync();
            return Common.Result<bool>.Success(true, "User roles updated successfully.");
        }
    }
}
