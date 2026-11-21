# Feature Specification: Platform Enhancements (Player Resume, Subtitles, Multi-Facet Filters, User Profile & Audit Trail)

**Feature Branch**: `004-platform-enhancements`  
**Created**: 2026-11-21  
**Status**: Ready for Implementation Planning  
**Input**: User request defining 5 core platform features:
1. Video Resume & Timestamp Remembering (Blazor JSInterop auto-seek from last watched time)
2. Multi-Subtitle & Audio Track (Myanmar .vtt support in video player)
3. Advanced Filtering & Pagination (Category View year, status ongoing/completed, rating multi-facet filtering and pagination)
4. User Profile & Password Change (Self-service profile and password change page)
5. Activity Log / Audit Trail (Admin action tracking table for deletes, role changes, edits)

## Clarifications

### Session 2026-11-21

- Q: How should the video player handle resume timestamps when a user has watched past the main content into the end credits (e.g., ≥90% progress)? → A: Option A - Mark completed at ≥90% progress and start from 00:00 on subsequent plays.
- Q: How should the video player discover Myanmar subtitle (.vtt) files when an episode does not have an explicit SubtitleUrl specified in the database? → A: Option A - Convention-based auto-discovery: probe 'wwwroot/subtitles/{contentId}_ep{episodeNumber}_my.vtt' if SubtitleUrl is null or empty.
- Q: How should users choose and store their profile avatars on the self-service /profile page? → A: Option B - Local file upload only (upload JPG/PNG files to server storage via FileStorageService).
- Q: Which administrative operations should be captured in the Activity Log / Audit Trail? → A: Option A - Content & Security mutations: Content/Movie/Episode creates, edits, and deletes + User role and permission alterations.
- Q: How should the Release Year and Rating filter facets be structured in Category View? → A: Option A - Preset year pills (2026, 2025, 2024, 2023, 2022, 'Older') and minimum rating thresholds (9.0+, 8.0+, 7.0+, 'All').

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Video Resume & Timestamp Remembering (Priority: P1)

As a NangWorld user, when I watch an episode or movie and leave (or close my browser), I want the video player to remember my exact timestamp (e.g. 14:32), and automatically ask or seek to that timestamp when I return, so that I can seamlessly continue watching without manually hunting for where I left off.

**Why this priority**: Directly solves a major friction point in streaming media. Users expect modern streaming platforms to remember their exact progress across sessions.

**Independent Test**:
1. Open an episode in `DramaDetail.razor` and play up to 01:25.
2. Refresh or navigate away to Home, then reopen that episode.
3. Verify that the player seeks to 01:25 or presents a "Resume from 01:25" prompt that jumps to that position.

**Acceptance Scenarios**:
1. **Given** an authenticated user playing an MP4 video, **When** playback passes a 5-second interval or is paused, **Then** the current timestamp (`currentTime`) is recorded to `UserContinueWatching` (and synchronized with browser localStorage/session storage).
2. **Given** a saved watch timestamp for a content/episode, **When** the user opens that episode in `DramaDetail.razor`, **Then** the player initializes and seeks to the saved timestamp via Blazor JSInterop once the video metadata is loaded.
3. **Given** a user who has watched ≥90% of an episode duration, **When** reopening the episode, **Then** the system marks the item as completed and starts playback from 00:00 rather than seeking to the end credits.

---

### User Story 2 - Multi-Subtitle & Audio Track Support (.vtt/.srt) (Priority: P2)

As a Burmese drama viewer, I want the built-in video player to support multiple subtitle tracks (especially Myanmar / Burmese subtitles via WebVTT `.vtt` format), so that I can toggle subtitles on/off or switch between Myanmar and original audio/subtitles.

**Why this priority**: Crucial for local audience accessibility. International dramas (Korean, Thai) require Myanmar subtitles for the core viewer demographic.

**Independent Test**:
1. Play an episode that has an associated `.vtt` file (e.g., `/subtitles/ep1_my.vtt`).
2. Verify that the subtitle selector appears in the player UI or HTML5 video tracks.
3. Verify that Myanmar text renders cleanly with proper Unicode typography on screen.

