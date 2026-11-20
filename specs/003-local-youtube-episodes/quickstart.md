# Quickstart Validation Guide: Local PC Videos & YouTube Stream Integration

**Feature**: `003-local-youtube-episodes`  
**Purpose**: Step-by-step verification and run guide to validate that only playable titles appear in the catalog, local videos stream properly, and YouTube links embed seamlessly.

---

## Prerequisites

1. **Local Media Assets**:
   - Physical video files present on `D:\` (`D:\Alchemy Of Soule S1+S2`, `D:\KingDomS1+S2`, `D:\Daung With You`, `D:\Fool In Love`, `D:\KhemJira`).
2. **Database Engine**:
   - Microsoft SQL Server running locally on localhost, database `MyFlixDb`.
3. **ASP.NET Core Web Services**:
   - `DotNetProject.WebApi` (port 7000 or assigned HTTPS/HTTP port).
   - `DotNetProject.Balzor` (port 7001 or Visual Studio assigned port).
4. **Network Access**:
   - Outbound internet access for YouTube iframe embeds (`youtube-nocookie.com`).

---

## Scenario 1: Validate Catalog Playable Filter ("ma shi tae har ma add nae")

### Objective
Ensure that unseeded/mock titles without video files (e.g. `Your Name`, `Spirited Away`, `Train to Busan`) do not appear on the home page or category views.

### Steps
1. Open browser and navigate to the application home page: `https://localhost:7001/` (or current port).
2. Inspect the **"Featured Movies & Anime"**, **"Trending Dramas"**, and **"Thai BL"** carousels/grids.
3. Verify that **only** titles with verified playable media are displayed (e.g., *Alchemy of Souls*, *Kingdom*, *Daung With You*, *Fool In Love*, *KhemJira*).
4. Verify that titles without playable media (such as `Your Name`, `Spirited Away`, `Train to Busan`) are **absent** from the catalog.

### Expected Outcome
- Zero ghost/unplayable titles are displayed.
- Clicking any visible card navigates to a valid detail page where playable episodes exist.

---

## Scenario 2: Validate Local Video File Streaming

### Objective
Verify that local MP4 files stored on `D:\` stream smoothly inside the Blazor video player using the HTML5 `<video>` element.

### Steps
1. Navigate to the Drama Detail page for *Kingdom*: `/drama/20003` (or click on *Kingdom* card).
2. Click on **Episode 1**.
3. Inspect network activity in browser developer tools (F12 > Network tab).
4. Verify the player makes requests to `/videos/kingdom/video_2026-07-17_16-39-57.mp4` returning `HTTP 200` or `HTTP 206 Partial Content`.
5. Test play, pause, seeking/scrubbing through the timeline, and volume controls.

### Expected Outcome
- Video starts playing immediately without CORS errors or broken media icons.
- Seeking responds smoothly with HTTP 206 byte-range requests.

---

## Scenario 3: Validate YouTube Video Embed Integration

### Objective
Verify that an episode configured with a YouTube URL renders an embedded YouTube player without errors.

### Steps
1. In the database or admin interface, configure an episode with a YouTube URL (e.g. `https://www.youtube.com/watch?v=dQw4w9WgXcQ` or GMMTV official episode link).
2. Open the drama detail page for that series and select the episode.
3. Verify that the player renders the YouTube `<iframe>` (`youtube-nocookie.com/embed/...`).
4. Click play on the YouTube player controls.

### Expected Outcome
- The YouTube video plays inside the theater frame with native YouTube controls and quality options.
- The player layout maintains 16:9 aspect ratio and responsiveness across screen sizes.

---

## Scenario 4: Automated Database Synchronization Verification

### Objective
Confirm that the media synchronization utility or script accurately inventories physical files on `D:\` and syncs them to `MyFlixDb`.

### Steps
1. Run the media synchronization script or endpoint.
2. Query SQL Server:
   ```sql
   SELECT c.Title, COUNT(e.EpisodeId) AS TotalEpisodes
   FROM Content c
   INNER JOIN Episode e ON c.ContentId = e.ContentId
   GROUP BY c.Title;
   ```
3. Check the counts:
   - *Alchemy of Souls*: 30 episodes
   - *Kingdom*: 13 episodes
   - *Daung With You*: 12 episodes
   - *Fool In Love*: 11 episodes
   - *KhemJira*: 12 episodes

### Expected Outcome
- All 78 physical files on `D:\` have corresponding active records in `Episode`.
- All `VideoUrl` values begin with valid route prefixes matching the `PhysicalFileProvider` setup.
