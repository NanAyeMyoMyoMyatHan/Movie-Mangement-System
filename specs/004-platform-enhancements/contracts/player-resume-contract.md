# Contract: Video Player JSInterop & Resume Protocol

**Feature Branch**: `004-platform-enhancements`  
**Date**: 2026-11-21  

This contract defines the client-side JavaScript interop module (`mediaPlayerInterop.js`) and the server-side callback/service protocol for tracking playback timestamps and auto-seeking.

---

## 1. JavaScript Module Interface (`mediaPlayerInterop.js`)

Located at: `DotNetProject.Balzor/wwwroot/js/mediaPlayerInterop.js`

### Methods Exposed to Blazor

```javascript
window.mediaPlayer = {
    /**
     * Initializes playback listener on the specified HTML5 <video> element.
     * @param {string} videoElementId - The DOM ID of the <video> tag (e.g. 'mainVideoPlayer').
     * @param {object} dotNetRef - DotNetObjectReference to the Blazor component.
     * @param {number} resumeTimestampSeconds - Saved timestamp to auto-seek to on metadata loaded.
     */
    initPlayer: function (videoElementId, dotNetRef, resumeTimestampSeconds) {},

    /**
     * Seeks the video to the specified timestamp in seconds.
     * @param {string} videoElementId - The DOM ID of the <video> tag.
     * @param {number} seconds - Position to seek to.
     */
    seekTo: function (videoElementId, seconds) {},

    /**
     * Retrieves the current video playback position in seconds.
     * @param {string} videoElementId - The DOM ID of the <video> tag.
     * @returns {number} Current seconds.
     */
    getCurrentTime: function (videoElementId) {},

    /**
     * Cleans up event listeners when component disposes.
     * @param {string} videoElementId - The DOM ID of the <video> tag.
     */
    disposePlayer: function (videoElementId) {}
};
```

---

## 2. Server Callback Contract (`DotNetObjectReference`)

Exposed on `DramaDetail.razor` via `[JSInvokable]`:

```csharp
[JSInvokable]
public async Task OnPlaybackProgress(int currentSeconds, int durationSeconds)
{
    // Throttled by JS: called at most once every 5 seconds or upon pause/seek
    if (ActiveEpisodeId > 0 && Drama != null)
    {
        await WatchHistoryService.RecordWatchProgressAsync(
            contentId: Drama.ContentId,
            episodeNumber: ActiveEpisodeNumber,
            currentSeconds: currentSeconds,
            durationSeconds: durationSeconds
        );
    }
}
```

---

## 3. Resume Behavior & Thresholds

| Condition | Behavior |
|---|---|
| `currentSeconds < 10` | Treat as beginning, do not prompt/seek (start from 0) |
| `10 <= currentSeconds <= (duration - 30)` | Seek directly to `currentSeconds` or show toast *"Resumed from 14:32"* |
| `currentSeconds > (duration - 30)` | Video completed: reset to 0 to replay |
| YouTube iframe embed | Append `&start={seconds}` parameter to embed URL |
