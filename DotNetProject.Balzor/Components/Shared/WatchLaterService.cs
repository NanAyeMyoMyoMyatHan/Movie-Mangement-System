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
                if (_activeUserId <= 0) return;
                var filePath = GetStoragePath(_activeUserKey);
                var json = JsonSerializer.Serialize(_items, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
            }
            catch { }
        }

        public async Task LoadItemsForActiveUserAsync()
        {
            _items.Clear();

            // When user is not authenticated (Guest), personal lists are strictly 0 items. No dummy seeds.
            if (_activeUserId <= 0 || !_authService.IsAuthenticated())
            {
                return;
            }

            // 1. If authenticated, load exclusively from SQL Server UserWatchLater table
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

            // 2. Fallback to per-user JSON file if db was temporarily unreachable
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
                    }
                }
                catch { }
            }
        }

        public async Task<List<WatchLaterItem>> GetItemsAsync(string? categoryFilter = null, string? statusFilter = null)
        {
            if (!_authService.IsAuthenticated() || _activeUserId <= 0)
            {
                return new List<WatchLaterItem>();
            }

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
            }

            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All")
            {
                query = query.Where(i => i.Status.Equals(statusFilter, StringComparison.OrdinalIgnoreCase));
            }

            return query.OrderByDescending(i => i.AddedAt).ToList();
        }

        public async Task<int> GetCountAsync()
        {
            if (!_authService.IsAuthenticated() || _activeUserId <= 0)
            {
                return 0;
            }

            if (!_items.Any())
            {
                await LoadItemsForActiveUserAsync();
            }
            return _items.Count;
        }

        public async Task<bool> IsInWatchLaterAsync(long id, bool isContent = true)
        {
            if (!_authService.IsAuthenticated() || _activeUserId <= 0)
            {
                return false;
            }

            if (!_items.Any())
            {
                await LoadItemsForActiveUserAsync();
            }
            return _items.Any(i => i.Id == id && i.IsContent == isContent);
        }

        public async Task<bool> ToggleWatchLaterAsync(WatchLaterItem item)
        {
            if (!_authService.IsAuthenticated() || _activeUserId <= 0)
            {
                return false;
            }

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
            if (!_authService.IsAuthenticated() || _activeUserId <= 0)
            {
                return;
            }

            if (!_items.Any(i => i.Id == item.Id && i.IsContent == item.IsContent))
            {
                item.AddedAt = DateTime.UtcNow;
                _items.Insert(0, item);
                SaveToFile();

                // Direct SQL Server insertion
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetService<AppDbContext>();
                    if (db != null)
                    {
                        var dbItem = await db.UserWatchLaters.FirstOrDefaultAsync(w =>
                            w.UserId == _activeUserId &&
                            w.ContentId == item.Id &&
                            w.IsContent == item.IsContent);

                        if (dbItem != null)
                        {
                            dbItem.IsDeleted = false;
                            dbItem.AddedAt = DateTime.UtcNow;
                            dbItem.Status = item.Status;
                            dbItem.CurrentEpisode = item.CurrentEpisode;
                        }
                        else
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
                        await db.SaveChangesAsync();
                    }
                }
                catch { }

                OnChange?.Invoke();
            }
        }

        public async Task RemoveItemAsync(long id, bool isContent = true)
        {
            if (!_authService.IsAuthenticated() || _activeUserId <= 0)
            {
                return;
            }

            var removed = _items.RemoveAll(i => i.Id == id && i.IsContent == isContent);
            if (removed > 0)
            {
                SaveToFile();

                // Direct SQL Server soft-delete
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetService<AppDbContext>();
                    if (db != null)
                    {
                        var dbItem = await db.UserWatchLaters.FirstOrDefaultAsync(w =>
                            w.UserId == _activeUserId &&
                            w.ContentId == id &&
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

                OnChange?.Invoke();
            }
        }

        public async Task UpdateStatusAsync(long id, string newStatus, bool isContent = true)
        {
            if (!_authService.IsAuthenticated() || _activeUserId <= 0)
            {
                return;
            }

            var item = _items.FirstOrDefault(i => i.Id == id && i.IsContent == isContent);
            if (item != null)
            {
                item.Status = newStatus;
                SaveToFile();

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetService<AppDbContext>();
                    if (db != null)
                    {
                        var dbItem = await db.UserWatchLaters.FirstOrDefaultAsync(w =>
                            w.UserId == _activeUserId &&
                            w.ContentId == id &&
                            w.IsContent == isContent &&
                            !w.IsDeleted);

                        if (dbItem != null)
                        {
                            dbItem.Status = newStatus;
                            await db.SaveChangesAsync();
                        }
                    }
                }
                catch { }

                OnChange?.Invoke();
            }
        }

        public async Task UpdateEpisodeProgressAsync(long id, int currentEpisode, bool isContent = true)
        {
            if (!_authService.IsAuthenticated() || _activeUserId <= 0)
            {
                return;
            }

            var item = _items.FirstOrDefault(i => i.Id == id && i.IsContent == isContent);
            if (item != null)
            {
                item.CurrentEpisode = currentEpisode;
                if (currentEpisode >= item.TotalEpisodes)
                {
                    item.Status = "Completed";
                }
                else if (currentEpisode > 1)
                {
                    item.Status = "Watching";
                }
                SaveToFile();

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetService<AppDbContext>();
                    if (db != null)
                    {
                        var dbItem = await db.UserWatchLaters.FirstOrDefaultAsync(w =>
                            w.UserId == _activeUserId &&
                            w.ContentId == id &&
                            w.IsContent == isContent &&
                            !w.IsDeleted);

                        if (dbItem != null)
                        {
                            dbItem.CurrentEpisode = item.CurrentEpisode;
                            dbItem.Status = item.Status;
                            await db.SaveChangesAsync();
                        }
                    }
                }
                catch { }

                OnChange?.Invoke();
            }
        }

        public void Dispose()
        {
            _authService.OnAuthStateChanged -= HandleAuthStateChanged;
        }
    }
}
