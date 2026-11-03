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
                    if (loaded != null && loaded.Any())
                    {
                        _favorites.AddRange(loaded);
                        return;
                    }
                }
                catch { }
            }

            // Provide default curated favorites for admin and default accounts
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
                    Id = 15,
                    IsContent = true,
                    Title = "Crash Landing on You",
                    PosterUrl = "https://upload.wikimedia.org/wikipedia/en/5/5e/Crash_Landing_on_You_poster.png",
                    Category = "Korean Drama",
                    Description = "A paragliding mishap drops a South Korean heiress into North Korea into the life of an army officer.",
                    Year = "2019",
                    Rating = 9.6,
                    TotalEpisodes = 16,
                    AddedAt = DateTime.UtcNow.AddDays(-12)
                },
                new FavoriteItem
                {
                    Id = 24,
                    IsContent = true,
                    Title = "Lovely Runner",
                    PosterUrl = "https://upload.wikimedia.org/wikipedia/en/6/67/Lovely_Runner.png",
                    Category = "Korean Drama",
                    Description = "A passionate fan travels back in time to save her favorite idol from a tragic fate.",
                    Year = "2024",
                    Rating = 9.4,
                    TotalEpisodes = 16,
                    AddedAt = DateTime.UtcNow.AddDays(-4)
                },
                new FavoriteItem
                {
                    Id = 20005,
                    IsContent = true,
                    Title = "KhemJira",
                    PosterUrl = "/images/khemjira.jpg",
                    Category = "Thai BL",
                    Description = "Born with a curse, Khemjira journeys to Isan to seek help from a formidable local shaman.",
                    Year = "2025",
                    Rating = 9.2,
                    TotalEpisodes = 12,
                    AddedAt = DateTime.UtcNow.AddDays(-3)
                },
                new FavoriteItem
                {
                    Id = 20004,
                    IsContent = true,
                    Title = "Daung With You",
                    PosterUrl = "/images/daung_with_you.jpg",
                    Category = "Thai BL",
                    Description = "A cheerful Decorative Arts student sparks a sweet campus romance with a jazz music major.",
                    Year = "2026",
                    Rating = 9.0,
                    TotalEpisodes = 12,
                    AddedAt = DateTime.UtcNow.AddDays(-2)
                }
            };
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
