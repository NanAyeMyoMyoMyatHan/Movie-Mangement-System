using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using DotNetProject.Database.AppDbModels;
using DotNetProject.Domain.Features.Rbac;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetProject.Balzor.Components.Shared
{
    public class RolePermissionService : IRolePermissionService
    {
        private readonly HttpClientService _http;
        private readonly IServiceProvider _serviceProvider;

        public RolePermissionService(HttpClientService http, IServiceProvider serviceProvider)
        {
            _http = http;
            _serviceProvider = serviceProvider;
        }

        // --- IRolePermissionService implementation via WebApi with resilient DbContext fallback ---

        public async Task<Result<List<RoleDto>>> GetRolesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var apiRes = await _http.GetRolesAsync();
                if (apiRes != null) return apiRes;
            }
            catch { /* fallback to DB below if WebApi is offline */ }

            return await FallbackGetRolesAsync(cancellationToken);
        }

        public async Task<List<RoleDto>> GetRolesListAsync(CancellationToken cancellationToken = default)
        {
            var res = await GetRolesAsync(cancellationToken);
            return res.Data ?? new List<RoleDto>();
        }

        public async Task<Result<List<PermissionDto>>> GetAllPermissionsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var apiRes = await _http.GetAllPermissionsAsync();
                if (apiRes != null) return apiRes;
            }
            catch { /* fallback to DB below if WebApi is offline */ }

            return await FallbackGetAllPermissionsAsync(cancellationToken);
        }

        public async Task<List<PermissionDto>> GetAllPermissionsListAsync(CancellationToken cancellationToken = default)
        {
            var res = await GetAllPermissionsAsync(cancellationToken);
            return res.Data ?? new List<PermissionDto>();
        }

        public async Task<Result<RoleDto>> SaveRoleAsync(SaveRoleRequest req, CancellationToken cancellationToken = default)
        {
            try
            {
                var apiRes = await _http.SaveRoleAsync(req);
                if (apiRes != null) return apiRes;
            }
            catch { /* fallback to DB below */ }

            return await FallbackSaveRoleAsync(req, cancellationToken);
        }

        public async Task<Result<bool>> DeleteRoleAsync(int roleId, CancellationToken cancellationToken = default)
        {
            try
            {
                var apiRes = await _http.DeleteRoleAsync(roleId);
                if (apiRes != null) return apiRes;
            }
            catch { /* fallback to DB below */ }

            return await FallbackDeleteRoleAsync(roleId, cancellationToken);
        }

        public async Task<Result<List<int>>> GetUserRoleIdsAsync(int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var apiRes = await _http.GetUserRoleIdsAsync(userId);
                if (apiRes != null) return apiRes;
            }
            catch { /* fallback to DB below */ }

            return await FallbackGetUserRoleIdsAsync(userId, cancellationToken);
        }

        public async Task<Result<List<string>>> GetUserRoleNamesAsync(int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var apiRes = await _http.GetUserRoleNamesAsync(userId);
                if (apiRes != null) return apiRes;
            }
            catch { /* fallback to DB below */ }

            return await FallbackGetUserRoleNamesAsync(userId, cancellationToken);
        }

        public async Task<Result<List<string>>> GetUserPermissionsAsync(int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var apiRes = await _http.GetUserPermissionsAsync(userId);
                if (apiRes != null) return apiRes;
            }
            catch { /* fallback to DB below */ }

            return await FallbackGetUserPermissionsAsync(userId, cancellationToken);
        }

        public async Task<HashSet<string>> GetUserPermissionSetAsync(int userId)
        {
            var res = await GetUserPermissionsAsync(userId);
            if (res.IsSuccess && res.Data != null)
            {
                return new HashSet<string>(res.Data, StringComparer.OrdinalIgnoreCase);
            }
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public async Task<Result<bool>> AssignUserRolesAsync(AssignUserRolesRequest req, CancellationToken cancellationToken = default)
        {
            try
            {
                var apiRes = await _http.AssignUserRolesAsync(req);
                if (apiRes != null) return apiRes;
            }
            catch { /* fallback to DB below */ }

            return await FallbackAssignUserRolesAsync(req, cancellationToken);
        }

        public Task<Result<bool>> AssignUserRolesAsync(int userId, List<int> roleIds) =>
            AssignUserRolesAsync(new AssignUserRolesRequest { UserId = userId, RoleIds = roleIds });

        public async Task<Result<List<UserRoleAssignmentDto>>> GetUserAssignmentsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var apiRes = await _http.GetUserAssignmentsAsync();
                if (apiRes != null) return apiRes;
            }
            catch { /* fallback to DB below */ }

            return await FallbackGetUserAssignmentsAsync(cancellationToken);
        }

        // ── Fallback Implementations using AppDbContext ─────────────────────────────

        private async Task<Result<List<RoleDto>>> FallbackGetRolesAsync(CancellationToken ct)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetService<AppDbContext>();
                if (db == null) return Result<List<RoleDto>>.Error("Database service unavailable.");

                var roles = await db.Roles
                    .AsNoTracking()
                    .Include(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                    .Include(r => r.UserRoles)
                    .OrderBy(r => r.IsSystemRole ? 0 : 1)
                    .ThenBy(r => r.RoleId)
                    .ToListAsync(ct);

                var list = roles.Select(r => new RoleDto
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

                return Result<List<RoleDto>>.Success(list);
            }
            catch (Exception ex)
            {
                return Result<List<RoleDto>>.SystemError(ex.Message);
            }
        }

        private async Task<Result<List<PermissionDto>>> FallbackGetAllPermissionsAsync(CancellationToken ct)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetService<AppDbContext>();
                if (db == null) return Result<List<PermissionDto>>.Error("Database service unavailable.");

                var perms = await db.Permissions
                    .AsNoTracking()
                    .OrderBy(p => p.Module)
                    .ThenBy(p => p.PermissionCode)
                    .ToListAsync(ct);

                var list = perms.Select(p => new PermissionDto
                {
                    PermissionId = p.PermissionId,
                    PermissionCode = p.PermissionCode,
                    PermissionName = p.PermissionName,
                    Module = p.Module,
                    Description = p.Description
                }).ToList();

                return Result<List<PermissionDto>>.Success(list);
            }
            catch (Exception ex)
            {
                return Result<List<PermissionDto>>.SystemError(ex.Message);
            }
        }

        private async Task<Result<RoleDto>> FallbackSaveRoleAsync(SaveRoleRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req?.RoleName))
                return Result<RoleDto>.ValidationError("Role name is required.");

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetService<AppDbContext>();
                if (db == null) return Result<RoleDto>.Error("Database service unavailable.");

                var cleanName = req.RoleName.Trim();
                Role? role;

                if (req.RoleId.HasValue && req.RoleId.Value > 0)
                {
                    role = await db.Roles
                        .Include(r => r.RolePermissions)
                        .FirstOrDefaultAsync(r => r.RoleId == req.RoleId.Value, ct);

                    if (role == null)
                        return Result<RoleDto>.NotFoundError("Role not found.");

                    if (!role.RoleName.Equals(cleanName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (role.IsSystemRole)
                            return Result<RoleDto>.ValidationError("System role names cannot be renamed.");

                        bool exists = await db.Roles.AnyAsync(r => r.RoleName == cleanName && r.RoleId != role.RoleId, ct);
                        if (exists)
                            return Result<RoleDto>.DuplicateRecordError($"Role name '{cleanName}' already exists.");

                        role.RoleName = cleanName;
                    }

                    role.Description = req.Description?.Trim();
                    role.IsActive = req.IsActive;

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
                    bool exists = await db.Roles.AnyAsync(r => r.RoleName == cleanName, ct);
                    if (exists)
                        return Result<RoleDto>.DuplicateRecordError($"Role '{cleanName}' already exists.");

                    role = new Role
                    {
                        RoleName = cleanName,
                        Description = req.Description?.Trim(),
                        IsSystemRole = false,
                        IsActive = req.IsActive,
                        CreatedAt = DateTime.UtcNow
                    };

                    db.Roles.Add(role);
                    await db.SaveChangesAsync(ct);

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

                await db.SaveChangesAsync(ct);
                return Result<RoleDto>.Success(new RoleDto
                {
                    RoleId = role.RoleId,
                    RoleName = role.RoleName,
                    Description = role.Description,
                    IsSystemRole = role.IsSystemRole,
                    IsActive = role.IsActive,
                    CreatedAt = role.CreatedAt
                }, "Role saved successfully.");
            }
            catch (Exception ex)
            {
                return Result<RoleDto>.SystemError(ex.Message);
            }
        }

        private async Task<Result<bool>> FallbackDeleteRoleAsync(int roleId, CancellationToken ct)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetService<AppDbContext>();
                if (db == null) return Result<bool>.Error("Database service unavailable.");

                var role = await db.Roles
                    .Include(r => r.UserRoles)
                    .FirstOrDefaultAsync(r => r.RoleId == roleId, ct);

                if (role == null)
                    return Result<bool>.NotFoundError("Role not found.");

                if (role.IsSystemRole)
                    return Result<bool>.ValidationError("System roles (SuperAdmin, Admin, User) cannot be deleted.");

                if (role.UserRoles.Any())
                    return Result<bool>.ValidationError($"Cannot delete role '{role.RoleName}' because {role.UserRoles.Count} user(s) are currently assigned to it. Please reassign those users first.");

                db.Roles.Remove(role);
                await db.SaveChangesAsync(ct);

                return Result<bool>.Success(true, $"Role '{role.RoleName}' deleted successfully.");
            }
            catch (Exception ex)
            {
                return Result<bool>.SystemError(ex.Message);
            }
        }

        private async Task<Result<List<int>>> FallbackGetUserRoleIdsAsync(int userId, CancellationToken ct)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetService<AppDbContext>();
                if (db == null) return Result<List<int>>.Error("Database service unavailable.");

                var ids = await db.UserRoles
                    .AsNoTracking()
                    .Where(ur => ur.UserId == userId)
                    .Select(ur => ur.RoleId)
                    .ToListAsync(ct);

                return Result<List<int>>.Success(ids);
            }
            catch (Exception ex)
            {
                return Result<List<int>>.SystemError(ex.Message);
            }
        }

        private async Task<Result<List<string>>> FallbackGetUserRoleNamesAsync(int userId, CancellationToken ct)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetService<AppDbContext>();
                if (db == null) return Result<List<string>>.Error("Database service unavailable.");

                var names = await db.UserRoles
                    .AsNoTracking()
                    .Where(ur => ur.UserId == userId)
                    .Select(ur => ur.Role.RoleName)
                    .ToListAsync(ct);

                return Result<List<string>>.Success(names);
            }
            catch (Exception ex)
            {
                return Result<List<string>>.SystemError(ex.Message);
            }
        }

        private async Task<Result<List<string>>> FallbackGetUserPermissionsAsync(int userId, CancellationToken ct)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetService<AppDbContext>();
                if (db == null) return Result<List<string>>.Error("Database service unavailable.");

                var roleIds = await db.UserRoles
                    .AsNoTracking()
                    .Where(ur => ur.UserId == userId && ur.Role.IsActive)
                    .Select(ur => ur.RoleId)
                    .ToListAsync(ct);

                if (!roleIds.Any())
                    return Result<List<string>>.Success(new List<string>());

                bool isSuperAdmin = await db.Roles
                    .AnyAsync(r => roleIds.Contains(r.RoleId) && r.RoleName == "SuperAdmin", ct);

                if (isSuperAdmin)
                {
                    var allPerms = await db.Permissions.Select(p => p.PermissionCode).ToListAsync(ct);
                    var set = new HashSet<string>(allPerms, StringComparer.OrdinalIgnoreCase) { "*" };
                    return Result<List<string>>.Success(set.ToList());
                }

                var permissions = await db.RolePermissions
                    .AsNoTracking()
                    .Where(rp => roleIds.Contains(rp.RoleId))
                    .Select(rp => rp.Permission.PermissionCode)
                    .Distinct()
                    .ToListAsync(ct);

                return Result<List<string>>.Success(permissions);
            }
            catch (Exception ex)
            {
                return Result<List<string>>.SystemError(ex.Message);
            }
        }

        private async Task<Result<bool>> FallbackAssignUserRolesAsync(AssignUserRolesRequest req, CancellationToken ct)
        {
            if (req == null || req.UserId <= 0)
                return Result<bool>.ValidationError("Invalid user ID.");

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetService<AppDbContext>();
                if (db == null) return Result<bool>.Error("Database service unavailable.");

                var user = await db.Users
                    .Include(u => u.UserRoles)
                    .FirstOrDefaultAsync(u => u.Id == req.UserId, ct);

                if (user == null)
                    return Result<bool>.NotFoundError("User not found.");

                db.UserRoles.RemoveRange(user.UserRoles);

                if (req.RoleIds != null && req.RoleIds.Any())
                {
                    foreach (var rId in req.RoleIds.Distinct())
                    {
                        db.UserRoles.Add(new UserRole
                        {
                            UserId = req.UserId,
                            RoleId = rId,
                            AssignedAt = DateTime.UtcNow
                        });
                    }

                    var primaryRole = await db.Roles.FirstOrDefaultAsync(r => r.RoleId == req.RoleIds.First(), ct);
                    if (primaryRole != null)
                    {
                        user.Role = primaryRole.RoleName;
                    }
                }
                else
                {
                    var defaultRole = await db.Roles.FirstOrDefaultAsync(r => r.RoleName == "User", ct);
                    if (defaultRole != null)
                    {
                        db.UserRoles.Add(new UserRole
                        {
                            UserId = req.UserId,
                            RoleId = defaultRole.RoleId,
                            AssignedAt = DateTime.UtcNow
                        });
                        user.Role = "User";
                    }
                }

                await db.SaveChangesAsync(ct);
                return Result<bool>.Success(true, "User roles updated successfully.");
            }
            catch (Exception ex)
            {
                return Result<bool>.SystemError(ex.Message);
            }
        }

        private async Task<Result<List<UserRoleAssignmentDto>>> FallbackGetUserAssignmentsAsync(CancellationToken ct)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetService<AppDbContext>();
                if (db == null) return Result<List<UserRoleAssignmentDto>>.Error("Database service unavailable.");

                var users = await db.Users
                    .AsNoTracking()
                    .Where(u => !u.IsDeleted)
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .OrderByDescending(u => u.Id)
                    .ToListAsync(ct);

                var list = users.Select(u => new UserRoleAssignmentDto
                {
                    UserId = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    AssignedRoleIds = u.UserRoles.Select(ur => ur.RoleId).ToList(),
                    AssignedRoleNames = u.UserRoles.Where(ur => ur.Role != null).Select(ur => ur.Role.RoleName).ToList()
                }).ToList();

                return Result<List<UserRoleAssignmentDto>>.Success(list);
            }
            catch (Exception ex)
            {
                return Result<List<UserRoleAssignmentDto>>.SystemError(ex.Message);
            }
        }
    }
}
