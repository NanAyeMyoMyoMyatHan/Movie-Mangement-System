# Media Stream & Playback Interface Contract

**Feature**: `003-local-youtube-episodes`  
**Protocol**: Blazor Client Component & ASP.NET Core Static File Serving

---

## 1. YouTube URL Embed Resolver Contract

### Function Signature:
```csharp
public static class MediaStreamHelper
{
    public static bool IsYouTube(string? url);
    public static string? ToYouTubeEmbedUrl(string? rawUrl, bool autoplay = true);
    public static bool IsLocalStream(string? url);
}
```

### Conversion Behavior:
| Input URL | Output Embed URL |
|---|---|
| `https://www.youtube.com/watch?v=dQw4w9WgXcQ` | `https://www.youtube-nocookie.com/embed/dQw4w9WgXcQ?autoplay=1&rel=0` |
| `https://youtu.be/dQw4w9WgXcQ` | `https://www.youtube-nocookie.com/embed/dQw4w9WgXcQ?autoplay=1&rel=0` |
| `https://www.youtube.com/embed/dQw4w9WgXcQ` | `https://www.youtube-nocookie.com/embed/dQw4w9WgXcQ?autoplay=1&rel=0` |
| `/videos/kingdom/video_2026-07-17_16-39-57.mp4` | `/videos/kingdom/video_2026-07-17_16-39-57.mp4` (HTML5 `<video>`) |

---

## 2. Catalog Playable Filter Contract

### Requirement:
When fetching titles for public catalog pages (`Index.razor`, `CategoryView.razor`), the query MUST only return items where:
1. At least one active episode exists with a non-empty `VideoUrl` (either starting with `/videos/` or containing `youtube.com`/`youtu.be`).
2. OR the item is explicitly mapped to a confirmed local PC directory with physical files present.

### Query Logic:
```sql
SELECT c.ContentId, c.Title, c.PosterUrl, c.Language, c.ReleaseDate, COUNT(e.EpisodeId) AS PlayableEpisodes
FROM Content c
INNER JOIN Episode e ON c.ContentId = e.ContentId AND e.Status <> 'Deleted' AND LEN(LTRIM(RTRIM(e.VideoUrl))) > 0
WHERE c.IsDeleted = 0
GROUP BY c.ContentId, c.Title, c.PosterUrl, c.Language, c.ReleaseDate
HAVING COUNT(e.EpisodeId) > 0;
```
