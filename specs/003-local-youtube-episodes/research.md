# Research & Architecture: Episode Acquisition & Media Streaming

**Feature**: `003-local-youtube-episodes`  
**Status**: Completed  
**Topic**: Methods for sourcing, storing, and streaming complete drama/movie episodes in NangWorld.

---

## 1. Episode Sourcing Methods (Comparative Analysis)

The user asks: *"is there any other way that i can get all ep for my pj, tell me"*.  
Below are the 5 practical ways to acquire and serve drama/movie episodes for a web project, evaluated for storage, cost, complexity, and legality.

### Method 1: Local PC Disk Serving (Current Setup)
- **How it works**: Videos downloaded onto the local PC (e.g. `D:\Alchemy Of Soule S1+S2`, `D:\KingDomS1+S2`, `D:\Daung With You`, `D:\Fool In Love`, `D:\KhemJira`) are mapped to ASP.NET Core static endpoints using `PhysicalFileProvider` (`/videos/kingdom/...`).
- **Pros**:
  - 100% offline and instant loading (no internet bandwidth required).
  - High quality (1080p / 720p crystal clear).
  - Completely free (uses existing disk space).
- **Cons**:
  - High disk usage (1 series of 16-30 episodes consumes 10 GB - 35 GB).
  - Can only be viewed on this local PC or local LAN network.
- **Current status in project**: **78 episodes** exist on `D:\` across 5 series.

---

### Method 2: Official YouTube Embeds & Playlists ("youtube ka nay u lote ya tr ko u pay")
- **How it works**: Many Asian drama production companies (especially Thai BL studios like **GMMTV**, **Studio Wabi Sabi**, **Me Mind Y**, **Mandee Channel**, and Japanese anime channels like **Muse Asia**, **Ani-One**) officially upload entire uncut episodes with multi-language subtitles to YouTube for free international viewing.
- **How to integrate**:
  - Store the YouTube Video ID or embed URL in the `Episode.VideoUrl` column (e.g. `https://www.youtube-nocookie.com/embed/XYZ123`).
  - The Blazor player dynamically renders an `<iframe src="https://www.youtube-nocookie.com/embed/XYZ123?autoplay=1">` when playing.
- **Pros**:
  - **Zero disk space used on PC**.
  - 100% legal, officially licensed streams with subtitles.
  - Video streaming bandwidth is handled by Google's global CDN (no server load).
  - Automatic quality scaling (360p up to 4K).
- **Cons**:
  - Occasional region restrictions (can be toggled or bypassed).
  - Not all Korean Netflix-exclusive dramas are on YouTube (trailers/highlights only, though Thai BL series are almost entirely on YouTube).

---

### Method 3: Telegram Channel Direct Streaming / Bot API
- **How it works**: As already hinted in `DramaDetail.razor` (the `@WEAREALLTRYINGHEREMM` QR code modal), Telegram channels in Myanmar frequently host complete series with Myanmar subtitles (`.mp4` files up to 2GB per episode).
- **How to integrate**:
  - **Option A (Modal/Link)**: Deep link to the specific Telegram post using `tg://resolve?domain=WEAREALLTRYINGHEREMM&post=123`.
  - **Option B (Direct Stream)**: Using a lightweight Telegram Stream Bot or open-source proxy (e.g. `FileStreamBot` or `Telegram-to-Direct-Link`), Telegram file IDs can be turned into direct HTTP streaming links (`https://bot-domain/watch/123.mp4`) that play directly in the HTML5 `<video>` tag!
- **Pros**:
  - Myanmar subtitles pre-hardcoded.
  - Unlimited free cloud storage on Telegram's servers.
- **Cons**:
  - Requires internet connection; bot hosting needed for direct streaming.

---

### Method 4: Cloud Object Storage CDN (Cloudflare R2 / Backblaze B2)
- **How it works**: Store MP4 video files in object storage buckets with zero egress fees:
  - **Cloudflare R2**: 10 GB storage free, **$0 egress/bandwidth cost**.
  - **Backblaze B2**: 10 GB free, $0.006/GB/month.
- **How to integrate**:
  - Upload episodes to the bucket and put the public/signed CDN URL into the database `Episode.VideoUrl`.
- **Pros**:
  - Plays in standard HTML5 `<video>` player anywhere in the world.
  - Doesn't fill up the local `C:\` or `D:\` drive.
- **Cons**:
  - Requires uploading files to the cloud.

---

### Method 5: HLS / m3u8 Adaptive Bitrate Streaming
- **How it works**: Videos are split into small 6-second `.ts` chunks with a `.m3u8` playlist index file.
- **Integration**:
  - Use `hls.js` or `Video.js` in Blazor to stream adaptive bitrates without buffering large single MP4 files.

---

## 2. Technical Decisions for This Project

1. **Strict Playable Catalog Rule ("ma shi tae har ma add nae")**:
   - Filter out mock/unseeded movie placeholders (`Your Name`, `Spirited Away`, `Train to Busan`) from the home page until actual video files or working YouTube stream links are added.
   - Only display the 5 confirmed titles with actual videos (`Alchemy of Souls`, `Kingdom`, `Daung With You`, `Fool In Love`, `KhemJira`).

2. **Automated YouTube URL Detection & Playback**:
   - Enhance the built-in video player in `DramaDetail.razor` and `Index.razor` so that any standard YouTube URL (e.g. `https://www.youtube.com/watch?v=...` or `https://youtu.be/...`) is automatically transformed into an embed URL (`https://www.youtube-nocookie.com/embed/...`) and displayed seamlessly in an iframe with theater mode.

3. **Local PC Video Synchronizer**:
   - Create a SQL script / C# seeder that automatically maps all local MP4 files from `D:\` into the database `Episode` table with correct episode numbers and durations.
