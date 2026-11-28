using System;

namespace DotNetProject.Domain.Features.WatchHistory
{
    public class WatchHistoryItem
    {
        public long ContentId { get; set; }
        public bool IsContent { get; set; } = true;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? PosterUrl { get; set; }
        public int EpisodeNumber { get; set; } = 1;
        public int TotalEpisodes { get; set; } = 16;
        public string? VideoUrl { get; set; }
        public int ProgressPercentage { get; set; } = 45;
        public int ProgressSeconds { get; set; } = 0;
        public int DurationSeconds { get; set; } = 0;
        public DateTime LastWatchedAt { get; set; } = DateTime.UtcNow;
    }
}
