# Feature Specification: Dynamic Role-Based Access Control (RBAC) & Database Schema Cleanup

**Feature Branch**: `001-dynamic-rbac`  
**Created**: 2026-11-16  
**Status**: Specified & Ready for Implementation  
**Input**: "change my pj into dynamic rbac, if need to create table create and remove unuse table"

---

## Executive Summary

This feature transitions the NangWorld application from a hardcoded two-tier role string model (`"Admin"` / `"User"` column on the `Users` table) into an enterprise-grade **Dynamic Role-Based Access Control (RBAC)** architecture. 

It introduces dedicated relational tables for **Roles**, **Permissions**, **RolePermissions**, and **UserRoles**, along with an administrative UI to dynamically create custom roles, toggle permissions per module in real-time, and assign multiple roles to users. In tandem, legacy and unused database tables (`WatchlistItems`, previously coupled to discontinued movie prototypes and superseded by `UserWatchLater`) are safely decommissioned and cleaned up from the SQL Server database schema and EF Core DbContext.

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Dynamic Role & Permission Assignment by SuperAdmin (Priority: P1)

As a System Administrator / SuperAdmin,  
I want to view, create, and customize Roles and assign granular permissions across all application modules (Content, Episodes, Users, Roles, Analytics),  
So that our organization can delegate specific responsibilities (e.g., Content Moderator, Episode Uploader, VIP Member Manager) without modifying backend code or redeploying the application.

**Why this priority**: Core value of dynamic RBAC. Without dynamic role definitions and permission mapping, administrative authority remains binary and inflexible.

**Independent Test**:
Can be fully tested by navigating to `/admin/roles`, creating a new role (e.g., `"ContentModerator"`), selecting permissions `[Content.View, Content.Create, Content.Edit, Episode.Manage]`, assigning it to a user, and verifying that the user can manage content and episodes but is denied access to `/admin/users` or `/admin/roles`.

**Acceptance Scenarios**:
1. **Given** an authenticated user with `Role.Manage` permission, **When** they navigate to `/admin/roles`, **Then** they see the list of all active roles with assigned user counts and permission summaries.
2. **Given** an authenticated admin creating a role `"DramaEditor"`, **When** they toggle permissions `Content.*` and `Episode.*` and click Save, **Then** a new record is added to `Roles` and the selected permissions are persisted in `RolePermissions`.
3. **Given** an existing custom role, **When** permissions are added or removed, **Then** changes take effect immediately for all users assigned to that role without restarting the application.

---

### User Story 2 - User-to-Roles Assignment in Admin Portal (Priority: P1)

As an Administrator managing staff and members,  
I want to assign one or multiple roles to a user through the Admin Users UI,  
So that users receive the exact aggregated permissions granted by their roles.

**Why this priority**: Connects users to the dynamic RBAC system; enables multi-role inheritance (e.g., a user can be both `ContentModerator` and `VIPMember`).

**Independent Test**:
Assign a user two distinct roles (`ContentModerator` + `SupportStaff`) and verify their effective permissions include the union of permissions from both roles.

**Acceptance Scenarios**:
1. **Given** an administrator on `/admin/users`, **When** clicking "Edit Roles" on a user, **Then** a multi-select modal displays all available dynamic roles with their description.
2. **Given** a user assigned multiple roles, **When** the user logs in, **Then** their session holds all permissions from all assigned roles.
3. **Given** a user whose roles are modified by admin, **When** the update is saved, **Then** `UserRoles` is updated in SQL Server.

---

### User Story 3 - Granular Client & Admin Navigation Protection (Priority: P2)

As a user navigating the application,  
I want the UI to conditionally render menu items, action buttons (such as Edit, Delete, Import, Upload), and guard admin routes based on my active dynamic permissions,  
So that unauthorized actions are neither visible nor executable.

**Why this priority**: Enhances security and user experience by preventing unauthorized actions and visual clutter for non-privileged users.

**Independent Test**:
Log in as a user with only `Content.View` (no `Content.Delete`), open `/admin/contents`, and verify that the "Delete" button is hidden/disabled, and direct POST/DELETE invocations return 403 Forbidden.

