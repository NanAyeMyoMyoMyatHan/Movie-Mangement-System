# Tasks: Local PC Videos & YouTube Stream Integration

**Branch**: `003-local-youtube-episodes`  
**Input**: Design documents from `/specs/003-local-youtube-episodes/`  
**Prerequisites**: [plan.md](file:///c:/Users/Windows11/Desktop/Projects/Movie/specs/003-local-youtube-episodes/plan.md), [spec.md](file:///c:/Users/Windows11/Desktop/Projects/Movie/specs/003-local-youtube-episodes/spec.md), [research.md](file:///c:/Users/Windows11/Desktop/Projects/Movie/specs/003-local-youtube-episodes/research.md), [data-model.md](file:///c:/Users/Windows11/Desktop/Projects/Movie/specs/003-local-youtube-episodes/data-model.md), [contracts/media-stream-contract.md](file:///c:/Users/Windows11/Desktop/Projects/Movie/specs/003-local-youtube-episodes/contracts/media-stream-contract.md), [quickstart.md](file:///c:/Users/Windows11/Desktop/Projects/Movie/specs/003-local-youtube-episodes/quickstart.md)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Verify static provider mappings and prepare stream helper utility.

- [x] T001 Verify `PhysicalFileProvider` routes in `DotNetProject.Balzor/Program.cs` and `DotNetProject.WebApi/Program.cs` for all 5 directories on `D:\` (`D:\Alchemy Of Soule S1+S2`, `D:\KingDomS1+S2`, `D:\Daung With You`, `D:\Fool In Love`, `D:\KhemJira`).
- [x] T002 [P] Create `MediaStreamHelper.cs` in `DotNetProject.Balzor/Helpers/MediaStreamHelper.cs` implementing YouTube URL resolution and local file detection as defined in `contracts/media-stream-contract.md`.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Database synchronization of the 78 physical video files on `D:\` into `MyFlixDb`.

- [x] T003 Verify `Content` and `Episode` table structures in `MyFlixDb` for `VideoUrl`, `EpisodeNumber`, and `Status`.
- [x] T004 Create SQL synchronization script `Scripts/SeedLocalPcEpisodes.sql` mapping:
  - *Alchemy of Souls* (ContentId 23) -> 30 episodes (`/videos/alchemy-of-souls/...`)
  - *Kingdom* (ContentId 20003) -> 13 episodes (`/videos/kingdom/...`)
  - *Daung With You* (ContentId 20004) -> 12 episodes (`/videos/daung-with-you/...`)
  - *Fool In Love* (ContentId 20002) -> 11 episodes (`/videos/fool-in-love/...`)
  - *KhemJira* (ContentId 20005) -> 12 episodes (`/videos/khemjira/...`)
- [x] T005 Execute `Scripts/SeedLocalPcEpisodes.sql` on `localhost` SQL Server.

---

## Phase 3: User Story 1 - Existing Posters Preserved with Verified Media Integration

**Goal**: Preserve all existing posters and cards in the UI without removal, while mapping local files to the 5 local series and YouTube streams to movies/series.

**Independent Test**: Load `/` and verify all movie cards (*Your Name*, *Spirited Away*, *Train to Busan*, *Parasite*), Thai BL cards, and Korean drama cards are visible with posters intact.

- [x] T006 [US1] Preserve all existing movie posters in `DotNetProject.Balzor/Components/Pages/Home/Index.razor.cs`.
- [x] T007 [US1] Preserve all existing Thai BL posters in `GetFallbackThaiList()` in `Index.razor.cs`.
- [x] T008 [US1] Preserve all hero carousel slides in `Index.razor.cs` including both existing movies and verified series.
- [x] T009 [US1] Retain all existing posters across `CategoryView.razor.cs`.

---

## Phase 4: User Story 2 - YouTube Video Streaming Integration ("youtube ka nay u lote ya tr ko u pay") (Priority: P2)

**Goal**: Support embedding official YouTube video streams in the drama episode player without local storage overhead.

**Independent Test**: Configure an episode with a YouTube URL and play it in `DramaDetail.razor` / player component, verifying the responsive `<iframe>` embed.

- [x] T010 [US2] Update `DramaDetail.razor` video container to evaluate `MediaStreamHelper.IsYouTube(selectedEpisode.VideoUrl)`.
- [x] T011 [US2] Render responsive `<iframe>` with `src="https://www.youtube-nocookie.com/embed/{id}?autoplay=1"` when playing a YouTube episode.
- [x] T012 [US2] Retain native HTML5 `<video>` player when playing local `/videos/...` MP4 files.

---

## Phase 5: User Story 3 - Episode Acquisition Guide & Admin Import (Priority: P3)

**Goal**: Document all 5 viable ways to acquire and host episodes for this project.

**Independent Test**: Review `research.md` documentation explaining Local Disk, YouTube Embeds, Telegram Bot Streaming, Cloud CDN (Cloudflare R2), and HLS.

- [x] T013 [P] [US3] Document full instructions in `research.md` and project README for adding episodes via YouTube, Telegram, or Cloudflare R2.
- [x] T014 [US3] Provide SQL templates for adding future YouTube episodes into `Episode` table (`Scripts/SeedYouTubeEpisodes.sql`).

---

## Phase 6: Polish & Verification

**Purpose**: End-to-end regression testing against `quickstart.md`.

- [x] T015 Run validation scenarios from `specs/003-local-youtube-episodes/quickstart.md`.
- [x] T016 Verify playback smoothness and Visual Studio Hot Reload stability.
