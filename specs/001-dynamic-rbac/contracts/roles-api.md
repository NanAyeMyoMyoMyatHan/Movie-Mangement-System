# Interface Contract: Roles & Permissions Management API

**Feature**: `001-dynamic-rbac`  
**Date**: 2026-11-16  

---

## 1. DTO Contracts

### `RoleDto`
```csharp
public class RoleDto
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
    public bool IsActive { get; set; }
    public int UserCount { get; set; }
    public List<string> PermissionCodes { get; set; } = new();
}
```

### `PermissionDto`
```csharp
public class PermissionDto
{
    public int PermissionId { get; set; }
    public string PermissionCode { get; set; } = string.Empty;
    public string PermissionName { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsGranted { get; set; }
}
```

### `SaveRoleRequest`
```csharp
public class SaveRoleRequest
{
    public int? RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public List<int> PermissionIds { get; set; } = new();
}
```

### `AssignUserRolesRequest`
```csharp
public class AssignUserRolesRequest
{
    public int UserId { get; set; }
    public List<int> RoleIds { get; set; } = new();
}
```

---

## 2. Service Interface: `IRbacService`

```csharp
public interface IRbacService
{
    Task<List<RoleDto>> GetAllRolesAsync();
    Task<RoleDto?> GetRoleByIdAsync(int roleId);
    Task<List<PermissionDto>> GetAllPermissionsAsync();
    Task<Result<RoleDto>> CreateOrUpdateRoleAsync(SaveRoleRequest request);
    Task<Result<bool>> DeleteRoleAsync(int roleId);
    Task<List<string>> GetUserRolesAsync(int userId);
    Task<HashSet<string>> GetUserPermissionsAsync(int userId);
    Task<Result<bool>> AssignUserRolesAsync(int userId, List<int> roleIds);
}
```
