# Implementation Plan: Dynamic Role-Based Access Control (RBAC) & Database Schema Cleanup

**Branch**: `001-dynamic-rbac` | **Date**: 2026-11-16 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/001-dynamic-rbac/spec.md`

## Summary

Transform the NangWorld platform from static string-based authorization into an enterprise-grade Dynamic Role-Based Access Control (RBAC) system. Introduce relational tables (`Roles`, `Permissions`, `RolePermissions`, `UserRoles`), an administrative management portal (`/admin/roles`), dynamic permission evaluation in `AuthService`, and cleanly drop the obsolete, dead `WatchlistItems` table from SQL Server and EF Core `AppDbContext`.

## Technical Context

**Language/Version**: C# 12 / .NET 8  
**Primary Dependencies**: Microsoft.EntityFrameworkCore 8.0.8, Microsoft.EntityFrameworkCore.SqlServer, Blazor InteractiveServer  
**Storage**: Microsoft SQL Server (`MyFlixDb` on `localhost`)  
**Testing**: Manual End-to-End Scenarios via Quickstart Guide + SQL Query Verification  
**Target Platform**: Windows / Cross-platform ASP.NET Core Blazor Server  
**Project Type**: Full-stack Blazor Web Application with EF Core Data Layer  
**Performance Goals**: Sub-millisecond ($O(1)$) dynamic permission checks via in-memory hashed permission sets; zero overhead on page renders  
**Constraints**: Zero disruption to existing registered user sessions; seamless automatic migration of existing `Role` column data into `UserRoles`  
**Scale/Scope**: 4 new tables, 1 dropped table, 1 new Admin UI page (`/admin/roles`), extended `AuthService`, and updated Admin navigation  

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle / Gate | Status | Notes |
| :--- | :--- | :--- |
| **Simplicity (YAGNI)** | PASS | Implements standard relational RBAC without introducing heavyweight external identity frameworks. |
| **Zero Regression on Existing Features** | PASS | `AuthService.IsAdmin()` maintained as an alias for backwards compatibility. |
| **Clean Schema Governance** | PASS | Obsolete `WatchlistItems` cleanly dropped as requested by user. |
| **Separation of Concerns** | PASS | Database entities in `DotNetProject.Database`, business logic in `RbacService`, UI in Blazor Admin pages. |

## Project Structure

### Documentation (this feature)

```text
specs/001-dynamic-rbac/
├── spec.md                  # Feature Specification (User scenarios, requirements)
├── plan.md                  # This Implementation Plan (/speckit-plan output)
├── research.md              # Phase 0 Architecture Decisions (/speckit-plan output)
├── data-model.md            # Phase 1 Database Schema & Entity DDL (/speckit-plan output)
├── quickstart.md            # Phase 1 Validation & Testing Guide (/speckit-plan output)
├── contracts/               # Phase 1 Interface Contracts
│   ├── roles-api.md         # IRbacService and DTO contracts
│   └── auth-permission-contract.md # AuthService extension contracts
└── tasks.md                 # Phase 2 Tasks Breakdown (/speckit-tasks output)
```

### Source Code (affected areas)

```text
DotNetProject.Database/
├── AppDbModels/
│   ├── AppDbContext.cs      # Add DbSets for Roles, Permissions, junctions; remove WatchlistItems
│   ├── Role.cs              # [NEW] Role entity
│   ├── Permission.cs        # [NEW] Permission entity
│   ├── RolePermission.cs    # [NEW] RolePermission junction entity
│   ├── UserRole.cs          # [NEW] UserRole junction entity
│   └── WatchlistItem.cs     # [REMOVE] Obsolete entity deleted

DotNetProject.Balzor/
├── Components/
│   ├── Pages/Admin/
│   │   ├── AdminRoles.razor # [NEW] Dynamic Roles & Permissions Management Page
│   │   ├── AdminRoles.razor.cs # [NEW] Code-behind for role creation & permission toggles
│   │   └── AdminUsers.razor # [UPDATE] Multi-role assignment modal
│   ├── Layout/
│   │   └── AdminLayout.razor # [UPDATE] Dynamic permission guarding for menu items
│   └── Shared/
│       ├── AuthService.cs   # [UPDATE] Add HasPermission, HasRole, and permission caching
│       └── RbacService.cs   # [NEW] Role & permission data operations
└── Program.cs               # Register IRbacService DI
```

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
| :--- | :--- | :--- |
| Many-to-Many RolePermissions | Dynamic permission assignment per role | Fixed permissions per role cannot be changed dynamically in admin UI |
| Many-to-Many UserRoles | Single user holding multiple roles | Single role per user limits real-world team delegation (e.g. Moderator + VIP) |
