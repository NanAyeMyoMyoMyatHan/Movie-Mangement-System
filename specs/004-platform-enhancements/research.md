# Research & Architecture Decisions: Platform Enhancements

**Feature Branch**: `004-platform-enhancements`  
**Date**: 2026-11-21  

This document captures the research, evaluation of alternatives, and architectural decisions for implementing the 5 platform features:
1. Video Resume & Timestamp Remembering
2. Multi-Subtitle & Audio Track (.vtt/.srt)
3. Advanced Multi-facet Filtering & Pagination
4. User Profile & Password Change
5. Activity Log & Audit Trail

---

## 1. Video Resume & Timestamp Remembering

### Research Question
How should Blazor Server interact with HTML5 `<video>` elements to track playback timestamps without overwhelming the SignalR circuit or database with high-frequency updates, while ensuring seamless auto-seek when a user opens an episode?

### Alternatives Considered
1. **Direct Blazor Event Binding (`@ontimeupdate`)**:
   - *Pros*: Pure C# in Blazor component.
   - *Cons*: Triggers 4-5 SignalR server roundtrips per second per active viewer. Causes severe network congestion, lag, and high server CPU usage.
2. **Periodic Polling via C# Timer**:
   - *Pros*: Controls frequency on server.
   - *Cons*: Must constantly poll JS via JSInterop; out of sync if video pauses, buffers, or window closes.
3. **Dedicated Client-Side JSInterop Module (`mediaPlayerInterop.js`) with Throttling (Chosen)**:
   - *Pros*: Attaches native `timeupdate`, `pause`, `ended`, and `beforeunload` listeners in the browser. Throttles timestamp recording to once every 5 seconds. Stores immediate progress in browser `localStorage` and flushes to the server via DotNetObjectReference / API. On video `loadedmetadata`, automatically seeks to `lastWatchedSeconds` or invokes a seamless seek callback.
   - *Cons*: Requires a dedicated JS file in `wwwroot/js/`.

### Decision & Rationale
- **Chosen**: Client-Side JSInterop Module with 5-second throttling and `beforeunload` flush.
- **Auto-seek Mechanism**:
  - When `DramaDetail.razor` loads, it retrieves the user's `ProgressSeconds` from `WatchHistoryService` (or `UserContinueWatching`).
  - If `ProgressSeconds > 10` and progress < 95% of duration, JSInterop seeks: `video.currentTime = progressSeconds`.
  - A subtle toast or badge: *"Resumed from 14:32"* gives visual confirmation.

---

## 2. Multi-Subtitle & Audio Track (Myanmar .vtt Support)

### Research Question
How should Myanmar subtitles (.vtt) and multiple audio/subtitle tracks be modeled, served, and rendered in the video player?

### Alternatives Considered
1. **Hardcoded Subtitles (Burned-in / Hardsub)**:
   - *Pros*: No player support needed.
   - *Cons*: Cannot be disabled; cannot support multiple languages; requires re-encoding heavy video files.
2. **Client-side SRT parser rendering over canvas/div**:
   - *Pros*: Doesn't rely on browser `<track>` support.
   - *Cons*: Complex CSS positioning, synchronization drift with fullscreen, accessibility issues.
3. **HTML5 `<track>` with WebVTT (`.vtt`) format and Static File Serving (Chosen)**:
   - *Pros*: Native browser support, hardware acceleration, automatic fullscreen track handling, works seamlessly across desktop and mobile.
   - *Cons*: Requires `.vtt` format (SRT can be converted to VTT by replacing comma `,` with period `.` in timestamps).

### Decision & Rationale
- **Chosen**: HTML5 `<track>` with WebVTT (`.vtt`).
- **Subtitle File Delivery**:
  - Subtitle files are served via ASP.NET Core `PhysicalFileProvider` / `StaticFiles` under `/subtitles/...` with MIME type `text/vtt; charset=utf-8`.
  - Episode metadata receives an optional `SubtitleUrl` field or subtitle manifest (e.g. `[{"lang":"my","label":"Myanmar (မြန်မာ)","src":"/subtitles/ep1_my.vtt"}]`).
  - UTF-8 encoding ensures pristine Myanmar Unicode glyph rendering (no broken ligatures).

---

## 3. Advanced Multi-facet Filtering & Pagination

