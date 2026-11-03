using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DotNetProject.Balzor.Components.Shared
{
    public class FavoriteItem
    {
        public long Id { get; set; }
        public bool IsContent { get; set; } = true; // true = Drama / Series, false = Movie
        public string Title { get; set; } = string.Empty;
        public string? PosterUrl { get; set; }
        public string? Category { get; set; } = "Korean Drama"; // "Korean Drama", "Thai BL", "Movie"
        public string? Description { get; set; }
        public string Year { get; set; } = "2024";
        public double Rating { get; set; } = 9.0;
        public int TotalEpisodes { get; set; } = 16;
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }

    public class FavoriteService : IDisposable
    {
        private readonly AuthService _authService;
        private readonly List<FavoriteItem> _favorites = new();
        private string _activeUserKey = "guest";

        public event Action? OnChange;

        public FavoriteService(AuthService authService)
        {
            _authService = authService;
            _activeUserKey = _authService.GetCurrentUserKey();
            _authService.OnAuthStateChanged += HandleAuthStateChanged;
            LoadFavoritesForActiveUser();
        }

        private void HandleAuthStateChanged()
        {
            var newKey = _authService.GetCurrentUserKey();
            if (_activeUserKey != newKey)
            {
                _activeUserKey = newKey;
                LoadFavoritesForActiveUser();
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
            return Path.Combine(targetDir, $"favorites_{userKey}.json");
        }

        private void SaveToFile()
        {
            try
            {
                var filePath = GetStoragePath(_activeUserKey);
                var json = JsonSerializer.Serialize(_favorites, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
            }
            catch { }
        }

        private void LoadFavoritesForActiveUser()
        {
            _favorites.Clear();
            var filePath = GetStoragePath(_activeUserKey);

            if (File.Exists(filePath))
            {
                try
                {
                    var json = File.ReadAllText(filePath);
                    var loaded = JsonSerializer.Deserialize<List<FavoriteItem>>(json);
                    if (loaded != null)
                    {
                        _favorites.AddRange(loaded);
                        return;
                    }
                }
                catch { }
            }

            // Notice: For a new user account, Favorites starts clean and empty.
            // Items are ONLY added when this specific user clicks "Add to Favorites"!
        }

        public Task<List<FavoriteItem>> GetFavoritesAsync(string? categoryFilter = null, string? sortBy = null)
        {
            var query = _favorites.AsEnumerable();

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

            if (sortBy == "title")
            {
                query = query.OrderBy(i => i.Title);
            }
            else if (sortBy == "rating")
            {
                query = query.OrderByDescending(i => i.Rating);
            }
            else
            {
                query = query.OrderByDescending(i => i.AddedAt);
            }

            return Task.FromResult(query.ToList());
        }

        public Task<int> GetCountAsync()
        {
            return Task.FromResult(_favorites.Count);
        }

        public Task<bool> IsFavoriteAsync(long id, bool isContent = true)
        {
            bool exists = _favorites.Any(i => i.Id == id && i.IsContent == isContent);
            return Task.FromResult(exists);
        }

        public Task<bool> ToggleFavoriteAsync(FavoriteItem item)
        {
            var existing = _favorites.FirstOrDefault(i => i.Id == item.Id && i.IsContent == item.IsContent);
            if (existing != null)
            {
                _favorites.Remove(existing);
                SaveToFile();
                NotifyStateChanged();
                return Task.FromResult(false); // Removed from favorites
            }
            else
            {
                item.AddedAt = DateTime.UtcNow;
                _favorites.Insert(0, item);
                SaveToFile();
                NotifyStateChanged();
                return Task.FromResult(true); // Added to favorites
            }
        }

        public Task AddFavoriteAsync(FavoriteItem item)
        {
            if (!_favorites.Any(i => i.Id == item.Id && i.IsContent == item.IsContent))
            {
                item.AddedAt = DateTime.UtcNow;
                _favorites.Insert(0, item);
                SaveToFile();
                NotifyStateChanged();
            }
            return Task.CompletedTask;
        }

        public Task RemoveFavoriteAsync(long id, bool isContent = true)
        {
            var existing = _favorites.FirstOrDefault(i => i.Id == id && i.IsContent == isContent);
            if (existing != null)
            {
                _favorites.Remove(existing);
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
