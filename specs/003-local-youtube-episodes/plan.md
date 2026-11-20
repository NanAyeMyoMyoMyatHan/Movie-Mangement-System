# Implementation Plan: Local PC Videos & YouTube Stream Integration

**Branch**: `003-local-youtube-episodes` | **Date**: 2026-11-20 | **Spec**: [specs/003-local-youtube-episodes/spec.md](file:///c:/Users/Windows11/Desktop/Projects/Movie/specs/003-local-youtube-episodes/spec.md)

**Input**: User request: *"ok ,only add video that have in my pc .ma shi tae har ma add nae.youtube ka nay u lote ya tr ko u pay .is there anyother way that i can get all ep for my pj ,tell me /speckit-plan"*

---

## Summary

This feature addresses four core requirements:
1. **Catalog Integrity ("ma shi tae har ma add nae")**: Exclude unseeded/mock movies (e.g., *Your Name*, *Spirited Away*, *Train to Busan*) from public views so only movies and series with verified playable video files appear in the catalog.
2. **Local PC Video Synchronization**: Link and sync all 78 physical video files currently stored on the user's `D:\` drive across 5 series (*Alchemy of Souls*, *Kingdom*, *Daung With You*, *Fool In Love*, *KhemJira*) into the `MyFlixDb` database.
3. **YouTube Video Embed Streaming ("youtube ka nay u lote ya tr ko u pay")**: Enhance the theater media player to support embedding official YouTube streams (e.g. Thai BL episodes and anime) with zero local storage footprint.
4. **Episode Acquisition Knowledge Base**: Provide comprehensive documentation of all 5 viable episode acquisition and streaming architectures (Local Disk, YouTube Embeds, Telegram Streaming, Cloudflare R2 / Backblaze B2, and HLS/m3u8).

---

## Technical Context

**Language/Version**: C# 13, .NET 9.0  
**Primary Dependencies**:
- ASP.NET Core Blazor (Interactive Server Components)
- ASP.NET Core Static Files (`Microsoft.AspNetCore.StaticFiles.PhysicalFileProvider`)
- Microsoft.Data.SqlClient / EF Core / Dapper for SQL Server operations
- HTML5 Media Elements (`<video>`) & YouTube IFrame API / responsive embed player  
**Storage**:
- Database: Microsoft SQL Server (`localhost`, database `MyFlixDb`)
- Media Files: Local disk `D:\` (~55.8 GB across 5 folders)  
**Testing**:
- Browser-based scenario verification (documented in [quickstart.md](file:///c:/Users/Windows11/Desktop/Projects/Movie/specs/003-local-youtube-episodes/quickstart.md))
- SQL query verification for content and episode relationships
- HTTP byte-range response checks (`HTTP 206 Partial Content`) for local video streams  
**Target Platform**: Windows 11 / Kestrel Web Server  
**Project Type**: Full-Stack Web Application (Blazor UI + Web API + SQL Server Database)  
**Performance Goals**:
- Instant video start (< 500ms time-to-first-frame for local PC MP4 files)
- Zero local disk footprint for YouTube streamed episodes
- 100% elimination of broken/missing video clicks  
**Constraints**:
- Must not hardcode unplayable fallback mock data in `Index.razor.cs`
- Must maintain Visual Studio Hot Reload compatibility
- Strict Windows PowerShell 5.1 compatibility for helper and maintenance scripts  
**Scale/Scope**:
- 5 local series / 78 physical MP4 files currently on `D:\`
- Extensible to unlimited series via YouTube URLs and future cloud/Telegram streams  

---

## Constitution Check

*GATE: Passed before Phase 0 research. Re-evaluated after Phase 1 design.*

| Principle / Rule | Compliance Status | Details |
|---|---|---|
| **P1: Functional Integrity** | **PASS** | Catalog only displays titles with actual, verified media streams. No ghost cards. |
| **P2: Modular Media Provider** | **PASS** | Media player cleanly distinguishes between HTML5 local streams and YouTube iframes. |
| **P3: Non-Destructive Hot Reload** | **PASS** | Implementation avoids full binary rebuild locks, leveraging Razor hot updates and SQL scripts. |
| **P4: Platform Compatibility** | **PASS** | All scripts and commands adhere to Windows PowerShell 5.1 and forward-slash URL standards. |

---

## Project Structure

### Documentation (this feature)

```text
specs/003-local-youtube-episodes/
├── plan.md                                # This implementation plan
├── spec.md                                # Feature specification & user stories
├── research.md                            # Phase 0: 5 Episode acquisition methods & tech decisions
├── data-model.md                          # Phase 1: DB entities & folder route mappings
├── quickstart.md                          # Phase 1: Step-by-step verification scenarios
├── contracts/
│   └── media-stream-contract.md           # Phase 1: Stream resolution & playable query contracts
└── tasks.md                               # Phase 2: Actionable task list (generated via /speckit-tasks)
```

### Source Code (repository root)

```text
DotNetProject.Balzor/
├── Components/
│   ├── Pages/
│   │   ├── Home/
│   │   │   ├── Index.razor                # Home page UI (Hero carousel, Drama/Movie sections)
│   │   │   ├── Index.razor.cs             # Catalog loading logic (remove unplayable mock fallbacks)
│   │   │   └── Index.razor.css            # Scrim, elevation, and card contrast styles
│   │   ├── Dramas/
│   │   │   ├── DramaDetail.razor          # Drama viewing page & episode list
│   │   │   └── CategoryView.razor         # Filtered views by country/type
│   │   ├── Player/
│   │   │   └── VideoPlayer.razor          # Dynamic HTML5 / YouTube embed player component
│   │   └── Layout/
│   │       ├── MainLayout.razor           # User navigation shell
│   │       └── AdminLayout.razor          # Admin layout (user view toggle removed)
├── Helpers/
│   └── MediaStreamHelper.cs               # YouTube embed parser & stream URL classifier
└── Program.cs                             # PhysicalFileProvider configuration for D:\ directories

DotNetProject.WebApi/
├── Controllers/
│   ├── ContentController.cs               # API endpoint returning playable content only
│   └── EpisodeController.cs               # API endpoint for series episodes & stream URLs
└── Program.cs                             # Static file serving config for D:\

Scripts/
└── SeedLocalPcEpisodes.sql                # SQL synchronization script mapping all 78 files on D:\
```

**Structure Decision**: Monorepo with Blazor frontend (`DotNetProject.Balzor`), Web API backend (`DotNetProject.WebApi`), and shared SQL database (`MyFlixDb`). Media serving relies on direct Kestrel static file streaming via `PhysicalFileProvider` combined with client-side YouTube iframe embedding.

---

## Complexity Tracking

*No constitutional violations identified. No unnecessary dependencies or architectures added.*

| Item | Approach | Why Chosen |
|---|---|---|
| YouTube Playback | Standard `<iframe>` embed | Zero server load, zero storage cost, official license compatibility |
| Local Video Serving | ASP.NET Core `PhysicalFileProvider` | High performance, native range-request handling (seek/scrub support), zero duplicate copying |
| Catalog Filtering | SQL `HAVING COUNT(EpisodeId) > 0` | Guarantees zero ghost cards at query level without client-side guessing |
