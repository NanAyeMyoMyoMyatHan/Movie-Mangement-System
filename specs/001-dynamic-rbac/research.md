# Phase 0 Research: Dynamic RBAC & Database Schema Cleanup

**Feature**: `001-dynamic-rbac`  
**Date**: 2026-11-16  
**Status**: Completed  

---

## Technical Decisions & Analysis

### 1. Dynamic RBAC vs Hardcoded String Roles vs ASP.NET Identity

#### Context
Currently, `User.Role` is a single string column (`"Admin"`, `"User"`). This prevents creating custom roles (e.g., `ContentModerator`, `VIPMember`, `EpisodeEditor`) or adjusting permissions without code changes and deployment.

#### Decision
Implement a pure **Relational Dynamic RBAC** schema using 4 tables:
1. `Roles`: (`RoleId` INT PK, `RoleName` NVARCHAR(50) UNIQUE, `Description` NVARCHAR(250), `IsSystemRole` BIT, `IsActive` BIT, `CreatedAt` DATETIME2)
2. `Permissions`: (`PermissionId` INT PK, `PermissionCode` NVARCHAR(100) UNIQUE, `PermissionName` NVARCHAR(100), `Module` NVARCHAR(50), `Description` NVARCHAR(250))
3. `RolePermissions`: (`RoleId` INT, `PermissionId` INT, `AssignedAt` DATETIME2) -> PK `(RoleId, PermissionId)`
4. `UserRoles`: (`UserId` INT, `RoleId` INT, `AssignedAt` DATETIME2) -> PK `(UserId, RoleId)`

#### Rationale
- Matches the existing `int` primary key convention of `Users.Id`.
- Completely decoupled from external libraries; 100% compatible with Blazor InteractiveServer, Entity Framework Core 8, and raw SQL Server queries.
- Many-to-Many relationship allows a user to hold multiple roles simultaneously (e.g. `Staff` + `Moderator`).
- Granular permission codes follow the dot-notation standard (`<Module>.<Action>`, e.g., `Content.Create`, `Episode.Edit`, `User.AssignRole`), making security policies intuitive and scalable.

#### Alternatives Considered
- *ASP.NET Core Identity*: Rejected because the project has a custom `AuthService` and custom `Users` table; refactoring to IdentityUser would introduce breaking changes to user passwords, schema tables, and token generation.
- *Claims-only storage in session JSON*: Rejected because claims stored only in JSON cannot be dynamically revoked or audited across the entire user base from SQL Server.

---

### 2. Database Schema Cleanup: Decommissioning Unused Tables

#### Context
The user explicitly specified: *"if need to create table create and remove unuse table"*.

#### Audit Findings
An audit of `MyFlixDb` tables and codebase references was performed:
1. **`WatchlistItems`**:
   - Table contains 11 legacy rows with foreign keys to discontinued `Movies`.
   - Blazor uses `UserWatchLater` (in `WatchLaterService.cs`) exclusively for all Watchlist and Continue Watching operations.
   - `WatchlistItems` is completely dead in the UI.
   - **Action**: Drop `WatchlistItems` table and remove `DbSet<WatchlistItem>` from `AppDbContext.cs`.
2. **`Ratings`**:
   - `Ratings` table contains 11 rows. The Blazor frontend does not use it (ratings are fetched dynamically from TMDB/hardcoded metadata). However, `DotNetProject.WebApi` has a standalone `RatingController`.
   - **Action**: Preserve `Ratings` for now to avoid breaking WebApi build, or isolate if WebApi is run independently.

---

### 3. Permission Evaluation in Blazor UI & AuthService

#### Context
Blazor components require fast, synchronous or cached authorization checks (e.g. `@if (AuthService.HasPermission("Content.Delete"))`).

#### Decision
- Extend `CurrentUserDto` with:
  ```csharp
  public List<string> Roles { get; set; } = new();
  public HashSet<string> Permissions { get; set; } = new(StringComparer.OrdinalIgnoreCase);
  public bool HasPermission(string permissionCode) => 
      IsAdmin || Permissions.Contains(permissionCode) || Permissions.Contains("*");
  public bool HasRole(string roleName) => 
      Roles.Any(r => r.Equals(roleName, StringComparison.OrdinalIgnoreCase));
  ```
- When a user logs in or refreshes, `AuthService` loads the user's assigned roles from `UserRoles` and resolves all distinct permission codes from `RolePermissions`.
- Pre-cache permissions into a `HashSet<string>` inside `CurrentUserDto` for $O(1)$ fast lookups in Razor components.
- SuperAdmin role automatically receives wildcard permission `*` so system administrators are never locked out of new features.

---

### 4. Permission Taxonomy & Default Seeding

The following permissions are pre-seeded across 5 primary modules:

| Module | Permission Code | Description |
| :--- | :--- | :--- |
| **Content** | `Content.View` | View contents and drama catalog in admin |
| **Content** | `Content.Create` | Add new dramas and movies |
| **Content** | `Content.Edit` | Edit existing drama metadata and posters |
| **Content** | `Content.Delete` | Delete content records |
| **Content** | `Content.Import` | Import content from TMDB API |
| **Episode** | `Episode.View` | View episode list and streaming links |
| **Episode** | `Episode.Create` | Add new episodes to dramas |
| **Episode** | `Episode.Edit` | Edit episode titles and update video URLs |
| **Episode** | `Episode.Delete` | Delete episodes |
| **User** | `User.View` | View registered members list |
| **User** | `User.Create` | Create user accounts |
| **User** | `User.Edit` | Edit user profile details and passwords |
| **User** | `User.Delete` | Delete or ban user accounts |
| **User** | `User.AssignRole` | Assign or revoke roles for users |
| **Role** | `Role.View` | View roles and permission matrices |
| **Role** | `Role.Manage` | Create, edit, and assign permissions to roles |
| **Analytics** | `Dashboard.View` | Access admin overview dashboard and statistics |

#### Default Roles
1. **`SuperAdmin`**: All permissions (`*`). System role (`IsSystemRole = 1`).
2. **`Admin`**: Management permissions across Content, Episodes, Users, and Analytics.
3. **`ContentModerator`**: Permissions for `Content.*` and `Episode.*`, but denied `User.*` and `Role.*`.
4. **`User`**: Base viewer role.