**Acceptance Scenarios**:
1. **Given** an episode with subtitle files configured (e.g., Myanmar .vtt, English .vtt), **When** rendered in the HTML5 player, **Then** `<track>` elements are rendered with `kind="subtitles"`, `srclang="my"`, and `label="Myanmar (မြန်မာ)"`.
2. **Given** an episode without an explicit `SubtitleUrl` in database, **When** the convention file `/subtitles/{contentId}_ep{episodeNumber}_my.vtt` exists in `wwwroot`, **Then** the player automatically attaches and enables the Myanmar subtitle track.
3. **Given** a user watching a drama, **When** they toggle subtitles from the player control bar, **Then** the active subtitle track changes dynamically without restarting video playback.
4. **Given** an episode with neither explicit nor convention subtitles, **When** rendered in the player, **Then** the subtitle selector gracefully hides or shows "Subtitles Unavailable" without errors.

---

### User Story 3 - Advanced Filtering & Pagination in Category View (Priority: P3)

As a user browsing dramas and movies in `CategoryView.razor`, I want to filter titles by Release Year, Completion Status (Ongoing vs Completed), and Minimum Rating, combined with search and paginated navigation, so that I can quickly find high-rated finished or trending series without endlessly scrolling.

**Why this priority**: Enhances content discovery as the catalog grows, preventing cluttered views and slow rendering.

**Independent Test**:
1. Open `/dramas/category/korean` or `/dramas/category/all`.
2. Filter by Year = "2024", Status = "Completed", Rating = "8.5+".
3. Verify that only items matching all criteria are shown, and pagination (e.g. Page 1 of 3) allows browsing with Prev/Next buttons.

**Acceptance Scenarios**:
1. **Given** the Category View page, **When** selecting a filter facet (Year, Status, or Rating), **Then** the list updates reactively to show items matching all active facets.
2. **Given** a filtered result set exceeding page size (e.g., 12 items per page), **When** changing pages, **Then** the view renders the corresponding page slice and scrolls smoothly to the top.
3. **Given** zero items matching a strict filter combination, **When** filters are applied, **Then** a friendly "No titles match your criteria" message with a "Reset Filters" button is displayed.

---

### User Story 4 - Self-Service User Profile & Password Change (Priority: P4)

As an authenticated NangWorld user, I want a dedicated Profile page (`/profile`) where I can view my account details, update my avatar/profile picture, and securely change my password, so that I have complete control over my account security and personal appearance.

**Why this priority**: Essential self-service feature. Eliminates the need for admins to manually reset passwords for ordinary users.

**Independent Test**:
1. Log in and navigate to `/profile`.
2. Select a new avatar and submit. Verify that the top navigation bar avatar updates immediately.
3. Enter current password and a new valid password; submit. Log out and verify successful login with the new password.

**Acceptance Scenarios**:
1. **Given** an authenticated user on `/profile`, **When** they upload a local image file (JPG/PNG, up to 5MB), **Then** the file is saved to server storage via `FileStorageService`, and their profile avatar updates immediately in the database and top navigation bar.
2. **Given** a password change form, **When** the user provides an incorrect current password, **Then** a clear error message is displayed and the password is not changed.
3. **Given** valid current password and matching new passwords satisfying complexity rules, **When** submitted, **Then** the password hash is updated and a success confirmation is shown.

---

### User Story 5 - Activity Log & Audit Trail for Administration (Priority: P5)

As a System Administrator, I want all sensitive administrative actions (such as deleting a movie/drama, updating content, changing user roles or permissions) to be logged in a dedicated Audit Log table and viewable in an Admin Audit Trail dashboard (`/admin/audit-logs`), so that platform security, accountability, and traceability are maintained.

**Why this priority**: Critical enterprise governance and security practice. Ensures accountability across multiple admins.

**Independent Test**:
1. Log in as Admin and delete or edit a movie in `/admin/movies`.
2. Navigate to `/admin/audit-logs`.
3. Verify that a new entry appears showing the Admin username, action (`DELETE_MOVIE` or `UPDATE_ROLE`), entity ID, timestamp, and metadata details.

**Acceptance Scenarios**:
1. **Given** an admin performing a mutation (create, edit, delete content, or grant/revoke roles), **When** the operation completes, **Then** an `AuditLog` record is automatically created with `UserId`, `Action`, `TargetEntity`, `TargetId`, `Details`, `IpAddress`/`Client`, and `TimestampUtc`.
2. **Given** the `/admin/audit-logs` view, **When** filtered by Action Type, Admin User, or Date range, **Then** the matching log entries are displayed in reverse chronological order with pagination.
3. **Given** a non-admin user, **When** attempting to access `/admin/audit-logs`, **Then** dynamic RBAC blocks access with an unauthorized notice or redirects.

