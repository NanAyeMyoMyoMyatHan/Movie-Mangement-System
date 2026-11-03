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
                    if (loaded != null && loaded.Any())
                    {
                        _items.AddRange(loaded);
                        return;
                    }
                }
                catch { }
            }

            // Provide default curated watch later items for admin and default accounts
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
                    Id = 21,
                    IsContent = true,
                    Title = "Vincenzo",
                    PosterUrl = "https://upload.wikimedia.org/wikipedia/en/5/5b/Vincenzo_TV_series.jpg",
                    Category = "Korean Drama",
                    Description = "A Korean-Italian mafia lawyer gives a conglomerate a taste of its own medicine.",
                    TotalEpisodes = 20,
                    CurrentEpisode = 4,
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
                    Description = "Years after surviving brutal bullying, a woman puts an elaborate revenge scheme into motion.",
                    TotalEpisodes = 16,
                    CurrentEpisode = 10,
                    Status = "Watching",
                    AddedAt = DateTime.UtcNow.AddDays(-4)
                },
                new WatchLaterItem
                {
                    Id = 20003,
                    IsContent = true,
                    Title = "Kingdom",
                    PosterUrl = "/images/kingdom.jpg",
                    Category = "Korean Drama",
                    Description = "While strange rumors grip the kingdom, the crown prince investigates a deadly plague.",
                    TotalEpisodes = 12,
                    CurrentEpisode = 1,
                    Status = "Want to Watch",
                    AddedAt = DateTime.UtcNow.AddDays(-5)
                },
                new WatchLaterItem
                {
                    Id = 24,
                    IsContent = true,
                    Title = "Lovely Runner",
                    PosterUrl = "https://upload.wikimedia.org/wikipedia/en/6/67/Lovely_Runner.png",
                    Category = "Korean Drama",
                    Description = "A passionate fan travels back in time to save her favorite idol from a tragic fate.",
                    TotalEpisodes = 16,
                    CurrentEpisode = 8,
                    Status = "Want to Watch",
                    AddedAt = DateTime.UtcNow.AddDays(-7)
                },
                new WatchLaterItem
                {
                    Id = 6,
                    IsContent = true,
                    Title = "Bad Buddy",
                    PosterUrl = "https://upload.wikimedia.org/wikipedia/en/b/b7/Bad_Buddy_Series_2021_poster.jpg",
                    Category = "Thai BL",
                    Description = "Two rival families live next door, but their sons secretly develop deep feelings.",
                    TotalEpisodes = 12,
                    CurrentEpisode = 6,
                    Status = "Want to Watch",
                    AddedAt = DateTime.UtcNow.AddDays(-8)
                },
                new WatchLaterItem
                {
                    Id = 20005,
                    IsContent = true,
                    Title = "KhemJira",
                    PosterUrl = "/images/khemjira.jpg",
                    Category = "Thai BL",
                    Description = "Born with a curse, Khemjira journeys to Isan to seek help from a formidable local shaman.",
                    TotalEpisodes = 12,
                    CurrentEpisode = 1,
                    Status = "Want to Watch",
                    AddedAt = DateTime.UtcNow.AddDays(-9)
                },
                new WatchLaterItem
                {
                    Id = 20002,
                    IsContent = true,
                    Title = "Fool In Love",
                    PosterUrl = "https://upload.wikimedia.org/wikipedia/en/3/3c/Fool_in_love_the_series.jpg",
                    Category = "Thai BL",
                    Description = "The heir of Araya Group meets an intelligent young successor, sparking unexpected love.",
                    TotalEpisodes = 6,
                    CurrentEpisode = 1,
                    Status = "Want to Watch",
                    AddedAt = DateTime.UtcNow.AddDays(-10)
                }
            };
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
