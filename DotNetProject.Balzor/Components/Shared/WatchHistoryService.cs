using DotNetProject.Domain.Features.WatchHistory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DotNetProject.Balzor.Components.Shared
{
    public class WatchHistoryService : IDisposable
    {
        private readonly AuthService _authService;
        private readonly HttpClientService _api;
        private readonly List<WatchHistoryItem> _history = new();
        private string _activeUserKey = "guest";
        private int _activeUserId = 0;

        public event Action? OnChange;

        public WatchHistoryService(AuthService authService, HttpClientService api)
        {
            _authService = authService;
            _api = api;
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
                if (_activeUserId <= 0) return;
                var filePath = GetStoragePath(_activeUserKey);
                var json = JsonSerializer.Serialize(_history, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
            }
            catch { }
        }

        public async Task LoadHistoryForActiveUserAsync()
        {
            _history.Clear();

            if (_activeUserId <= 0 || !_authService.IsAuthenticated())
            {
                return;
            }

            try
            {
                var apiItems = await _api.GetWatchHistoryAsync(_activeUserId);
                if (apiItems != null && apiItems.Any())
                {
                    _history.AddRange(apiItems);
                    SaveToFile();
                    return;
                }
            }
            catch { }

            // Fallback to local JSON file
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
                    }
                }
                catch { }
            }
        }

        public async Task<List<WatchHistoryItem>> GetHistoryAsync(string? categoryFilter = null)
        {
            if (!_authService.IsAuthenticated() || _activeUserId <= 0)
            {
                return new List<WatchHistoryItem>();
            }

            if (!_history.Any())
            {
                await LoadHistoryForActiveUserAsync();
            }

            var query = _history.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(categoryFilter) && categoryFilter != "All")
            {
                if (categoryFilter.Equals("Korean Drama", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(i => i.Description != null && i.Description.Contains("Korean", StringComparison.OrdinalIgnoreCase));
                }
                else if (categoryFilter.Equals("Thai BL", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(i => i.Description != null && i.Description.Contains("Thai", StringComparison.OrdinalIgnoreCase));
                }
                else if (categoryFilter.Equals("Movies", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(i => !i.IsContent || (i.Description != null && i.Description.Contains("Movie", StringComparison.OrdinalIgnoreCase)));
                }
            }

            return query.OrderByDescending(i => i.LastWatchedAt).ToList();
        }

        public async Task<List<WatchHistoryItem>> GetHistoryAsync(int limit)
        {
            var list = await GetHistoryAsync((string?)null);
            return limit > 0 ? list.Take(limit).ToList() : list;
        }

        public async Task<int> GetSavedTimestampAsync(long contentId, int episodeNumber)
        {
            var item = await GetItemAsync(contentId, true, episodeNumber);
            if (item == null) return 0;
            if (item.ProgressPercentage >= 90) return 0;
            return item.ProgressSeconds;
        }

        public async Task RecordWatchAsync(
            long contentId,
            string title,
            string? description,
            string? posterUrl,
            int episodeNumber,
            int totalEpisodes,
            string? videoUrl,
            bool isContent = true,
            int progressPercentage = 0)
        {
            int duration = 3600;
            int progressSec = (int)(duration * (progressPercentage / 100.0));
            await RecordProgressAsync(contentId, isContent, title, posterUrl, description, episodeNumber, totalEpisodes, progressSec, duration, videoUrl);
        }

        public async Task RecordWatchProgressAsync(
            long contentId,
            int episodeNumber,
            int currentSeconds,
            int durationSeconds,
            bool isContent = true)
        {
            var existing = await GetItemAsync(contentId, isContent, episodeNumber);
            string title = existing?.Title ?? $"Content {contentId}";
            string? poster = existing?.PosterUrl;
            string? desc = existing?.Description;
            int totalEps = existing?.TotalEpisodes ?? 16;
            string? videoUrl = existing?.VideoUrl;

            await RecordProgressAsync(contentId, isContent, title, poster, desc, episodeNumber, totalEps, currentSeconds, durationSeconds, videoUrl);
        }

        public async Task<int> GetCountAsync()
        {
            if (!_authService.IsAuthenticated() || _activeUserId <= 0)
            {
                return 0;
            }

            if (!_history.Any())
            {
                await LoadHistoryForActiveUserAsync();
            }
            return _history.Count;
        }

        public async Task<WatchHistoryItem?> GetItemAsync(long contentId, bool isContent = true, int episodeNumber = 1)
        {
            if (!_authService.IsAuthenticated() || _activeUserId <= 0)
            {
                return null;
            }

            var cached = _history.FirstOrDefault(i => i.ContentId == contentId && i.IsContent == isContent && i.EpisodeNumber == episodeNumber);
            if (cached != null) return cached;

            try
            {
                var apiItem = await _api.GetWatchHistoryItemAsync(_activeUserId, contentId, isContent, episodeNumber);
                if (apiItem != null)
                {
                    _history.RemoveAll(i => i.ContentId == contentId && i.IsContent == isContent && i.EpisodeNumber == episodeNumber);
                    _history.Insert(0, apiItem);
                    return apiItem;
                }
            }
            catch { }

            return null;
        }

        public async Task RecordProgressAsync(
            long contentId,
            bool isContent,
            string title,
            string? posterUrl,
            string? description,
            int episodeNumber,
            int totalEpisodes,
            int progressSeconds,
            int durationSeconds,
            string? videoUrl = null)
        {
            if (!_authService.IsAuthenticated() || _activeUserId <= 0)
            {
                return;
            }

            int percent = 0;
            if (durationSeconds > 0 && progressSeconds > 0)
            {
                percent = (int)Math.Min(100, Math.Round((double)progressSeconds / durationSeconds * 100));
            }

            var item = new WatchHistoryItem
            {
                ContentId = contentId,
                IsContent = isContent,
                Title = title,
                PosterUrl = posterUrl,
                Description = description,
                EpisodeNumber = episodeNumber,
                TotalEpisodes = totalEpisodes,
                ProgressSeconds = progressSeconds,
                DurationSeconds = durationSeconds,
                ProgressPercentage = percent,
                VideoUrl = videoUrl,
                LastWatchedAt = DateTime.UtcNow
            };

            var existingIndex = _history.FindIndex(i => i.ContentId == contentId && i.IsContent == isContent && i.EpisodeNumber == episodeNumber);
            if (existingIndex >= 0)
            {
                _history[existingIndex] = item;
            }
            else
            {
                _history.Insert(0, item);
            }

            SaveToFile();
            OnChange?.Invoke();

            _ = _api.SaveWatchProgressAsync(_activeUserId, item);
        }

        public async Task RemoveAsync(long contentId, bool isContent = true)
        {
            if (!_authService.IsAuthenticated() || _activeUserId <= 0)
            {
                return;
            }

            _history.RemoveAll(i => i.ContentId == contentId && i.IsContent == isContent);
            SaveToFile();
            OnChange?.Invoke();

            _ = _api.RemoveWatchHistoryAsync(_activeUserId, contentId, isContent);
        }

        public async Task ClearAllAsync()
        {
            if (!_authService.IsAuthenticated() || _activeUserId <= 0)
            {
                return;
            }

            _history.Clear();
            SaveToFile();
            OnChange?.Invoke();

            _ = _api.ClearWatchHistoryAsync(_activeUserId);
        }

        public void Dispose()
        {
            _authService.OnAuthStateChanged -= HandleAuthStateChanged;
        }
    }
}