---

## Edge Cases

- **Video Seek Failure**: If the video has not loaded metadata when seeking, JSInterop must listen for `loadedmetadata` or `canplay` before applying `currentTime`.
- **Near-Completion Playback Reset**: When a video is opened and its saved progress is ≥90% of total duration, the player treats the episode as completed and initializes playback from 00:00 rather than seeking to end credits.
- **YouTube vs Local Playback Resume**: For local MP4, HTML5 video JSInterop handles exact second resume. For YouTube embeds, resume uses `&start={seconds}` parameter or YouTube IFrame API.
- **Subtitle Sync & Missing Files**: If a `.vtt` file returns 404 or fails to load, the video player must continue playing without throwing unhandled exceptions.
- **Audit Logging Fail-Safe**: If audit log persistence fails, it must not crash the primary user workflow unless strict compliance is required, or it should log an error to server logs.
- **Password Strength & Same-Password**: Password change form must reject submissions where new password equals the current password or is shorter than 6 characters.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide Blazor JSInterop helpers (`mediaPlayerInterop.js`) to track playback position and seek to specified timestamps on HTML5 `<video>` elements.
- **FR-002**: System MUST persist playback timestamps in `UserContinueWatching` in database (and in localStorage for guest or offline resilience) at regular intervals (5-10s) and upon pause/navigation. When watched progress reaches ≥90%, the episode MUST be marked as completed and reset to restart from 00:00 on future plays.
- **FR-003**: System MUST support subtitle track bindings on HTML5 video players using `.vtt` files, including Myanmar language track options (`srclang="my"`).
- **FR-004**: System MUST allow episode metadata to store subtitle track associations (e.g. `SubtitleUrl` or a subtitle track manifest), and automatically fallback to probe convention-based paths (`/subtitles/{contentId}_ep{episodeNumber}_my.vtt`) if `SubtitleUrl` is empty.
- **FR-005**: System MUST provide multi-facet filtering in `CategoryView.razor` supporting Release Year pills (`All`, `2026`, `2025`, `2024`, `2023`, `2022`, `Older`), Status pills (`All`, `Ongoing`, `Completed`), and Rating thresholds (`9+`, `8+`, `7+`, `All`).
- **FR-006**: System MUST provide client-side / server-side pagination with page size options (e.g. 12 or 24 items per page) and smooth scroll-to-top on page change.
- **FR-007**: System MUST provide a dedicated User Profile page (`/profile`) accessible to all authenticated users.
- **FR-008**: System MUST allow users to upload their profile avatar (JPG/PNG, up to 5MB) directly from their device, storing it via `FileStorageService` and updating their account profile picture.
- **FR-009**: System MUST allow users to change their password securely by verifying the existing password and confirming the new password.
- **FR-010**: System MUST establish an `AuditLog` entity in the database to record administrative actions.
- **FR-011**: System MUST record audit log entries whenever Content, Movies, or Episodes are created, edited, or deleted, and whenever User roles or permissions are assigned or modified.
- **FR-012**: System MUST provide an Admin Audit Trail dashboard (`/admin/audit-logs`) with search, action filtering, date filtering, and pagination.
- **FR-013**: System MUST protect the `/admin/audit-logs` dashboard using Dynamic RBAC permissions (requiring `CanViewAuditLogs` or `CanManageAdmin`).
- **FR-014**: System MUST maintain UI consistency with the Dreamy Light Pastel theme across all new and enhanced pages.

### Non-Functional Requirements

- **NFR-001**: JSInterop timestamp sync MUST not introduce stutter or UI freeze during video playback (throttled to max 1 write every 5 seconds).
- **NFR-002**: Subtitle font rendering MUST properly display Myanmar Unicode fonts (e.g., Pyidaungsu, Noto Sans Myanmar) without overlapping diacritics.
- **NFR-003**: Password updates MUST hash passwords using existing security standards (SHA256 / BCrypt / PBKDF2 as used in `DotNetProject.WebApi`).
- **NFR-004**: Audit log writes MUST be performed asynchronously to minimize latency on administrative actions.
