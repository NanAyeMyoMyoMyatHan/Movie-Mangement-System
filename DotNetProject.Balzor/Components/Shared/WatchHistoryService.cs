using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
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

    public class WatchHistoryService : IDisposable
    {
        private readonly AuthService _authService;
        private readonly List<WatchHistoryItem> _history = new();
        private string _activeUserKey = "guest";

        public event Action? OnChange;

        public WatchHistoryService(AuthService authService)
        {
            _authService = authService;
            _activeUserKey = _authService.GetCurrentUserKey();
            _authService.OnAuthStateChanged += HandleAuthStateChanged;
            LoadHistoryForActiveUser();
        }

        private void HandleAuthStateChanged()
        {
            var newKey = _authService.GetCurrentUserKey();
            if (_activeUserKey != newKey)
            {
                _activeUserKey = newKey;
                LoadHistoryForActiveUser();
                OnChange?.Invoke();
            }
        }

        private static string GetBaseStorageDir()
        {
            string currentDir = Directory.GetCurrentDirectory();
            string targetDir;
            if (File.Exists(Path.Combine(currentDir, "Program.cs")))
            {
                targetDir = Path.Combine(currentDir, "Storage");
            }
            else
            {
                string baseDir = AppContext.BaseDirectory;
                string probe = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));
                if (File.Exists(Path.Combine(probe, "Program.cs")))
                {
                    targetDir = Path.Combine(probe, "Storage");
                }
                else
                {
                    targetDir = Path.Combine(baseDir, "Storage");
                }
            }

            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }
            return targetDir;
        }

        private string GetStoragePath(string userKey)
        {
            var targetDir = GetBaseStorageDir();
            return Path.Combine(targetDir, $"watchhistory_{userKey}.json");
        }

        private void SaveToFile()
        {
            try
            {
                var filePath = GetStoragePath(_activeUserKey);
                var json = JsonSerializer.Serialize(_history, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
            }
            catch { }
        }

        private void LoadHistoryForActiveUser()
        {
            _history.Clear();
            var filePath = GetStoragePath(_activeUserKey);

            if (File.Exists(filePath))
            {
                try
                {
                    var json = File.ReadAllText(filePath);
                    var loaded = JsonSerializer.Deserialize<List<WatchHistoryItem>>(json);
                    if (loaded != null && loaded.Any())
                    {
                        _history.AddRange(loaded);
                        return;
                    }
                }
                catch { }
            }

            // Provide default continue watching for admin and default accounts
            if (_activeUserKey == "u_1005_admin" || _activeUserKey == "guest" || _activeUserKey.Contains("admin"))
            {
                _history.AddRange(GetDefaultSeeds());
                SaveToFile();
            }
        }

        private static List<WatchHistoryItem> GetDefaultSeeds()
        {
            return new List<WatchHistoryItem>
            {
                new WatchHistoryItem
                {
                    ContentId = 20004,
                    IsContent = true,
                    Title = "Daung With You",
                    Description = "A bright, cheerful Decorative Arts student falls in love at first sight with a reserved Jazz music student.",
                    PosterUrl = "/images/daung_with_you.jpg",
                    EpisodeNumber = 9,
                    TotalEpisodes = 12,
                    VideoUrl = "/videos/daung-with-you/EP9 1080P.mp4",
                    ProgressPercentage = 45,
                    LastWatchedAt = DateTime.UtcNow.AddMinutes(-30)
                },
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
                    LastWatchedAt = DateTime.UtcNow.AddHours(-3)
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
                    LastWatchedAt = DateTime.UtcNow.AddHours(-12)
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
                    LastWatchedAt = DateTime.UtcNow.AddDays(-1)
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
                    LastWatchedAt = DateTime.UtcNow.AddDays(-2)
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
                    LastWatchedAt = DateTime.UtcNow.AddDays(-3)
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
                    LastWatchedAt = DateTime.UtcNow.AddDays(-4)
                }
            };
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

            SaveToFile();
            NotifyStateChanged();
            return Task.CompletedTask;
        }

        public Task RemoveFromHistoryAsync(long contentId, bool isContent = true)
        {
            var existing = _history.FirstOrDefault(h => h.ContentId == contentId && h.IsContent == isContent);
            if (existing != null)
            {
                _history.Remove(existing);
                SaveToFile();
                NotifyStateChanged();
            }
            return Task.CompletedTask;
        }

        private void NotifyStateChanged() => OnChange?.Invoke();

        public void Dispose()
        {
            _authService.OnAuthStateChanged -= HandleAuthStateChanged;
        }
    }
}
