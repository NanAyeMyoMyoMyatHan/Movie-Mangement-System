# Feature Specification: Local PC Videos & YouTube Stream Integration

**Feature Branch**: `003-local-youtube-episodes`  
**Created**: 2026-11-17  
**Status**: Ready for Planning  
**Input**: User request: "ok ,only add video that have in my pc .ma shi tae har ma add nae.youtube ka nay u lote ya tr ko u pay .is there anyother way that i can get all ep for my pj ,tell me /speckit-plan"

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Verified Playable Media Only (Priority: P1)

As a NangWorld user, I want the home page, category views, and recommendations to only feature movies and dramas that have playable media (either verified local video files on my PC or active streaming links), so that I never click on a title only to encounter a missing file, broken link, or non-functional playback.

**Why this priority**: Directly solves "ma shi tae har ma add nae" (do not add items that do not exist). Users must only see titles they can actually watch.

**Independent Test**: Load `/` and browse Korean Dramas, Thai BL, and Movies. Verify that every listed title has playable video files (e.g., Alchemy of Souls, Kingdom, Daung With You, Fool In Love, KhemJira on D:\) and ghost placeholders without videos are excluded.

**Acceptance Scenarios**:
1. **Given** titles in the database, **When** queried by the catalog or home page, **Then** only titles with verified local video files or valid streaming links are returned.
2. **Given** placeholder titles with zero episodes/video files (e.g. unseeded movies or empty entries), **When** browsing the UI, **Then** they are filtered out from public catalog views.

---

### User Story 2 - YouTube Video Streaming Integration (Priority: P2)

As a user without unlimited local disk space, I want series and episodes to support official YouTube video links (`youtube.com/watch` or `youtu.be` or YouTube playlist IDs), so that I can stream full episodes directly inside the built-in theater player via responsive iframe embeds without needing to store tens of gigabytes of MP4 files locally.

**Why this priority**: Directly fulfills "youtube ka nay u lote ya tr ko u pay" (fetch/play from YouTube). Enables streaming popular dramas (especially Thai BL and anime series available officially on YouTube) with zero local storage cost.

**Independent Test**: Open a drama with a YouTube episode URL (e.g. GMMTV official episodes) and verify that the built-in player embeds and plays the YouTube video cleanly with play, pause, fullscreen, and quality selection.

**Acceptance Scenarios**:
1. **Given** an episode with a YouTube URL, **When** the user clicks "Play", **Then** the video player renders the YouTube embedded player (`<iframe src="...">`) with autoplay and controls.
2. **Given** an episode with a local MP4 path (e.g. `/videos/kingdom/...`), **When** the user clicks "Play", **Then** the player uses the HTML5 `<video>` element with hardware-accelerated playback.

---

### User Story 3 - Episode Acquisition & Local Import System (Priority: P3)

As a platform owner/developer, I want clear, documented methods and an automated database synchronizer to link all episodes available on my PC (e.g. scanning `D:\Alchemy Of Soule S1+S2`, `D:\Daung With You`, `D:\Fool In Love`, `D:\KhemJira`, `D:\KingDomS1+S2`), and to understand alternative cloud/streaming sources, so that my project has rich, complete episode catalogs.

**Why this priority**: Answers the user's explicit question: "is there any other way that i can get all ep for my pj, tell me".

**Independent Test**: Run the database synchronization utility to scan local directories on `D:\` and verify that all 78+ local episodes are properly mapped into the `Episode` table with working stream URLs.

**Acceptance Scenarios**:
1. **Given** local folders on `D:\`, **When** the media synchronizer runs, **Then** episode entries are created or updated with correct episode numbers and file paths matching the `StaticFileOptions` routes.
2. **Given** the documentation guide, **When** reviewed, **Then** all 5 viable episode sourcing methods (Local Disk, YouTube Embeds, Telegram Streaming, Cloud Storage CDN, HLS m3u8) are clearly explained with pros and cons.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST only display dramas and movies in the catalog (`Dramas`, `Thais`, `Movies`) that have verified playable media attached.
- **FR-002**: System MUST support local video playback served via ASP.NET Core `PhysicalFileProvider` across the user's verified folders (`D:\Alchemy Of Soule S1+S2`, `D:\KingDomS1+S2`, `D:\Daung With You`, `D:\KhemJira`, `D:\Fool In Love`).
- **FR-003**: System MUST support embedding YouTube video URLs in the episode player, converting standard YouTube watch URLs (`youtube.com/watch?v=XYZ` or `youtu.be/XYZ`) to privacy-enhanced embed URLs (`youtube-nocookie.com/embed/XYZ`).
- **FR-004**: System MUST provide an automated database sync script/seeder that populates `Content` and `Episode` records for all 78+ video files physically existing on the user's PC.
- **FR-005**: System MUST document the 5 distinct methods for getting and streaming episodes into this project (Local PC, YouTube, Telegram, Cloud/CDN, HLS/m3u8).
