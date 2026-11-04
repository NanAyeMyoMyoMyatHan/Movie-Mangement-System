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
                var filePath = GetStoragePath(_activeUserKey);
                var json = JsonSerializer.Serialize(_favorites, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
            }
            catch { }
        }

        public async Task LoadFavoritesForActiveUserAsync()
        {
            _favorites.Clear();

            // 1. If authenticated, load from SQL Server UserFavorites table
            if (_activeUserId > 0)
            {
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
            }

            // 2. Load from per-user JSON file
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

            // 3. For guest and admin accounts, seed initial curated favorites if file does not exist
            if (_activeUserKey == "u_1005_admin" || _activeUserKey == "guest" || _activeUserKey.Contains("admin"))
            {
                _favorites.AddRange(GetDefaultSeeds());
                SaveToFile();
            }
        }

        private static List<FavoriteItem> GetDefaultSeeds()
        {
            return new List<FavoriteItem>
            {
                new FavoriteItem
                {
                    Id = 23,
                    IsContent = true,
                    Title = "Alchemy of Souls",
                    PosterUrl = "https://upload.wikimedia.org/wikipedia/en/b/b5/Alchemy_of_Souls.jpg",
                    Category = "Korean Drama",
                    Description = "A powerful sorceress whose soul is trapped in another body becomes involved with a young nobleman.",
                    Year = "2022",
                    Rating = 9.4,
                    TotalEpisodes = 30,
                    AddedAt = DateTime.UtcNow.AddDays(-10)
                },
                new FavoriteItem
                {
                    Id = 20003,
                    IsContent = true,
                    Title = "Kingdom",
                    PosterUrl = "/images/kingdom.jpg",
                    Category = "Korean Drama",
                    Description = "While strange rumors grip the kingdom, the crown prince becomes their only hope against a deadly plague.",
                    Year = "2019",
                    Rating = 9.1,
                    TotalEpisodes = 12,
                    AddedAt = DateTime.UtcNow.AddDays(-8)
                },
                new FavoriteItem
                {
                    Id = 20002,
                    IsContent = true,
                    Title = "Fool In Love",
                    PosterUrl = "https://upload.wikimedia.org/wikipedia/en/3/3c/Fool_in_love_the_series.jpg",
                    Category = "Thai BL",
                    Description = "The confident heir to the Araya Group crosses paths with an intelligent young successor with a boxer past.",
                    Year = "2026",
                    Rating = 9.3,
                    TotalEpisodes = 6,
                    AddedAt = DateTime.UtcNow.AddDays(-6)
                },
                new FavoriteItem
                {
                    Id = 7,
                    IsContent = true,
                    Title = "KinnPorsche",
                    PosterUrl = "https://upload.wikimedia.org/wikipedia/en/9/9e/KinnPorsche_series_poster.jpg",
                    Category = "Thai BL",
                    Description = "The second son of a major mafia family is assigned a bodyguard with hidden martial arts skills.",
                    Year = "2022",
                    Rating = 9.5,
                    TotalEpisodes = 14,
                    AddedAt = DateTime.UtcNow.AddDays(-14)
                },
                new FavoriteItem
                {
                    Id = 24,
                    IsContent = true,
                    Title = "Lovely Runner",
                    PosterUrl = "https://upload.wikimedia.org/wikipedia/en/6/67/Lovely_Runner.png",
                    Category = "Korean Drama",
                    Description = "A devoted fan journeys back in time to change the destiny of her beloved idol.",
                    Year = "2024",
                    Rating = 9.4,
                    TotalEpisodes = 16,
                    AddedAt = DateTime.UtcNow.AddDays(-4)
                },
                new FavoriteItem
                {
                    Id = 10002,
                    IsContent = false,
                    Title = "Your Name",
                    PosterUrl = "https://upload.wikimedia.org/wikipedia/en/0/0b/Your_Name_poster.png",
                    Category = "Movie",
                    Description = "Two high school students discover they can swap bodies across time and distance.",
                    Year = "2016",
                    Rating = 9.5,
                    TotalEpisodes = 1,
                    AddedAt = DateTime.UtcNow.AddDays(-20)
                }
            };
        }

        public async Task<List<FavoriteItem>> GetFavoritesAsync(string? categoryFilter = null, string? sortBy = null)
        {
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
            if (!_favorites.Any())
            {
                await LoadFavoritesForActiveUserAsync();
            }
            return _favorites.Count;
        }

        public async Task<bool> IsFavoriteAsync(long id, bool isContent = true)
        {
            if (!_favorites.Any())
            {
                await LoadFavoritesForActiveUserAsync();
            }
            return _favorites.Any(i => i.Id == id && i.IsContent == isContent);
        }

        public async Task<bool> ToggleFavoriteAsync(FavoriteItem item)
        {
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
            var existing = _favorites.FirstOrDefault(i => i.Id == item.Id && i.IsContent == item.IsContent);
            if (existing == null)
            {
                item.AddedAt = DateTime.UtcNow;
                _favorites.Insert(0, item);
                SaveToFile();

                // Persist to SQL Server UserFavorites table if authenticated
                if (_activeUserId > 0)
                {
                    try
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var db = scope.ServiceProvider.GetService<AppDbContext>();
                        if (db != null)
                        {
                            var dbFav = await db.UserFavorites.FirstOrDefaultAsync(f =>
                                f.UserId == _activeUserId &&
                                f.ContentId == item.Id &&
                                f.IsContent == item.IsContent);

                            if (dbFav == null)
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
                            else
                            {
                                dbFav.IsDeleted = false;
                                dbFav.AddedAt = DateTime.UtcNow;
                            }
                            await db.SaveChangesAsync();
                        }
                    }
                    catch { }
                }

                NotifyStateChanged();
            }
        }

        public async Task RemoveFavoriteAsync(long id, bool isContent = true)
        {
            var existing = _favorites.FirstOrDefault(i => i.Id == id && i.IsContent == isContent);
            if (existing != null)
            {
                _favorites.Remove(existing);
                SaveToFile();

                // Soft-delete from SQL Server UserFavorites table if authenticated
                if (_activeUserId > 0)
                {
                    try
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var db = scope.ServiceProvider.GetService<AppDbContext>();
                        if (db != null)
                        {
                            var dbFav = await db.UserFavorites.FirstOrDefaultAsync(f =>
                                f.UserId == _activeUserId &&
                                f.ContentId == id &&
                                f.IsContent == isContent &&
                                !f.IsDeleted);

                            if (dbFav != null)
                            {
                                dbFav.IsDeleted = true;
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