### Research Question
How to structure multi-facet filtering (Year, Status Ongoing/Completed, Rating) and pagination in `CategoryView.razor` while maintaining fast client interactions and consistent Dreamy Pastel UI styling?

### Alternatives Considered
1. **Server-Side API re-query on every dropdown change**:
   - *Pros*: Good for 100,000+ items.
   - *Cons*: Noticeable network latency on each filter change; current catalog has hundreds of titles, not millions.
2. **In-Memory Multi-Facet Slicing with Client Pagination (Chosen)**:
   - *Pros*: Instant reactive filtering (<10ms). Category items are already cached in `AllItems`. Allows real-time facet counts (e.g., "Completed (14)"). Simple, robust pagination with customizable items-per-page (12 or 24).
   - *Cons*: Memory usage on client, which is negligible for current and near-future catalog sizes (<5,000 titles).

### Decision & Rationale
- **Chosen**: In-Memory Multi-Facet Slicing with Pagination.
- **Filter Facets**:
  - **Release Year**: All, 2026, 2025, 2024, 2023, 2022, Older.
  - **Status**: All, Ongoing, Completed.
  - **Rating**: All, 9.0+, 8.0+, 7.0+.
  - **Search Query**: Real-time title search.
- **Pagination**:
  - `CurrentPage` (1-based index).
  - `PageSize` = 12 items.
  - Controls: `[Prev]` `[1]` `[2]` ... `[Next]`. Smooth scroll to top on page change.

---

## 4. User Profile & Password Change

### Research Question
What is the optimal architecture for self-service user profile management and secure password change within the existing Blazor InteractiveServer and WebApi structure?

### Alternatives Considered
1. **Admin-only password resets**:
   - *Pros*: Easy to implement.
   - *Cons*: Poor user experience; fails user requirements.
2. **Dedicated Self-Service `/profile` Page with Unified User Service (Chosen)**:
   - *Pros*: Empowers users. Includes profile avatar selection (preset pastel avatars or image URL), display name update, and secure password change with verification of current password and matching new password confirmation.
   - *Cons*: Needs new DTOs and API endpoints in `UserController` / `UserServices`.

### Decision & Rationale
- **Chosen**: Dedicated `/profile` page in Blazor + updated `UserServices` / `UserController`.
- **Password Security**:
  - Compares provided current password against stored password hash.
  - Enforces minimum 6 characters for new password.
  - Confirms new password equals confirmation password.
- **Avatar Support**:
  - Provides a curated set of Dreamy Pastel avatar presets (e.g., pastel avatars 1 to 8) plus custom image URL option.

---

## 5. Activity Log & Audit Trail

### Research Question
How should administrative actions (content deletion, role assignment, content modification) be captured and audited without tightly coupling to individual UI actions?

### Alternatives Considered
1. **Application-level Console/File Logging (Serilog/NLog only)**:
   - *Pros*: Fast to set up.
   - *Cons*: Not queryable from Blazor Admin portal; admins cannot view history in UI.
2. **Database-backed `AuditLog` Entity & Dedicated `AuditLogService` (Chosen)**:
   - *Pros*: Queryable via EF Core; can be displayed in `/admin/audit-logs` dashboard; supports filtering by Admin User, Action Type, Target Entity, and Date. Non-blocking asynchronous logging.
   - *Cons*: Requires a new database table and migration script.

### Decision & Rationale
- **Chosen**: Database-backed `AuditLog` table with `IAuditLogService` injected across Admin services.
- **Audit Schema**:
  - `Id` (BIGINT IDENTITY PK)
  - `UserId` (INT, ID of the admin)
  - `UserName` (NVARCHAR(100), username of admin)
  - `Action` (NVARCHAR(50), e.g., `DELETE_MOVIE`, `UPDATE_ROLE`, `CREATE_CONTENT`, `EDIT_CONTENT`)
  - `TargetEntity` (NVARCHAR(50), e.g., `Movie`, `Role`, `User`, `Content`)
  - `TargetId` (NVARCHAR(100), target entity identifier)
  - `Details` (NVARCHAR(MAX), summary description or JSON snapshot)
  - `Timestamp` (DATETIME2, UTC)
  - `IpAddress` (NVARCHAR(50), optional)
- **Admin Dashboard**:
  - Route: `/admin/audit-logs`
  - Protected by dynamic RBAC: `CanViewAuditLogs` or `CanManageAdmin`.
  - Filterable by Action, Admin, Entity, and Date range.
