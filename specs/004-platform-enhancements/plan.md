# Implementation Plan: Platform Enhancements (Resume, Subtitles, Multi-Facet Filters, User Profile, Audit Trail)

**Branch**: `004-platform-enhancements` | **Date**: 2026-11-21 | **Spec**: [specs/004-platform-enhancements/spec.md](file:///C:/Users/Windows11/Desktop/Projects/Movie/specs/004-platform-enhancements/spec.md)

**Input**: Feature specification from `/specs/004-platform-enhancements/spec.md`

---

## Summary

This plan implements five essential platform enhancements to elevate the NangWorld movie/drama streaming experience:
1. **Video Resume & Timestamp Remembering**: Blazor JSInterop integration (`mediaPlayerInterop.js`) to record playback progress (throttled every 5 seconds) and auto-seek on load/play.
2. **Multi-Subtitle & Audio Track (VTT Support)**: Support for Myanmar WebVTT (`.vtt`) subtitles rendered via HTML5 `<track>` with proper UTF-8 font rendering.
3. **Advanced Filtering & Pagination**: Multi-facet catalog filtering (Release Year, Status Ongoing/Completed, Minimum Rating, Search) with clean pagination in `CategoryView.razor`.
4. **User Profile & Password Change**: Self-service user management page (`/profile`) allowing users to change their avatar, bio, and securely update passwords.
5. **Activity Log / Audit Trail**: Administrative audit logging system capturing content deletions, edits, and RBAC role changes, surfaced in `/admin/audit-logs`.

All features are designed to integrate seamlessly with the existing Dynamic RBAC security architecture and Dreamy Light Pastel theme tokens.

---

## Technical Context

**Language/Version**: C# 12 / .NET 8.0, HTML5 / CSS3, JavaScript (ES6)

**Primary Dependencies**: 
- `Microsoft.AspNetCore.Components.Web` (Blazor InteractiveServer)
- `Microsoft.EntityFrameworkCore.SqlServer` 8.0
- `Microsoft.JSInterop`

**Storage**: 
- SQL Server via Entity Framework Core `AppDbContext` (`AuditLog`, `UserContinueWatching`, `User`, `Episode`)
- Local JSON storage fallback in `DotNetProject.Balzor/Storage/`

**Testing**: 
- Manual end-to-end verification via [quickstart.md](file:///C:/Users/Windows11/Desktop/Projects/Movie/specs/004-platform-enhancements/quickstart.md)
- PowerShell 5.1-compatible verification scripts

**Target Platform**: 
- Windows 10/11, Web Browsers (Chrome, Edge, Firefox, Safari)

**Project Type**: 
- Full-Stack ASP.NET Core Solution (Blazor Server frontend + WebApi REST backend + EF Core DB models)

**Performance Goals**: 
- Subtitle & Video seeking < 200ms latency
- Debounced/throttled timestamp saving (5s intervals) to prevent network and DB thrashing
- Filter updates < 16ms (instant UI response)

**Constraints**: 
- Adhere strictly to the Dreamy Light Pastel color palette and design tokens
- All admin audit endpoints protected by Dynamic RBAC permissions
- Target Windows PowerShell 5.1 for all scripts and command tooling

**Scale/Scope**: 
- 5 integrated features touching Player, Category Catalog, User Profile, and Admin Portal

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle / Rule | Compliance Status | Analysis & Verification |
|---|---|---|
| **Clean Flow (UI -> HttpClient -> API -> Service -> DB)** | **PASS** | User profile, password change, and audit queries follow the strict layer separation established in prior features. |
| **Dynamic RBAC Protection** | **PASS** | Audit log dashboard and admin APIs require specific permissions (`CanViewAuditLogs`, `CanManageAdmin`). |
| **Dreamy Pastel Design Tokens** | **PASS** | New profile page, audit log table, and filter controls use defined CSS variables (`--color-lavender`, `--color-mint`, `--color-peach`, etc.). |
| **Windows PowerShell 5.1 Parity** | **PASS** | All scripts use Windows PowerShell 5.1 syntax and parameters. |
| **Resilient Playback & Subtitles** | **PASS** | Native HTML5 `<track>` with UTF-8 WebVTT ensures high-performance rendering without external bloated dependencies. |

---

## Project Structure

### Documentation (this feature)

```text
specs/004-platform-enhancements/
├── spec.md              # Feature specification
├── plan.md              # Implementation plan (this file)
├── research.md          # Phase 0 architectural decisions
├── data-model.md        # Phase 1 data models and schema changes
├── quickstart.md        # Phase 1 end-to-end testing scenarios
└── contracts/           # Phase 1 protocol contracts
    ├── player-resume-contract.md
    ├── subtitles-contract.md
    ├── user-profile-contract.md
    └── audit-log-contract.md
```

### Source Code (repository root)

```text
DotNetProject.Balzor/
├── wwwroot/
│   ├── js/
│   │   └── mediaPlayerInterop.js         # Video resume and timestamp tracking
│   └── subtitles/
│       └── alchemy_ep1_my.vtt            # Sample Myanmar WebVTT subtitle
├── Components/
│   ├── Layout/
│   │   └── NavMenu.razor                 # Links to /profile and /admin/audit-logs
│   ├── Pages/
│   │   ├── Dramas/
│   │   │   ├── DramaDetail.razor         # Video player with resume & subtitles
│   │   │   ├── DramaDetail.razor.cs
│   │   │   ├── CategoryView.razor        # Multi-facet filtering & pagination
│   │   │   ├── CategoryView.razor.cs
│   │   │   └── CategoryView.razor.css
│   │   ├── Auth/
│   │   │   ├── Profile.razor             # User profile & password change
│   │   │   └── Profile.razor.cs
│   │   └── Admin/
│   │       ├── AdminAuditLogs.razor      # Admin Audit Trail table
│   │       └── AdminAuditLogs.razor.cs
│   └── Shared/
│       ├── WatchHistoryService.cs        # Progress tracking integration
│       └── AuditLogService.cs            # Admin activity auditing
DotNetProject.Database/
├── AppDbModels/
│   ├── AuditLog.cs                       # New AuditLog entity
│   ├── User.cs                           # Updated with AvatarUrl & Bio
│   ├── Episode.cs                        # Updated with SubtitleUrl
│   └── AppDbContext.cs                   # DbSets and Model configurations
└── Scripts/
    └── 002_create_audit_log_table.sql    # Migration script for AuditLog
DotNetProject.Domain/
└── Features/
    ├── Users/                            # Profile DTOs and interfaces
    ├── Audit/                            # Audit DTOs and interfaces
    ├── Tmdb/                             # TMDB DTOs and ITmdbService
    ├── Favorites/                        # Favorites DTOs and IFavoriteService
    ├── WatchLater/                       # WatchLater DTOs and IWatchLaterService
    └── WatchHistory/                     # WatchHistory DTOs and IWatchHistoryService
DotNetProject.WebApi/
└── Features/
    ├── Users/                            # User profile and password change endpoints
    ├── Audit/                            # Audit log retrieval and insertion endpoints
    ├── Tmdb/                             # TmdbService & TmdbController
    ├── Favorites/                        # FavoriteService & FavoritesController
    ├── WatchLater/                       # WatchLaterService & WatchLaterController
    └── WatchHistory/                     # WatchHistoryService & WatchHistoryController
DotNetProject.Balzor/
└── Components/
    └── Shared/                           # UI-only presentation services calling WebApi via HTTP
```

**Structure Decision**:
Follows RULES.md clean architecture principles:
- **Domain**: Contains business models, DTOs, and interfaces.
- **WebApi**: Contains database contexts, service implementations, and API controllers.
- **Blazor**: Dedicated exclusively to UI presentation. Interacts with the backend solely via HttpClient. No direct database dependencies.

---

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

*No violations identified. The design utilizes standard HTML5 video APIs, existing EF Core DbContext entities, and the established Blazor Server architecture.*
