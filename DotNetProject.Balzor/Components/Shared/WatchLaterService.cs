using System;
using System.Collections.Generic;
using System.Linq;
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

    public class WatchLaterService
    {
        private readonly List<WatchLaterItem> _items = new();
        private bool _isInitialized = false;

        public event Action? OnChange;

        public WatchLaterService()
        {
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (_isInitialized) return;

            // Seed initial items matching the user's catalog & favorites
            _items.AddRange(new[]
            {
                new WatchLaterItem
                {
                    Id = 23,
                    IsContent = true,
                    Title = "Alchemy of Souls",
                    PosterUrl = "https://upload.wikimedia.org/wikipedia/en/b/b5/Alchemy_of_Souls.jpg",
                    Category = "Korean Drama",
                    Description = "A powerful sorceress trapped in another body becomes involved with a young nobleman.",
                    TotalEpisodes = 30,
                    CurrentEpisode = 1,
                    Status = "Watching",
                    AddedAt = DateTime.UtcNow.AddDays(-5)
                },
                new WatchLaterItem
                {
                    Id = 20003,
                    IsContent = true,
                    Title = "Kingdom",
                    PosterUrl = "https://static.tvmaze.com/uploads/images/original_untouched/498/1245252.jpg",
                    Category = "Korean Drama",
                    Description = "While strange rumors grip the kingdom, the crown prince investigates a deadly plague.",
                    TotalEpisodes = 12,
                    CurrentEpisode = 1,
                    Status = "Want to Watch",
                    AddedAt = DateTime.UtcNow.AddDays(-3)
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
                    AddedAt = DateTime.UtcNow.AddDays(-2)
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
                    Status = "Watching",
                    AddedAt = DateTime.UtcNow.AddDays(-8)
                },
                new WatchLaterItem
                {
                    Id = 15,
                    IsContent = true,
                    Title = "Crash Landing on You",
                    PosterUrl = "https://upload.wikimedia.org/wikipedia/en/5/5e/Crash_Landing_on_You_poster.png",
                    Category = "Korean Drama",
                    Description = "A paragliding mishap drops a South Korean heiress into the life of an army officer.",
                    TotalEpisodes = 16,
                    CurrentEpisode = 12,
                    Status = "Watching",
                    AddedAt = DateTime.UtcNow.AddDays(-10)
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
                    Status = "Watching",
                    AddedAt = DateTime.UtcNow.AddDays(-7)
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
                    AddedAt = DateTime.UtcNow.AddDays(-12)
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
                    AddedAt = DateTime.UtcNow.AddDays(-14)
                },
                new WatchLaterItem
                {
                    Id = 20004,
                    IsContent = true,
                    Title = "Daung With You",
                    PosterUrl = "https://static.tvmaze.com/uploads/images/original_untouched/611/1528703.jpg",
                    Category = "Thai BL",
                    Description = "A cheerful Decorative Arts student sparks a sweet campus romance with a jazz music major.",
                    TotalEpisodes = 12,
                    CurrentEpisode = 1,
                    Status = "Want to Watch",
                    AddedAt = DateTime.UtcNow.AddDays(-1)
                },
                new WatchLaterItem
                {
                    Id = 20005,
                    IsContent = true,
                    Title = "KhemJira",
                    PosterUrl = "https://static.tvmaze.com/uploads/images/original_untouched/581/1454779.jpg",
                    Category = "Thai BL",
                    Description = "Born with a curse, Khemjira journeys to Isan to seek help from a formidable local shaman.",
                    TotalEpisodes = 12,
                    CurrentEpisode = 1,
                    Status = "Want to Watch",
                    AddedAt = DateTime.UtcNow
                }
            });

            _isInitialized = true;
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
                NotifyStateChanged();
                return Task.FromResult(false); // Removed
            }
            else
            {
                item.AddedAt = DateTime.UtcNow;
                _items.Insert(0, item);
                NotifyStateChanged();
                return Task.FromResult(true); // Added
            }
        }

        public Task AddItemAsync(WatchLaterItem item)
        {
            var existing = _items.FirstOrDefault(i => i.Id == item.Id && i.IsContent == item.IsContent);
            if (existing == null)
            {
                item.AddedAt = DateTime.UtcNow;
                _items.Insert(0, item);
                NotifyStateChanged();
            }
            return Task.CompletedTask;
        }

        public Task RemoveItemAsync(long id, bool isContent = true)
        {
            var existing = _items.FirstOrDefault(i => i.Id == id && i.IsContent == isContent);
            if (existing != null)
            {
                _items.Remove(existing);
                NotifyStateChanged();
            }
            return Task.CompletedTask;
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