**Acceptance Scenarios**:
1. **Given** a user lacking `User.View` permission, **When** they try to access `/admin/users`, **Then** access is denied with a clear "Permission Denied" prompt.
2. **Given** a user possessing `Episode.Edit` permission, **When** viewing the drama theater player, **Then** the "✏ Edit Video URL" action is displayed and operational.

---

### User Story 4 - Database Schema Cleanup of Obsolete Tables (Priority: P3)

As a database maintainer,  
I want unused and obsolete database tables (specifically `WatchlistItems`, which was superseded by `UserWatchLater`) safely dropped and removed from EF Core `AppDbContext`,  
So that the database schema is clean, maintainable, and contains zero dead tables.

**Why this priority**: Fulfills the explicit user request: *"if need to create table create and remove unuse table"*. Eliminates schema confusion.

**Independent Test**:
Execute table audit query on `MyFlixDb`; verify `WatchlistItems` is removed from SQL Server and `AppDbContext.cs` compiles and runs without missing table errors.

**Acceptance Scenarios**:
1. **Given** `UserWatchLater` handles all active watchlist operations in `WatchLaterService`, **When** schema cleanup script runs, **Then** `WatchlistItems` table is dropped without data loss to active watchlists.
2. **Given** `AppDbContext`, **When** rebuilt, **Then** all `DbSet<WatchlistItem>` and foreign key configurations are cleanly excised.

---

## Edge Cases

- **System Role Protection**: System roles like `SuperAdmin` and `User` must be protected (`IsSystemRole = true`) from deletion to prevent locking out all administrators.
- **Last Admin Safeguard**: The system must prevent removing the `SuperAdmin` role from the last remaining administrator account.
- **Concurrent Permission Changes**: When an admin revokes a permission, active user sessions must re-evaluate effective permissions without requiring re-login if refreshed.
- **Legacy User Migration**: Existing users with string `Role = 'Admin'` must automatically map to `Admin` role in `UserRoles`; users with `Role = 'User'` or `NULL` must map to `User` role.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST create `Roles` table with columns: `RoleId` (INT PK IDENTITY), `RoleName` (NVARCHAR(50) UNIQUE), `Description` (NVARCHAR(250)), `IsSystemRole` (BIT), `IsActive` (BIT), `CreatedAt` (DATETIME2).
- **FR-002**: System MUST create `Permissions` table with columns: `PermissionId` (INT PK IDENTITY), `PermissionCode` (NVARCHAR(100) UNIQUE), `PermissionName` (NVARCHAR(100)), `Module` (NVARCHAR(50)), `Description` (NVARCHAR(250)).
- **FR-003**: System MUST create `RolePermissions` junction table with composite key `(RoleId, PermissionId)` referencing `Roles` and `Permissions` with cascading deletes.
- **FR-004**: System MUST create `UserRoles` junction table with composite key `(UserId, RoleId)` referencing `Users` and `Roles` with cascading deletes.
- **FR-005**: System MUST seed default permissions across 5 modules: `Content` (View, Create, Edit, Delete, Import), `Episode` (View, Create, Edit, Delete), `User` (View, Create, Edit, Delete, AssignRole), `Role` (View, Create, Edit, Delete, AssignPermission), `Analytics` (Dashboard.View).
- **FR-006**: System MUST seed default system roles: `SuperAdmin` (all permissions), `Admin` (management), `ContentModerator` (content/episodes), `User` (viewer).
- **FR-007**: System MUST migrate all existing accounts in `Users` table into `UserRoles` according to their current `Role` column.
- **FR-008**: System MUST provide an Admin UI at `/admin/roles` to manage roles, inspect permissions per module, and toggle permissions dynamically.
- **FR-009**: System MUST update `AuthService` to load effective permissions for the authenticated user and provide `HasPermission(string code)` and `HasRole(string roleName)`.
- **FR-010**: System MUST drop obsolete table `WatchlistItems` and remove its EF Core entity from `AppDbContext`.
