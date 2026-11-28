using DotNetProject.Domain.Features.WatchLater;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DotNetProject.Balzor.Components.Shared
{
    public class WatchLaterService : IDisposable
    {
        private readonly AuthService _authService;
        private readonly HttpClientService _api;
        private readonly List<WatchLaterItem> _items = new();
        private string _activeUserKey = "guest";
        private int _activeUserId = 0;

        public event Action? OnChange;

        public WatchLaterService(AuthService authService, HttpClientService api)
        {
            _authService = authService;
            _api = api;
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

            if (_activeUserId <= 0 || !_authService.IsAuthenticated())
            {
                return;
            }

            try
            {
                var apiItems = await _api.GetWatchLaterAsync(_activeUserId);
                if (apiItems != null && apiItems.Any())
                {
                    _items.AddRange(apiItems);
                    SaveToFile();
                    return;
                }
            }
            catch { }

            // Fallback to per-user JSON file if API was temporarily unreachable
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

        public async Task<List<WatchLaterItem>> GetItemsAsync(string? categoryFilter = null, string? statusFilter = null, string? sortBy = null)
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

            if (sortBy == "title")
            {
                query = query.OrderBy(i => i.Title);
            }
            else if (sortBy == "episodes")
            {
                query = query.OrderByDescending(i => i.TotalEpisodes);
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

            if (!_items.Any())
            {
                await LoadItemsForActiveUserAsync();
            }
            return _items.Count;
        }

        public async Task<bool> IsWatchLaterAsync(long id, bool isContent = true)
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

        public Task<bool> IsInWatchLaterAsync(long id, bool isContent = true) => IsWatchLaterAsync(id, isContent);

        public Task RemoveItemAsync(long id, bool isContent = true) => RemoveAsync(id, isContent);

        public async Task<bool> ToggleWatchLaterAsync(WatchLaterItem item)
        {
            if (!_authService.IsAuthenticated() || _activeUserId <= 0)
            {
                return false;
            }

            var existing = _items.FirstOrDefault(i => i.Id == item.Id && i.IsContent == item.IsContent);
            if (existing != null)
            {
                _items.Remove(existing);
                SaveToFile();
                OnChange?.Invoke();
                _ = _api.RemoveWatchLaterAsync(_activeUserId, item.Id, item.IsContent);
                return false;
            }
            else
            {
                item.AddedAt = DateTime.UtcNow;
                _items.Insert(0, item);
                SaveToFile();
                OnChange?.Invoke();
                _ = _api.AddWatchLaterAsync(_activeUserId, item);
                return true;
            }
        }

        public async Task UpdateStatusAsync(long id, bool isContent, string newStatus)
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
                OnChange?.Invoke();
                _ = _api.AddWatchLaterAsync(_activeUserId, item);
            }
        }

        public async Task RemoveAsync(long id, bool isContent = true)
        {
            if (!_authService.IsAuthenticated() || _activeUserId <= 0)
            {
                return;
            }

            var item = _items.FirstOrDefault(i => i.Id == id && i.IsContent == isContent);
            if (item != null)
            {
                _items.Remove(item);
                SaveToFile();
                OnChange?.Invoke();
                _ = _api.RemoveWatchLaterAsync(_activeUserId, id, isContent);
            }
        }

        public void Dispose()
        {
            _authService.OnAuthStateChanged -= HandleAuthStateChanged;
        }
    }
}
