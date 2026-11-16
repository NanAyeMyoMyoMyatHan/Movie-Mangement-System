# Quickstart & Validation Guide: Dynamic RBAC & Database Cleanup

**Feature**: `001-dynamic-rbac`  
**Date**: 2026-11-16  

---

## 1. Prerequisites

- Microsoft SQL Server running on `localhost` (`MyFlixDb`).
- .NET 8 SDK installed.
- Solution build passes cleanly.

---

## 2. Validation Scenarios

### Scenario 1: Database Migration & Schema Audit
**Objective**: Verify new tables exist and obsolete `WatchlistItems` is dropped.

1. Run the database migration script.
2. Query SQL Server catalog:
   ```sql
   SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES 
   WHERE TABLE_NAME IN ('Roles', 'Permissions', 'RolePermissions', 'UserRoles', 'WatchlistItems');
   ```
3. **Expected Result**:
   - `Roles`, `Permissions`, `RolePermissions`, `UserRoles` return in query.
   - `WatchlistItems` returns nothing (successfully dropped).

---

### Scenario 2: Default System Roles & Seeded Permissions
**Objective**: Verify initial seed permissions and roles.

1. Query seeded counts:
   ```sql
   SELECT COUNT(*) AS RoleCount FROM Roles;
   SELECT COUNT(*) AS PermCount FROM Permissions;
   ```
2. **Expected Result**:
   - At least 4 default roles (`SuperAdmin`, `Admin`, `ContentModerator`, `User`).
   - At least 17 atomic permissions spanning Content, Episode, User, Role, and Analytics.
   - Existing `sa`/admin user linked to `SuperAdmin` in `UserRoles`.

---

### Scenario 3: Custom Role Creation in Admin Portal
**Objective**: Create a new dynamic role with specific permissions.

1. Sign in as Admin and navigate to `/admin/roles`.
2. Click **Add New Role**.
3. Name: `EpisodeEditor`, Description: `Only uploads and edits episodes`.
4. Check permissions: `Episode.View`, `Episode.Edit`, `Episode.Create`.
5. Click **Save Role**.
6. **Expected Result**:
   - `EpisodeEditor` appears in the Roles list.
   - `RolePermissions` contains exactly the selected permission IDs.

---

### Scenario 4: User Role Assignment & Dynamic Access Enforcement
**Objective**: Assign `EpisodeEditor` to a user and test permission enforcement.

1. Navigate to `/admin/users`.
2. Select a test user (e.g., `tester`), click **Edit Roles**.
3. Assign `EpisodeEditor` and uncheck `Admin`.
4. Sign in as `tester`.
5. Attempt navigating to `/admin/users` or `/admin/roles`.
6. **Expected Result**:
   - Access to `/admin/users` is blocked (Permission Denied).
   - Access to `/drama/{id}` allows editing episode video URLs (`Episode.Edit`).
   - Admin menu dynamically hides unauthorized navigation links.
