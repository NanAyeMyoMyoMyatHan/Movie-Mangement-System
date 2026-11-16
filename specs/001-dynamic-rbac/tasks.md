---
description: "Task list for Dynamic RBAC and Database Schema Cleanup"
---

# Tasks: Dynamic Role-Based Access Control (RBAC) & Database Schema Cleanup

**Input**: Design documents from `specs/001-dynamic-rbac/`  
**Prerequisites**: [plan.md](plan.md), [spec.md](spec.md), [research.md](research.md), [data-model.md](data-model.md), [contracts/](contracts/)

---

## Phase 1: Setup (Database Schema & Migration)

**Purpose**: Database DDL execution, table creation, default data seeding, and obsolete table drop.

- [x] T001 Create SQL migration script in `DotNetProject.Database/Scripts/001_create_dynamic_rbac_tables.sql` containing DDL for `Roles`, `Permissions`, `RolePermissions`, `UserRoles`, drop statement for `WatchlistItems`, and seed data for default roles and permissions.
- [x] T002 Execute SQL migration script against SQL Server `MyFlixDb` to apply schema changes, seed initial permissions/roles, and migrate existing users into `UserRoles`.

---

## Phase 2: Foundational (Entities & DbContext)

**Purpose**: Core EF Core models and DbContext mapping required for all user stories.

**⚠️ CRITICAL**: Must be completed before user story implementation begins.

- [x] T003 [P] Create `Role.cs` entity in `DotNetProject.Database/AppDbModels/Role.cs`
- [x] T004 [P] Create `Permission.cs` entity in `DotNetProject.Database/AppDbModels/Permission.cs`
- [x] T005 [P] Create `RolePermission.cs` junction entity in `DotNetProject.Database/AppDbModels/RolePermission.cs`
- [x] T006 [P] Create `UserRole.cs` junction entity in `DotNetProject.Database/AppDbModels/UserRole.cs`
- [x] T007 Update `DotNetProject.Database/AppDbModels/User.cs` to add `UserRoles` navigation collection and remove obsolete `WatchlistItems` reference
- [x] T008 Remove obsolete `DotNetProject.Database/AppDbModels/WatchlistItem.cs` and update `DotNetProject.Database/AppDbModels/AppDbContext.cs` to register `Roles`, `Permissions`, `RolePermissions`, `UserRoles` with fluent configurations and remove `WatchlistItems`

---

## Phase 3: User Story 1 - Dynamic Role & Permission Management (Priority: P1) 🎯 MVP

**Goal**: Enable administrators to create custom dynamic roles and toggle permissions across Content, Episodes, Users, Roles, and Analytics without code redeployment.

**Independent Test**: Navigate to `/admin/roles`, create role `"DramaModerator"`, toggle permissions, click save, and verify records in SQL Server `Roles` and `RolePermissions`.

- [x] T009 [US1] Create RBAC DTOs and `RbacService.cs` in `DotNetProject.Balzor/Components/Shared/RbacService.cs` implementing role retrieval, role creation/update, permission grouping, and role deletion
- [x] T010 [US1] Register `RbacService` into DI container in `DotNetProject.Balzor/Program.cs`
- [x] T011 [US1] Create Claymorphic Admin Roles page `DotNetProject.Balzor/Components/Pages/Admin/AdminRoles.razor` and `AdminRoles.razor.cs` with role list, user count badges, system role protections, and dynamic permission checkbox matrix modal

---

## Phase 4: User Story 2 - User-to-Roles Assignment in Admin Portal (Priority: P1)

**Goal**: Allow administrators to assign one or multiple dynamic roles to users with instant persistence.

**Independent Test**: In `/admin/users`, select a user, assign custom roles (`ContentModerator` + `User`), save, and verify `UserRoles` records in SQL Server.

- [x] T012 [US2] Extend `RbacService.cs` with `GetUserRolesAsync(int userId)` and `AssignUserRolesAsync(int userId, List<int> roleIds)`
- [x] T013 [US2] Update `DotNetProject.Balzor/Components/Pages/Admin/AdminUsers.razor` with Claymorphic "Manage Roles" modal supporting multi-role assignment and dynamic role chips

---

## Phase 5: User Story 3 - Granular Client & Admin Navigation Protection (Priority: P2)

**Goal**: Dynamically evaluate user permissions in real-time, conditionally rendering menu links and action buttons while guarding admin pages.

**Independent Test**: Sign in as a user with only `Content.View` (no `Role.View`), verify `/admin/roles` navigation link is hidden and direct URL access is blocked.

- [x] T014 [US3] Update `DotNetProject.Balzor/Components/Shared/AuthService.cs` to load user roles, resolve and cache effective permissions in `CurrentUserDto`, and provide `HasPermission(string code)`, `HasAnyPermission(...)`, and `HasRole(string role)`
- [x] T015 [US3] Update `DotNetProject.Balzor/Components/Layout/AdminLayout.razor` to show/hide navigation links based on permissions (`Role.View`, `Content.View`, `Episode.View`, `User.View`) and display a friendly Claymorphic "Permission Denied" alert when unauthorized
- [x] T016 [US3] Guard action buttons across `AdminContents.razor` (`Content.Create`, `Content.Edit`, `Content.Delete`) and `AdminEpisodes.razor` (`Episode.Create`, `Episode.Edit`, `Episode.Delete`)

---

## Phase 6: Polish & Verification

**Purpose**: Verification of build, database integrity, and quickstart scenarios.

- [x] T017 Remove any remaining references to `WatchlistItems` in the solution and verify clean build with `dotnet build`
- [x] T018 Execute validation scenarios from `specs/001-dynamic-rbac/quickstart.md` and verify database state and UI behavior
