using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using DotNetProject.Database.AppDbModels;
using DotNetProject.Domain.Features.Rbac;
using Microsoft.EntityFrameworkCore;

namespace DotNetProject.WebApi.Features.Rbac
{
    public class RolePermissionService : IRolePermissionService
    {
        private readonly AppDbContext _db;

        public RolePermissionService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Result<List<RoleDto>>> GetRolesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var roles = await _db.Roles
                    .AsNoTracking()
                    .Include(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                    .Include(r => r.UserRoles)
                    .OrderBy(r => r.IsSystemRole ? 0 : 1)
                    .ThenBy(r => r.RoleId)
                    .ToListAsync(cancellationToken);

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
                return Result<List<RoleDto>>.SystemError($"Failed to fetch roles: {ex.Message}");
            }
        }

        public async Task<Result<List<PermissionDto>>> GetAllPermissionsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var perms = await _db.Permissions
                    .AsNoTracking()
                    .OrderBy(p => p.Module)
                    .ThenBy(p => p.PermissionCode)
                    .ToListAsync(cancellationToken);

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
                return Result<List<PermissionDto>>.SystemError($"Failed to fetch permissions: {ex.Message}");
            }
        }

        public async Task<Result<RoleDto>> SaveRoleAsync(SaveRoleRequest req, CancellationToken cancellationToken = default)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.RoleName))
                return Result<RoleDto>.ValidationError("Role name is required.");

            try
            {
                var cleanName = req.RoleName.Trim();
                Role? role;

                if (req.RoleId.HasValue && req.RoleId.Value > 0)
                {
                    role = await _db.Roles
                        .Include(r => r.RolePermissions)
                        .FirstOrDefaultAsync(r => r.RoleId == req.RoleId.Value, cancellationToken);

                    if (role == null)
                        return Result<RoleDto>.NotFoundError("Role not found.");

                    // If role name is changing, check uniqueness
                    if (!role.RoleName.Equals(cleanName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (role.IsSystemRole)
                            return Result<RoleDto>.ValidationError("System role names cannot be renamed.");

                        bool exists = await _db.Roles.AnyAsync(r => r.RoleName == cleanName && r.RoleId != role.RoleId, cancellationToken);
                        if (exists)
                            return Result<RoleDto>.DuplicateRecordError($"Role name '{cleanName}' already exists.");

                        role.RoleName = cleanName;
                    }

                    role.Description = req.Description?.Trim();
                    role.IsActive = req.IsActive;

                    // Update Permissions (clear existing, insert new)
                    _db.RolePermissions.RemoveRange(role.RolePermissions);
                    if (req.PermissionIds != null && req.PermissionIds.Any())
                    {
                        foreach (var permId in req.PermissionIds.Distinct())
                        {
                            _db.RolePermissions.Add(new RolePermission
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
                    bool exists = await _db.Roles.AnyAsync(r => r.RoleName == cleanName, cancellationToken);
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

                    _db.Roles.Add(role);
                    await _db.SaveChangesAsync(cancellationToken);

                    if (req.PermissionIds != null && req.PermissionIds.Any())
                    {
                        foreach (var permId in req.PermissionIds.Distinct())
                        {
                            _db.RolePermissions.Add(new RolePermission
                            {
                                RoleId = role.RoleId,
                                PermissionId = permId,
                                AssignedAt = DateTime.UtcNow
                            });
                        }
                    }
                }

                await _db.SaveChangesAsync(cancellationToken);

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
                return Result<RoleDto>.SystemError($"Error saving role: {ex.Message}");
            }
        }

        public async Task<Result<bool>> DeleteRoleAsync(int roleId, CancellationToken cancellationToken = default)
        {
            try
            {
                var role = await _db.Roles
                    .Include(r => r.UserRoles)
                    .FirstOrDefaultAsync(r => r.RoleId == roleId, cancellationToken);

                if (role == null)
                    return Result<bool>.NotFoundError("Role not found.");

                if (role.IsSystemRole)
                    return Result<bool>.ValidationError("System roles (SuperAdmin, Admin, User) cannot be deleted.");

                if (role.UserRoles.Any())
                    return Result<bool>.ValidationError($"Cannot delete role '{role.RoleName}' because {role.UserRoles.Count} user(s) are currently assigned to it. Please reassign those users first.");

                _db.Roles.Remove(role);
                await _db.SaveChangesAsync(cancellationToken);

                return Result<bool>.Success(true, $"Role '{role.RoleName}' deleted successfully.");
            }
            catch (Exception ex)
            {
                return Result<bool>.SystemError($"Error deleting role: {ex.Message}");
            }
        }

        public async Task<Result<List<int>>> GetUserRoleIdsAsync(int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var ids = await _db.UserRoles
                    .AsNoTracking()
                    .Where(ur => ur.UserId == userId)
                    .Select(ur => ur.RoleId)
                    .ToListAsync(cancellationToken);

                return Result<List<int>>.Success(ids);
            }
            catch (Exception ex)
            {
                return Result<List<int>>.SystemError($"Error fetching user role IDs: {ex.Message}");
            }
        }

        public async Task<Result<List<string>>> GetUserRoleNamesAsync(int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var names = await _db.UserRoles
                    .AsNoTracking()
                    .Where(ur => ur.UserId == userId)
                    .Select(ur => ur.Role.RoleName)
                    .ToListAsync(cancellationToken);

                return Result<List<string>>.Success(names);
            }
            catch (Exception ex)
            {
                return Result<List<string>>.SystemError($"Error fetching user role names: {ex.Message}");
            }
        }

        public async Task<Result<List<string>>> GetUserPermissionsAsync(int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var roleIds = await _db.UserRoles
                    .AsNoTracking()
                    .Where(ur => ur.UserId == userId && ur.Role.IsActive)
                    .Select(ur => ur.RoleId)
                    .ToListAsync(cancellationToken);

                if (!roleIds.Any())
                    return Result<List<string>>.Success(new List<string>());

                // If user has SuperAdmin role, grant all permissions plus wildcard
                bool isSuperAdmin = await _db.Roles
                    .AnyAsync(r => roleIds.Contains(r.RoleId) && r.RoleName == "SuperAdmin", cancellationToken);

                if (isSuperAdmin)
                {
                    var allPerms = await _db.Permissions.Select(p => p.PermissionCode).ToListAsync(cancellationToken);
                    var set = new HashSet<string>(allPerms, StringComparer.OrdinalIgnoreCase) { "*" };
                    return Result<List<string>>.Success(set.ToList());
                }

                var permissions = await _db.RolePermissions
                    .AsNoTracking()
                    .Where(rp => roleIds.Contains(rp.RoleId))
                    .Select(rp => rp.Permission.PermissionCode)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                return Result<List<string>>.Success(permissions);
            }
            catch (Exception ex)
            {
                return Result<List<string>>.SystemError($"Error fetching user permissions: {ex.Message}");
            }
        }

        public async Task<Result<bool>> AssignUserRolesAsync(AssignUserRolesRequest req, CancellationToken cancellationToken = default)
        {
            if (req == null || req.UserId <= 0)
                return Result<bool>.ValidationError("Invalid user ID.");

            try
            {
                var user = await _db.Users
                    .Include(u => u.UserRoles)
                    .FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken);

                if (user == null)
                    return Result<bool>.NotFoundError("User not found.");

                // Clear old roles
                _db.UserRoles.RemoveRange(user.UserRoles);

                if (req.RoleIds != null && req.RoleIds.Any())
                {
                    foreach (var rId in req.RoleIds.Distinct())
                    {
                        _db.UserRoles.Add(new UserRole
                        {
                            UserId = req.UserId,
                            RoleId = rId,
                            AssignedAt = DateTime.UtcNow
                        });
                    }

                    // Update fallback User.Role string column to primary role for legacy compatibility
                    var primaryRole = await _db.Roles.FirstOrDefaultAsync(r => r.RoleId == req.RoleIds.First(), cancellationToken);
                    if (primaryRole != null)
                    {
                        user.Role = primaryRole.RoleName;
                    }
                }
                else
                {
                    // Fallback to standard User role if no role selected
                    var defaultRole = await _db.Roles.FirstOrDefaultAsync(r => r.RoleName == "User", cancellationToken);
                    if (defaultRole != null)
                    {
                        _db.UserRoles.Add(new UserRole
                        {
                            UserId = req.UserId,
                            RoleId = defaultRole.RoleId,
                            AssignedAt = DateTime.UtcNow
                        });
                        user.Role = "User";
                    }
                }

                await _db.SaveChangesAsync(cancellationToken);
                return Result<bool>.Success(true, "User roles updated successfully.");
            }
            catch (Exception ex)
            {
                return Result<bool>.SystemError($"Error assigning user roles: {ex.Message}");
            }
        }

        public async Task<Result<List<UserRoleAssignmentDto>>> GetUserAssignmentsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var users = await _db.Users
                    .AsNoTracking()
                    .Where(u => !u.IsDeleted)
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .OrderByDescending(u => u.Id)
                    .ToListAsync(cancellationToken);

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
                return Result<List<UserRoleAssignmentDto>>.SystemError($"Error fetching user assignments: {ex.Message}");
            }
        }
    }
}
