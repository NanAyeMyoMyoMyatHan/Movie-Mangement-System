using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetProject.Balzor.Components.Shared
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
        public DateTime LastWatchedAt { get; set; } = DateTime.UtcNow;
    }

    public class WatchHistoryService
    {
        private readonly List<WatchHistoryItem> _history = new();
        private bool _isInitialized = false;

        public event Action? OnChange;

        public WatchHistoryService()
        {
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (_isInitialized) return;

            // Seed with initial realistic viewing history
            _history.AddRange(new[]
            {
                new WatchHistoryItem
                {
                    ContentId = 23,
                    IsContent = true,
                    Title = "Alchemy of Souls",
                    Description = "A powerful sorceress whose soul is trapped in another body becomes involved with a young nobleman.",
                    PosterUrl = "https://upload.wikimedia.org/wikipedia/en/b/b5/Alchemy_of_Souls.jpg",
                    EpisodeNumber = 1,
                    TotalEpisodes = 30,
                    VideoUrl = "/videos/video_2026-08-04_13-49-33.mp4",
                    ProgressPercentage = 60,
                    LastWatchedAt = DateTime.UtcNow.AddMinutes(-15)
                },
                new WatchHistoryItem
                {
                    ContentId = 6,
                    IsContent = true,
                    Title = "Bad Buddy",
                    Description = "Two rival families live next door, but their sons secretly develop deep feelings.",
                    PosterUrl = "https://upload.wikimedia.org/wikipedia/en/b/b7/Bad_Buddy_Series_2021_poster.jpg",
                    EpisodeNumber = 6,
                    TotalEpisodes = 12,
                    VideoUrl = "/videos/video_2026-08-04_13-49-33.mp4",
                    ProgressPercentage = 45,
                    LastWatchedAt = DateTime.UtcNow.AddHours(-2)
                },
                new WatchHistoryItem
                {
                    ContentId = 15,
                    IsContent = true,
                    Title = "Crash Landing on You",
                    Description = "A paragliding mishap drops a South Korean heiress in North Korea into the life of an army officer.",
                    PosterUrl = "https://upload.wikimedia.org/wikipedia/en/5/5e/Crash_Landing_on_You_poster.png",
                    EpisodeNumber = 12,
                    TotalEpisodes = 16,
                    VideoUrl = "/videos/video_2026-08-04_13-49-33.mp4",
                    ProgressPercentage = 80,
                    LastWatchedAt = DateTime.UtcNow.AddHours(-5)
                },
                new WatchHistoryItem
                {
                    ContentId = 24,
                    IsContent = true,
                    Title = "Lovely Runner",
                    Description = "A passionate fan travels back in time to save her favorite idol from a tragic fate.",
                    PosterUrl = "https://upload.wikimedia.org/wikipedia/en/6/67/Lovely_Runner.png",
                    EpisodeNumber = 8,
                    TotalEpisodes = 16,
                    VideoUrl = "/videos/video_2026-08-04_13-49-33.mp4",
                    ProgressPercentage = 50,
                    LastWatchedAt = DateTime.UtcNow.AddDays(-1)
                },
                new WatchHistoryItem
                {
                    ContentId = 21,
                    IsContent = true,
                    Title = "Vincenzo",
                    Description = "A Korean-Italian mafia lawyer gives a conglomerate a taste of its own medicine.",
                    PosterUrl = "https://upload.wikimedia.org/wikipedia/en/5/5b/Vincenzo_TV_series.jpg",
                    EpisodeNumber = 4,
                    TotalEpisodes = 20,
                    VideoUrl = "/videos/video_2026-08-04_13-49-33.mp4",
                    ProgressPercentage = 30,
                    LastWatchedAt = DateTime.UtcNow.AddDays(-2)
                },
                new WatchHistoryItem
                {
                    ContentId = 22,
                    IsContent = true,
                    Title = "The Glory",
                    Description = "Years after surviving horrific high school bullying, a woman puts an elaborate revenge scheme into motion.",
                    PosterUrl = "https://upload.wikimedia.org/wikipedia/en/7/79/The_Glory_TV_series.jpg",
                    EpisodeNumber = 10,
                    TotalEpisodes = 16,
                    VideoUrl = "/videos/video_2026-08-04_13-49-33.mp4",
                    ProgressPercentage = 70,
                    LastWatchedAt = DateTime.UtcNow.AddDays(-3)
                }
            });

            _isInitialized = true;
        }

        public Task<List<WatchHistoryItem>> GetHistoryAsync(int limit = 10)
        {
            var list = _history
                .OrderByDescending(h => h.LastWatchedAt)
                .Take(limit)
                .ToList();
            return Task.FromResult(list);
        }

        public Task RecordWatchAsync(
            long contentId,
            string title,
            string? description,
            string? posterUrl,
            int episodeNumber,
            int totalEpisodes = 16,
            string? videoUrl = null,
            bool isContent = true,
            int progressPercentage = 40)
        {
            var existing = _history.FirstOrDefault(h => h.ContentId == contentId && h.IsContent == isContent);
            if (existing != null)
            {
                existing.EpisodeNumber = episodeNumber;
                existing.TotalEpisodes = totalEpisodes > 0 ? totalEpisodes : existing.TotalEpisodes;
                existing.LastWatchedAt = DateTime.UtcNow;
                existing.ProgressPercentage = progressPercentage;
                if (!string.IsNullOrWhiteSpace(posterUrl)) existing.PosterUrl = posterUrl;
                if (!string.IsNullOrWhiteSpace(videoUrl)) existing.VideoUrl = videoUrl;
                if (!string.IsNullOrWhiteSpace(description)) existing.Description = description;

                // Move to front
                _history.Remove(existing);
                _history.Insert(0, existing);
            }
            else
            {
                var newItem = new WatchHistoryItem
                {
                    ContentId = contentId,
                    IsContent = isContent,
                    Title = title,
                    Description = description,
                    PosterUrl = posterUrl,
                    EpisodeNumber = episodeNumber,
                    TotalEpisodes = totalEpisodes > 0 ? totalEpisodes : 16,
                    VideoUrl = videoUrl,
                    ProgressPercentage = progressPercentage,
                    LastWatchedAt = DateTime.UtcNow
                };
                _history.Insert(0, newItem);
            }

            NotifyStateChanged();
            return Task.CompletedTask;
        }

        public Task RemoveFromHistoryAsync(long contentId, bool isContent = true)
        {
            var existing = _history.FirstOrDefault(h => h.ContentId == contentId && h.IsContent == isContent);
            if (existing != null)
            {
                _history.Remove(existing);
                NotifyStateChanged();
            }
            return Task.CompletedTask;
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
