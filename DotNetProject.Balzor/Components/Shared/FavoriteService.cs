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
        private readonly IServiceProvider _serviceProvider;
        private readonly List<FavoriteItem> _favorites = new();
        private string _activeUserKey = "guest";
        private int _activeUserId = 0;

        public event Action? OnChange;

        public FavoriteService(AuthService authService, IServiceProvider serviceProvider)
        {
            _authService = authService;
            _serviceProvider = serviceProvider;
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

            // When user is not authenticated (Guest), personal lists are strictly 0 items. No dummy seeds.
            if (_activeUserId <= 0 || !_authService.IsAuthenticated())
            {
                return;
            }

            // 1. If authenticated, load exclusively from SQL Server UserFavorites table
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetService<AppDbContext>();
                if (db != null)
                {
                    var dbRows = await db.UserFavorites
                        .Where(f => f.UserId == _activeUserId && !f.IsDeleted)
                        .OrderByDescending(f => f.AddedAt)
                        .ToListAsync();

                    if (dbRows.Any())
                    {
                        foreach (var r in dbRows)
                        {
                            _favorites.Add(new FavoriteItem
                            {
                                Id = r.ContentId,
                                IsContent = r.IsContent,
                                Title = r.Title,
                                PosterUrl = r.PosterUrl,
                                Category = r.Category,
                                Description = r.Description,
                                Year = r.Year ?? "2024",
                                Rating = r.Rating,
                                TotalEpisodes = r.TotalEpisodes,
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
                await RemoveFavoriteAsync(item.Id, item.IsContent);
                return false;
            }
            else
            {
                await AddFavoriteAsync(item);
                return true;
            }
        }

        public async Task AddFavoriteAsync(FavoriteItem item)
        {
            if (!_authService.IsAuthenticated() || _activeUserId <= 0)
            {
                return;
            }

            if (!_favorites.Any(i => i.Id == item.Id && i.IsContent == item.IsContent))
            {
                item.AddedAt = DateTime.UtcNow;
                _favorites.Insert(0, item);
                SaveToFile();

                // Direct SQL Server insertion
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetService<AppDbContext>();
                    if (db != null)
                    {
                        var dbItem = await db.UserFavorites.FirstOrDefaultAsync(f =>
                            f.UserId == _activeUserId &&
                            f.ContentId == item.Id &&
                            f.IsContent == item.IsContent);

                        if (dbItem != null)
                        {
                            dbItem.IsDeleted = false;
                            dbItem.AddedAt = DateTime.UtcNow;
                        }
                        else
                        {
                            db.UserFavorites.Add(new UserFavorite
                            {
                                UserId = _activeUserId,
                                ContentId = item.Id,
                                IsContent = item.IsContent,
                                Title = item.Title,
                                PosterUrl = item.PosterUrl,
                                Category = item.Category,
                                Description = item.Description,
                                Year = item.Year,
                                Rating = item.Rating,
                                TotalEpisodes = item.TotalEpisodes,
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

        public async Task RemoveFavoriteAsync(long id, bool isContent = true)
        {
            if (!_authService.IsAuthenticated() || _activeUserId <= 0)
            {
                return;
            }

            var removed = _favorites.RemoveAll(i => i.Id == id && i.IsContent == isContent);
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
                        var dbItem = await db.UserFavorites.FirstOrDefaultAsync(f =>
                            f.UserId == _activeUserId &&
                            f.ContentId == id &&
                            f.IsContent == isContent &&
                            !f.IsDeleted);

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

        public void Dispose()
        {
            _authService.OnAuthStateChanged -= HandleAuthStateChanged;
        }
    }
}
