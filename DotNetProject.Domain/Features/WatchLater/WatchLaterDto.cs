using System;

namespace DotNetProject.Domain.Features.WatchLater
{
    public class WatchLaterItem
    {
        public long Id { get; set; }
        public bool IsContent { get; set; } = true;
        public string Title { get; set; } = string.Empty;
        public string? PosterUrl { get; set; }
        public string? Category { get; set; } = "Korean Drama";
        public string? Description { get; set; }
        public int TotalEpisodes { get; set; } = 16;
        public int CurrentEpisode { get; set; } = 1;
        public string Status { get; set; } = "Want to Watch";
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
