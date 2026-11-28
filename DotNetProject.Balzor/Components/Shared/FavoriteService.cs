using DotNetProject.Domain.Features.Favorites;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DotNetProject.Balzor.Components.Shared
{
    public class FavoriteService : IDisposable
    {
        private readonly AuthService _authService;
        private readonly HttpClientService _api;
        private readonly List<FavoriteItem> _favorites = new();
        private string _activeUserKey = "guest";
        private int _activeUserId = 0;

        public event Action? OnChange;

        public FavoriteService(AuthService authService, HttpClientService api)
        {
            _authService = authService;
            _api = api;
            _activeUserKey = _authService.GetCurrentUserKey();
            _activeUserId = _authService.CurrentUser.Id;
            _authService.OnAuthStateChanged += HandleAuthStateChanged;
            _ = LoadFavoritesForActiveUserAsync();
        }

        private async void HandleAuthStateChanged()
        {
            var newKey = _authService.GetCurrentUserKey();
            var newId = _authService.CurrentUser.Id;
            if (_activeUserKey != newKey || _activeUserId != newId)
            {
                _activeUserKey = newKey;
                _activeUserId = newId;
                await LoadFavoritesForActiveUserAsync();
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
                if (_activeUserId <= 0) return;
                var filePath = GetStoragePath(_activeUserKey);
                var json = JsonSerializer.Serialize(_favorites, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
            }
            catch { }
        }

        public async Task LoadFavoritesForActiveUserAsync()
        {
            _favorites.Clear();

            if (_activeUserId <= 0 || !_authService.IsAuthenticated())
            {
                return;
            }

            try
            {
                var apiItems = await _api.GetFavoritesAsync(_activeUserId);
                if (apiItems != null && apiItems.Any())
                {
                    _favorites.AddRange(apiItems);
                    SaveToFile();
                    return;
                }
            }
            catch { }

            // Fallback to per-user JSON file if API is temporarily unreachable
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
                    }
                }
                catch { }
            }
        }

        public async Task<List<FavoriteItem>> GetFavoritesAsync(string? categoryFilter = null, string? sortBy = null)
        {
            if (!_authService.IsAuthenticated() || _activeUserId <= 0)
            {
                return new List<FavoriteItem>();
            }

            if (!_favorites.Any())
            {
                await LoadFavoritesForActiveUserAsync();
            }

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

            return query.ToList();
        }

        public async Task<int> GetCountAsync()
        {
            if (!_authService.IsAuthenticated() || _activeUserId <= 0)
            {
                return 0;
            }

            if (!_favorites.Any())
            {
                await LoadFavoritesForActiveUserAsync();
            }
            return _favorites.Count;
        }

        public async Task<bool> IsFavoriteAsync(long id, bool isContent = true)
        {
            if (!_authService.IsAuthenticated() || _activeUserId <= 0)
            {
                return false;
            }

            if (!_favorites.Any())
            {
                await LoadFavoritesForActiveUserAsync();
            }
            return _favorites.Any(i => i.Id == id && i.IsContent == isContent);
        }

        public async Task<bool> ToggleFavoriteAsync(FavoriteItem item)
        {
            if (!_authService.IsAuthenticated() || _activeUserId <= 0)
            {
                return false;
            }

            var existing = _favorites.FirstOrDefault(i => i.Id == item.Id && i.IsContent == item.IsContent);
            if (existing != null)
            {
                _favorites.Remove(existing);
                SaveToFile();
                OnChange?.Invoke();
                _ = _api.RemoveFavoriteAsync(_activeUserId, item.Id, item.IsContent);
                return false;
            }
            else
            {
                item.AddedAt = DateTime.UtcNow;
                _favorites.Insert(0, item);
                SaveToFile();
                OnChange?.Invoke();
                _ = _api.AddFavoriteAsync(_activeUserId, item);
                return true;
            }
        }

        public async Task RemoveFavoriteAsync(long id, bool isContent = true)
        {
            if (!_authService.IsAuthenticated() || _activeUserId <= 0)
            {
                return;
            }

            var item = _favorites.FirstOrDefault(i => i.Id == id && i.IsContent == isContent);
            if (item != null)
            {
                _favorites.Remove(item);
                SaveToFile();
                OnChange?.Invoke();
                _ = _api.RemoveFavoriteAsync(_activeUserId, id, isContent);
            }
        }

        public void Dispose()
        {
            _authService.OnAuthStateChanged -= HandleAuthStateChanged;
        }
    }
}
