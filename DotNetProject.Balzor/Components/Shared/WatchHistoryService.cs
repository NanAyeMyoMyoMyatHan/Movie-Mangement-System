using DotNetProject.Database.AppDbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly IServiceProvider _serviceProvider;
        private readonly List<WatchHistoryItem> _history = new();
        private string _activeUserKey = "guest";
        private int _activeUserId = 0;

        public event Action? OnChange;

        public WatchHistoryService(AuthService authService, IServiceProvider serviceProvider)
        {
            _authService = authService;
            _serviceProvider = serviceProvider;
            _activeUserKey = _authService.GetCurrentUserKey();
            _activeUserId = _authService.CurrentUser.Id;
            _authService.OnAuthStateChanged += HandleAuthStateChanged;
            _ = LoadHistoryForActiveUserAsync();
        }

        private async void HandleAuthStateChanged()
        {
            var newKey = _authService.GetCurrentUserKey();
            var newId = _authService.CurrentUser.Id;
            if (_activeUserKey != newKey || _activeUserId != newId)
            {
                _activeUserKey = newKey;
                _activeUserId = newId;
                await LoadHistoryForActiveUserAsync();
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

        public async Task LoadHistoryForActiveUserAsync()
        {
            _history.Clear();

            // 1. If authenticated, load from SQL Server UserContinueWatching table
            if (_activeUserId > 0)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetService<AppDbContext>();
                    if (db != null)
                    {
                        var dbRows = await db.UserContinueWatchings
                            .Where(w => w.UserId == _activeUserId && !w.IsDeleted)
                            .OrderByDescending(w => w.LastWatchedAt)
                            .Take(20)
                            .ToListAsync();

                        if (dbRows.Any())
                        {
                            foreach (var r in dbRows)
                            {
                                int pct = r.DurationSeconds > 0 
                                    ? (int)Math.Min(100, Math.Round((double)r.ProgressSeconds / r.DurationSeconds * 100))
                                    : (r.ProgressSeconds > 0 ? 50 : 35);

                                _history.Add(new WatchHistoryItem
                                {
                                    ContentId = r.ContentId,
                                    IsContent = r.IsContent,
                                    Title = r.Title,
                                    PosterUrl = r.PosterUrl,
                                    EpisodeNumber = r.EpisodeNumber,
                                    TotalEpisodes = r.TotalEpisodes > 0 ? r.TotalEpisodes : 16,
                                    ProgressPercentage = pct,
                                    LastWatchedAt = r.LastWatchedAt
                                });
                            }
                            SaveToFile();
                            return;
                        }
                    }
                }
                catch { }
            }

            // 2. Fallback to per-user JSON file
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

            // 3. For guest or admin initial seed only if no history exists
            if (_activeUserKey == "u_1005_admin" || _activeUserKey == "guest")
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

        public async Task RecordWatchAsync(
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
            // 1. Update in-memory
            var existing = _history.FirstOrDefault(h => h.ContentId == contentId && h.IsContent == isContent);
            if (existing != null)
            {
                existing.EpisodeNumber = episodeNumber;
                existing.TotalEpisodes = totalEpisodes > 0 ? totalEpisodes : existing.TotalEpisodes;
                existing.LastWatchedAt = DateTime.UtcNow;
                existing.ProgressPercentage = progressPercentage;
                if (!string.IsNullOrWhiteSpace(posterUrl)) existing.PosterUrl = posterUrl;
                if (!string.IsNullOrWhiteSpace(videoUrl)) existing.VideoUrl = videoUrl;
            }
            else
            {
                _history.Insert(0, new WatchHistoryItem
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
                });
            }

            SaveToFile();

            // 2. Save directly to SQL Server UserContinueWatching table
            if (_activeUserId > 0)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetService<AppDbContext>();
                    if (db != null)
                    {
                        var dbItem = await db.UserContinueWatchings.FirstOrDefaultAsync(w =>
                            w.UserId == _activeUserId &&
                            w.ContentId == contentId &&
                            w.IsContent == isContent &&
                            !w.IsDeleted);

                        if (dbItem != null)
                        {
                            dbItem.EpisodeNumber = episodeNumber;
                            dbItem.TotalEpisodes = totalEpisodes > 0 ? totalEpisodes : dbItem.TotalEpisodes;
                            dbItem.ProgressSeconds = progressPercentage;
                            dbItem.DurationSeconds = 100;
                            dbItem.LastWatchedAt = DateTime.UtcNow;
                            if (!string.IsNullOrWhiteSpace(posterUrl)) dbItem.PosterUrl = posterUrl;
                        }
                        else
                        {
                            db.UserContinueWatchings.Add(new UserContinueWatching
                            {
                                UserId = _activeUserId,
                                ContentId = contentId,
                                IsContent = isContent,
                                Title = title,
                                PosterUrl = posterUrl,
                                EpisodeNumber = episodeNumber,
                                TotalEpisodes = totalEpisodes > 0 ? totalEpisodes : 16,
                                ProgressSeconds = progressPercentage,
                                DurationSeconds = 100,
                                LastWatchedAt = DateTime.UtcNow,
                                IsDeleted = false
                            });
                        }

                        await db.SaveChangesAsync();
                    }
                }
                catch { }
            }

            OnChange?.Invoke();
        }

        public async Task RemoveFromHistoryAsync(long contentId, bool isContent = true)
        {
            _history.RemoveAll(h => h.ContentId == contentId && h.IsContent == isContent);
            SaveToFile();

            if (_activeUserId > 0)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetService<AppDbContext>();
                    if (db != null)
                    {
                        var dbItem = await db.UserContinueWatchings.FirstOrDefaultAsync(w =>
                            w.UserId == _activeUserId &&
                            w.ContentId == contentId &&
                            w.IsContent == isContent &&
                            !w.IsDeleted);

                        if (dbItem != null)
                        {
                            dbItem.IsDeleted = true;
                            await db.SaveChangesAsync();
                        }
                    }
                }
                catch { }
            }

            OnChange?.Invoke();
        }

        public void Dispose()
        {
            _authService.OnAuthStateChanged -= HandleAuthStateChanged;
        }
    }
}
