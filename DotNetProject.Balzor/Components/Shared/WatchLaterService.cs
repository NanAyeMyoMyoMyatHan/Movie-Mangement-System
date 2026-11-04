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

    public class WatchLaterService : IDisposable
    {
        private readonly AuthService _authService;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<WatchLaterItem> _items = new();
        private string _activeUserKey = "guest";
        private int _activeUserId = 0;

        public event Action? OnChange;

        public WatchLaterService(AuthService authService, IServiceProvider serviceProvider)
        {
            _authService = authService;
            _serviceProvider = serviceProvider;
            _activeUserKey = _authService.GetCurrentUserKey();
            _activeUserId = _authService.CurrentUser.Id;
            _authService.OnAuthStateChanged += HandleAuthStateChanged;
            _ = LoadItemsForActiveUserAsync();
        }

        private async void HandleAuthStateChanged()
        {
            var newKey = _authService.GetCurrentUserKey();
            var newId = _authService.CurrentUser.Id;
            if (_activeUserKey != newKey || _activeUserId != newId)
            {
                _activeUserKey = newKey;
                _activeUserId = newId;
                await LoadItemsForActiveUserAsync();
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
            return Path.Combine(targetDir, $"watchlater_{userKey}.json");
        }

        private void SaveToFile()
        {
            try
            {
                var filePath = GetStoragePath(_activeUserKey);
                var json = JsonSerializer.Serialize(_items, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
            }
            catch { }
        }

        public async Task LoadItemsForActiveUserAsync()
        {
            _items.Clear();

            // 1. If authenticated, load from SQL Server UserWatchLater table
            if (_activeUserId > 0)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetService<AppDbContext>();
                    if (db != null)
                    {
                        var dbRows = await db.UserWatchLaters
                            .Where(w => w.UserId == _activeUserId && !w.IsDeleted)
                            .OrderByDescending(w => w.AddedAt)
                            .ToListAsync();

                        if (dbRows.Any())
                        {
                            foreach (var r in dbRows)
                            {
                                _items.Add(new WatchLaterItem
                                {
                                    Id = r.ContentId,
                                    IsContent = r.IsContent,
                                    Title = r.Title,
                                    PosterUrl = r.PosterUrl,
                                    Category = r.Category,
                                    Description = r.Description,
                                    TotalEpisodes = r.TotalEpisodes,
                                    CurrentEpisode = r.CurrentEpisode,
                                    Status = r.Status,
                                    AddedAt = r.AddedAt
                                });
                            }
                            SaveToFile();
                            return;
                        }
                    }
                }
                catch { }
            }

            // 2. Load from per-user JSON file
            var filePath = GetStoragePath(_activeUserKey);
            if (File.Exists(filePath))
            {
                try
                {
                    var json = File.ReadAllText(filePath);
                    var loaded = JsonSerializer.Deserialize<List<WatchLaterItem>>(json);
                    if (loaded != null)
                    {
                        _items.AddRange(loaded);
                        return;
                    }
                }
                catch { }
            }

            // 3. For guest and admin accounts, seed default curated items if file does not exist
            if (_activeUserKey == "u_1005_admin" || _activeUserKey == "guest" || _activeUserKey.Contains("admin"))
            {
                _items.AddRange(GetDefaultSeeds());
                SaveToFile();
            }
        }

        private static List<WatchLaterItem> GetDefaultSeeds()
        {
            return new List<WatchLaterItem>
            {
                new WatchLaterItem
                {
                    Id = 20004,
                    IsContent = true,
                    Title = "Daung With You",
                    PosterUrl = "/images/daung_with_you.jpg",
                    Category = "Thai BL",
                    Description = "A cheerful Decorative Arts student sparks a sweet campus romance with a jazz music major.",
                    TotalEpisodes = 12,
                    CurrentEpisode = 9,
                    Status = "Watching",
                    AddedAt = DateTime.UtcNow.AddDays(-1)
                },
                new WatchLaterItem
                {
                    Id = 20005,
                    IsContent = true,
                    Title = "KhemJira",
                    PosterUrl = "/images/khemjira.jpg",
                    Category = "Thai BL",
                    Description = "Born under a spiritual curse, Khemjira journeys to Isan to seek salvation from a powerful shaman.",
                    TotalEpisodes = 12,
                    CurrentEpisode = 1,
                    Status = "Want to Watch",
                    AddedAt = DateTime.UtcNow.AddDays(-2)
                },
                new WatchLaterItem
                {
                    Id = 21,
                    IsContent = true,
                    Title = "Vincenzo",
                    PosterUrl = "https://upload.wikimedia.org/wikipedia/en/5/5b/Vincenzo_TV_series.jpg",
                    Category = "Korean Drama",
                    Description = "A Korean-Italian mafia lawyer gives an untouchable conglomerate a taste of its own medicine.",
                    TotalEpisodes = 20,
                    CurrentEpisode = 8,
                    Status = "Watching",
                    AddedAt = DateTime.UtcNow.AddDays(-3)
                },
                new WatchLaterItem
                {
                    Id = 22,
                    IsContent = true,
                    Title = "The Glory",
                    PosterUrl = "https://upload.wikimedia.org/wikipedia/en/7/79/The_Glory_TV_series.jpg",
                    Category = "Korean Drama",
                    Description = "Years after surviving horrific high school bullying, a woman puts an elaborate revenge scheme into motion.",
                    TotalEpisodes = 16,
                    CurrentEpisode = 1,
                    Status = "Want to Watch",
                    AddedAt = DateTime.UtcNow.AddDays(-5)
                },
                new WatchLaterItem
                {
                    Id = 10003,
                    IsContent = false,
                    Title = "Spirited Away",
                    PosterUrl = "https://upload.wikimedia.org/wikipedia/en/d/db/Spirited_Away_Japanese_poster.png",
                    Category = "Movie",
                    Description = "A young girl wanders into a strange world ruled by spirits, witches, and gods.",
                    TotalEpisodes = 1,
                    CurrentEpisode = 1,
                    Status = "Want to Watch",
                    AddedAt = DateTime.UtcNow.AddDays(-7)
                }
            };
        }

        public async Task<List<WatchLaterItem>> GetItemsAsync(string? categoryFilter = null, string? sortBy = null)
        {
            if (!_items.Any())
            {
                await LoadItemsForActiveUserAsync();
            }

            var query = _items.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(categoryFilter) && categoryFilter != "All")
            {
                if (categoryFilter.Equals("Korean Drama", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(i => i.Category != null && i.Category.Contains("Korean", StringComparison.OrdinalIgnoreCase));
                }
                else if (categoryFilter.Equals("Thai BL", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(i => i.Category != null && i.Category.Contains("Thai", StringComparison.OrdinalIgnoreCase));
                }
                else if (categoryFilter.Equals("Movies", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(i => !i.IsContent || (i.Category != null && i.Category.Contains("Movie", StringComparison.OrdinalIgnoreCase)));
                }
                else if (categoryFilter.Equals("Watching", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(i => i.Status == "Watching");
                }
                else if (categoryFilter.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(i => i.Status == "Completed");
                }
            }

            if (sortBy == "title")
            {
                query = query.OrderBy(i => i.Title);
            }
            else
            {
                query = query.OrderByDescending(i => i.AddedAt);
            }

            return query.ToList();
        }

        public async Task<int> GetCountAsync()
        {
            if (!_items.Any())
            {
                await LoadItemsForActiveUserAsync();
            }
            return _items.Count;
        }

        public async Task<bool> IsInWatchLaterAsync(long id, bool isContent = true)
        {
            if (!_items.Any())
            {
                await LoadItemsForActiveUserAsync();
            }
            return _items.Any(i => i.Id == id && i.IsContent == isContent);
        }

        public async Task<bool> ToggleWatchLaterAsync(WatchLaterItem item)
        {
            var existing = _items.FirstOrDefault(i => i.Id == item.Id && i.IsContent == item.IsContent);
            if (existing != null)
            {
                await RemoveItemAsync(item.Id, item.IsContent);
                return false;
            }
            else
            {
                await AddItemAsync(item);
                return true;
            }
        }

        public async Task AddItemAsync(WatchLaterItem item)
        {
            var existing = _items.FirstOrDefault(i => i.Id == item.Id && i.IsContent == item.IsContent);
            if (existing == null)
            {
                item.AddedAt = DateTime.UtcNow;
                _items.Insert(0, item);
            }
            else
            {
                existing.CurrentEpisode = item.CurrentEpisode;
                if (!string.IsNullOrWhiteSpace(item.Status)) existing.Status = item.Status;
                existing.AddedAt = DateTime.UtcNow;
                _items.Remove(existing);
                _items.Insert(0, existing);
            }

            SaveToFile();

            // Persist to SQL Server UserWatchLater table if authenticated
            if (_activeUserId > 0)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetService<AppDbContext>();
                    if (db != null)
                    {
                        var dbWL = await db.UserWatchLaters.FirstOrDefaultAsync(w =>
                            w.UserId == _activeUserId &&
                            w.ContentId == item.Id &&
                            w.IsContent == item.IsContent);

                        if (dbWL == null)
                        {
                            db.UserWatchLaters.Add(new UserWatchLater
                            {
                                UserId = _activeUserId,
                                ContentId = item.Id,
                                IsContent = item.IsContent,
                                Title = item.Title,
                                PosterUrl = item.PosterUrl,
                                Category = item.Category,
                                Description = item.Description,
                                TotalEpisodes = item.TotalEpisodes,
                                CurrentEpisode = item.CurrentEpisode,
                                Status = item.Status,
                                AddedAt = DateTime.UtcNow,
                                IsDeleted = false
                            });
                        }
                        else
                        {
                            dbWL.IsDeleted = false;
                            dbWL.CurrentEpisode = item.CurrentEpisode;
                            dbWL.Status = item.Status;
                            dbWL.AddedAt = DateTime.UtcNow;
                        }
                        await db.SaveChangesAsync();
                    }
                }
                catch { }
            }

            NotifyStateChanged();
        }

        public async Task RemoveItemAsync(long id, bool isContent = true)
        {
            var existing = _items.FirstOrDefault(i => i.Id == id && i.IsContent == isContent);
            if (existing != null)
            {
                _items.Remove(existing);
                SaveToFile();

                // Soft-delete from SQL Server UserWatchLater table if authenticated
                if (_activeUserId > 0)
                {
                    try
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var db = scope.ServiceProvider.GetService<AppDbContext>();
                        if (db != null)
                        {
                            var dbWL = await db.UserWatchLaters.FirstOrDefaultAsync(w =>
                                w.UserId == _activeUserId &&
                                w.ContentId == id &&
                                w.IsContent == isContent &&
                                !w.IsDeleted);

                            if (dbWL != null)
                            {
                                dbWL.IsDeleted = true;
                                await db.SaveChangesAsync();
                            }
                        }
                    }
                    catch { }
                }

                NotifyStateChanged();
            }
        }

        private void NotifyStateChanged() => OnChange?.Invoke();

        public void Dispose()
        {
            _authService.OnAuthStateChanged -= HandleAuthStateChanged;
        }
    }
}
