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
                    if (loaded != null)
                    {
                        _history.AddRange(loaded);
                        return;
                    }
                }
                catch { }
            }

            // Notice: For a new user account, Continue Watching starts empty.
            // Items are only added when this specific user actually plays or resumes a video!
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
