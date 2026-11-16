using System;
using System.Collections.Generic;

namespace DotNetProject.Domain.Features.Rbac
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

    public class AssignUserRolesRequest
    {
        public int UserId { get; set; }
        public List<int> RoleIds { get; set; } = new();
    }

    public class UserRoleAssignmentDto
    {
        public int UserId { get; set; }
        public int Id { get => UserId; set => UserId = value; }
        public string UserName { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Role { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<int> AssignedRoleIds { get; set; } = new();
        public List<string> AssignedRoleNames { get; set; } = new();
    }

    public class UserPermissionsResponse
    {
        public int UserId { get; set; }
        public List<string> RoleNames { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
    }
}
