# Interface Contract: Client Authorization & Permission Guard Contract

**Feature**: `001-dynamic-rbac`  
**Date**: 2026-11-16  

---

## 1. `AuthService` RBAC Extensions

```csharp
public partial class AuthService
{
    /// <summary>
    /// Checks whether the currently authenticated user holds a specific permission code.
    /// Returns true if user is SuperAdmin (*) or holds the permission directly through any assigned role.
    /// </summary>
    public bool HasPermission(string permissionCode);

    /// <summary>
    /// Checks whether the user holds any of the specified permissions.
    /// </summary>
    public bool HasAnyPermission(params string[] permissionCodes);

    /// <summary>
    /// Checks whether the user is assigned a specific role name.
    /// </summary>
    public bool HasRole(string roleName);

    /// <summary>
    /// Re-evaluates and refreshes the user's roles and permissions from SQL Server.
    /// </summary>
    public Task RefreshPermissionsAsync();
}
```

---

## 2. Dynamic Guarding in Razor Components

### Example: Conditional Button Rendering
```razor
@if (AuthService.HasPermission("Content.Delete"))
{
    <button class="btn-admin-danger" @onclick="DeleteContent">Delete</button>
}
```

### Example: Navigation Menu Guarding
```razor
@if (AuthService.HasPermission("Role.View"))
{
    <NavLink href="/admin/roles" class="myflix-nav-item">
        <span>Roles &amp; Permissions</span>
    </NavLink>
}
```

### Example: Page-Level Guard in `AdminLayout.razor`
```razor
@if (!AuthService.IsAuthenticated() || !AuthService.HasAnyPermission("Dashboard.View", "Content.View", "User.View", "Role.View"))
{
    <PermissionDeniedNotice />
}
else
{
    @Body
}
```
