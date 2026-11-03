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
        public bool IsContent { get; set; } = true; // true = Drama / Series, false = Movie
        public string Title { get; set; } = string.Empty;
        public string? PosterUrl { get; set; }
        public string? Category { get; set; } = "Korean Drama"; // "Korean Drama", "Thai BL", "Movie"
        public string? Description { get; set; }
        public int TotalEpisodes { get; set; } = 16;
        public int CurrentEpisode { get; set; } = 1;
        public string Status { get; set; } = "Want to Watch"; // "Want to Watch", "Watching", "Completed"
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }

    public class WatchLaterService : IDisposable
    {
        private readonly AuthService _authService;
        private readonly List<WatchLaterItem> _items = new();
        private string _activeUserKey = "guest";

        public event Action? OnChange;

        public WatchLaterService(AuthService authService)
        {
            _authService = authService;
            _activeUserKey = _authService.GetCurrentUserKey();
            _authService.OnAuthStateChanged += HandleAuthStateChanged;
            LoadItemsForActiveUser();
        }

        private void HandleAuthStateChanged()
        {
            var newKey = _authService.GetCurrentUserKey();
            if (_activeUserKey != newKey)
            {
                _activeUserKey = newKey;
                LoadItemsForActiveUser();
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

        private void LoadItemsForActiveUser()
        {
            _items.Clear();
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

            // Notice: For a new user account, Watch Later starts clean and empty.
            // Items are ONLY added when this specific user explicitly adds them to Watch Later!
        }

        public Task<List<WatchLaterItem>> GetItemsAsync(string? categoryFilter = null, string? sortBy = null)
        {
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

            return Task.FromResult(query.ToList());
        }

        public Task<int> GetCountAsync()
        {
            return Task.FromResult(_items.Count);
        }

        public Task<bool> IsInWatchLaterAsync(long id, bool isContent = true)
        {
            bool exists = _items.Any(i => i.Id == id && i.IsContent == isContent);
            return Task.FromResult(exists);
        }

        public Task<bool> ToggleWatchLaterAsync(WatchLaterItem item)
        {
            var existing = _items.FirstOrDefault(i => i.Id == item.Id && i.IsContent == item.IsContent);
            if (existing != null)
            {
                _items.Remove(existing);
                SaveToFile();
                NotifyStateChanged();
                return Task.FromResult(false); // Removed from watch later
            }
            else
            {
                item.AddedAt = DateTime.UtcNow;
                _items.Insert(0, item);
                SaveToFile();
                NotifyStateChanged();
                return Task.FromResult(true); // Added to watch later
            }
        }

        public Task AddItemAsync(WatchLaterItem item)
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
            NotifyStateChanged();
            return Task.CompletedTask;
        }

        public Task RemoveItemAsync(long id, bool isContent = true)
        {
            var existing = _items.FirstOrDefault(i => i.Id == id && i.IsContent == isContent);
            if (existing != null)
            {
                _items.Remove(existing);
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
