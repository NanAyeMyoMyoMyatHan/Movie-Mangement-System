using DotNetProject.Balzor.Components.Shared;
using DotNetProject.Domain.Features.Contents;
using DotNetProject.Domain.Features.Movies;
using DotNetProject.Domain.Features.Thai;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetProject.Balzor.Components.Pages
{
    public class CategoryItemDto
    {
        public long Id { get; set; }
        public bool IsContent { get; set; } = true;
        public string Title { get; set; } = string.Empty;
        public string? PosterUrl { get; set; }
        public string? Description { get; set; }
        public string Year { get; set; } = "2024";
        public string Category { get; set; } = "Korean Drama";
        public double Rating { get; set; } = 9.2;
        public int TotalEpisodes { get; set; } = 16;
    }

    public partial class CategoryView : ComponentBase, IDisposable
    {
        [Inject]
        private HttpClientService HttpClientService { get; set; } = default!;

        [Inject]
        private NavigationManager Nav { get; set; } = default!;

        [Inject]
        private FavoriteService FavoriteService { get; set; } = default!;

        [Inject]
        private WatchLaterService WatchLaterService { get; set; } = default!;

        [Inject]
        public AuthService AuthService { get; set; } = default!;

        [Parameter]
        public string? CategorySlug { get; set; }

        protected string CurrentSlug { get; set; } = "new-releases";
        protected string CategoryTitle { get; set; } = "New Releases";
        protected string CategorySubtitle { get; set; } = "Explore our latest collection of hit dramas, movies, and series.";
        protected string CategoryBadge { get; set; } = "Curated Releases";

        protected List<CategoryItemDto> AllItems { get; set; } = new();
        protected List<CategoryItemDto> FilteredItems { get; set; } = new();

        protected HashSet<long> FavoriteIds { get; set; } = new();
        protected HashSet<long> WatchLaterIds { get; set; } = new();
        protected string SearchQuery { get; set; } = "";
        protected string SortBy { get; set; } = "year";
        protected bool IsLoading { get; set; } = true;
        protected string? ToastMessage { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Nav.LocationChanged += HandleLocationChanged;
            FavoriteService.OnChange += HandleFavoritesChanged;
            WatchLaterService.OnChange += HandleWatchLaterChanged;
            AuthService.OnAuthStateChanged += HandleAuthChanged;
            await RefreshFavoriteCacheAsync();
            await RefreshWatchLaterCacheAsync();
        }

        protected override async Task OnParametersSetAsync()
        {
            var detectedSlug = ExtractSlug(Nav.Uri, CategorySlug);
            if (detectedSlug != CurrentSlug || !AllItems.Any())
            {
                CurrentSlug = detectedSlug;
                ConfigureCategoryMeta();
                await LoadCategoryDataAsync();
                StateHasChanged();
            }
        }

        private async void HandleLocationChanged(object? sender, LocationChangedEventArgs e)
        {
            await InvokeAsync(async () =>
            {
                var detectedSlug = ExtractSlug(e.Location);
                if (detectedSlug != CurrentSlug)
                {
                    CurrentSlug = detectedSlug;
                    ConfigureCategoryMeta();
                    await LoadCategoryDataAsync();
                    StateHasChanged();
                }
            });
        }


        private string ExtractSlug(string? uri, string? paramSlug = null)
        {
            // 1. Check explicit route parameter if provided
            if (!string.IsNullOrWhiteSpace(paramSlug))
            {
                var s = paramSlug.Trim().ToLowerInvariant();
                if (s.Contains("korean") || s.Contains("k-drama"))
                    return "korean-drama";
                if (s.Contains("bl") || s.Contains("thai"))
                    return "bl-series";
                if (s.Contains("movie"))
                    return "movies";
                if (s.Contains("new"))
                    return "new-releases";
            }

            // 2. Check full URI
            if (!string.IsNullOrWhiteSpace(uri))
            {
                var u = uri.ToLowerInvariant();
                if (u.Contains("korean") || u.Contains("k-drama"))
                    return "korean-drama";
                if (u.Contains("bl-series") || u.Contains("thai-bl") || u.Contains("/category/bl"))
                    return "bl-series";
                if (u.Contains("/category/movies") || u.Contains("/category/movie") || u.EndsWith("movies") || u.EndsWith("movie"))
                    return "movies";
                if (u.Contains("new-releases") || u.EndsWith("new-releases"))
                    return "new-releases";
            }

            return "new-releases";
        }

        private async Task RefreshFavoriteCacheAsync()
        {
            var favs = await FavoriteService.GetFavoritesAsync();
            FavoriteIds = favs.Select(f => f.Id).ToHashSet();
        }

        private async Task RefreshWatchLaterCacheAsync()
        {
            var items = await WatchLaterService.GetItemsAsync();
            WatchLaterIds = items.Select(w => w.Id).ToHashSet();
        }

        private async void HandleAuthChanged()
        {
            await RefreshFavoriteCacheAsync();
            await RefreshWatchLaterCacheAsync();
            await InvokeAsync(StateHasChanged);
        }

        private void ConfigureCategoryMeta()
        {
            switch (CurrentSlug)
            {
                case "bl-series":
                    CategoryTitle = "Thai BL Series";
                    CategorySubtitle = "Discover the most beloved Thai Boys' Love dramas, romantic journeys, and heart-fluttering romances.";
                    CategoryBadge = "BL Series • Curated Collection";
                    SortBy = "rating";
                    break;

                case "korean-drama":
                    CategoryTitle = "Korean Dramas";
                    CategorySubtitle = "Immerse yourself in gripping storylines, unforgettable romantic comedies, and thrilling K-drama masterpieces.";
                    CategoryBadge = "KR Korean Drama • All Episodes";
                    SortBy = "rating";
                    break;

                case "movies":
                    CategoryTitle = "Movies & Cinema";
                    CategorySubtitle = "Stream award-winning cinema, blockbuster thrillers, action hits, and celebrated Asian feature films.";
                    CategoryBadge = "Feature Films • HD Quality";
                    SortBy = "rating";
                    break;

                case "new-releases":
                default:
                    CurrentSlug = "new-releases";
                    CategoryTitle = "New Releases";
                    CategorySubtitle = "Freshly released episodes, trending series, and the newest additions to NangWorld.";
                    CategoryBadge = "Newest Releases 2024 - 2026";
                    SortBy = "year"; // Always default to year descending for New Releases!
                    break;
            }
        }

        protected async Task SetCategory(string slug)
        {
            if (CurrentSlug == slug && AllItems.Any()) return;
            CurrentSlug = slug;
            ConfigureCategoryMeta();
            Nav.NavigateTo($"/category/{slug}");
            await LoadCategoryDataAsync();
            StateHasChanged();
        }

        private async Task LoadCategoryDataAsync()
        {
            IsLoading = true;
            AllItems.Clear();

            try
            {
                if (CurrentSlug == "bl-series")
                {
                    await LoadThaiBLAsync();
                }
                else if (CurrentSlug == "korean-drama")
                {
                    await LoadKoreanDramaAsync();
                }
                else if (CurrentSlug == "movies")
                {
                    await LoadMoviesAsync();
                }
                else
                {
                    await LoadNewReleasesAsync();
                }
            }
            catch
            {
                SeedFallbackForCurrentSlug();
            }

            ApplyFilters();
            IsLoading = false;
        }

        private async Task LoadThaiBLAsync()
        {
            try
            {
                var res = await HttpClientService.GetThaiPagedAsync(new GetThaiPagedListRequest { PageNo = 1, PageSize = 30 });
                if (res != null && res.IsSuccess && res.Data?.Items != null && res.Data.Items.Any())
                {
                    foreach (var t in res.Data.Items)
                    {
                        var poster = CleanPosterUrl(t.PosterUrl, t.Title);
                        AllItems.Add(new CategoryItemDto
                        {
                            Id = t.Id,
                            IsContent = true,
                            Title = t.Title,
                            PosterUrl = poster,
                            Year = t.ReleaseYear?.ToString() ?? "2024",
                            Category = "Thai BL",
                            Rating = 9.2,
                            TotalEpisodes = 12,
                            Description = $"{t.Title} is a popular Thai romance drama full of emotion and charming chemistry."
                        });
                    }
                }
            }
            catch { }

            // Ensure curated Thai BL items are present
            AddSeedIfMissing(20002, true, "Fool In Love", "https://upload.wikimedia.org/wikipedia/en/3/3c/Fool_in_love_the_series.jpg", "2026", "Thai BL", 9.3, 6, "The confident heir of Araya Group meets an intelligent young successor with a boxer past.");
            AddSeedIfMissing(20004, true, "Daung With You", "/images/daung_with_you.jpg", "2026", "Thai BL", 9.0, 12, "A cheerful Decorative Arts student sparks a sweet campus romance with a jazz musician.");
            AddSeedIfMissing(20005, true, "KhemJira", "/images/khemjira.jpg", "2025", "Thai BL", 9.2, 12, "Born under a curse, Khemjira journeys to Isan to seek salvation from a powerful shaman.");
            AddSeedIfMissing(7, true, "KinnPorsche", "https://upload.wikimedia.org/wikipedia/en/9/9e/KinnPorsche_series_poster.jpg", "2022", "Thai BL", 9.5, 14, "The second son of a major mafia family is assigned an extraordinary bodyguard.");
            AddSeedIfMissing(6, true, "Bad Buddy", "https://upload.wikimedia.org/wikipedia/en/b/b7/Bad_Buddy_Series_2021_poster.jpg", "2021", "Thai BL", 9.2, 12, "Two rival families live next door, but their sons secretly develop deep feelings.");
            AddSeedIfMissing(5, true, "2gether: The Series", "https://upload.wikimedia.org/wikipedia/en/6/68/2gether_The_Series_2020_poster.jpg", "2020", "Thai BL", 9.1, 13, "A college student asks an attractive guitarist to be his fake boyfriend, leading to unexpected love.");
            AddSeedIfMissing(8, true, "TharnType: The Series", "https://upload.wikimedia.org/wikipedia/en/7/73/TharnType_The_Series_2019_poster.jpg", "2019", "Thai BL", 8.9, 12, "Two college freshmen rooming together gradually overcome deep prejudices.");
            AddSeedIfMissing(10, true, "A Tale of Thousand Stars", "https://upload.wikimedia.org/wikipedia/en/0/0e/A_Tale_of_Thousand_Stars_2021_poster.jpg", "2021", "Thai BL", 9.4, 10, "A young man receives a heart transplant and travels to a cliffside village to fulfill a promise.");
            AddSeedIfMissing(11, true, "I Told Sunset About You", "https://upload.wikimedia.org/wikipedia/en/a/ab/I_Told_Sunset_About_You_poster.jpg", "2020", "Thai BL", 9.4, 5, "Two childhood friends in Phuket rekindle their deep bond while studying for university admissions.");
            AddSeedIfMissing(12, true, "My School President", "https://upload.wikimedia.org/wikipedia/en/2/2b/My_School_President_GMMTV_Poster.png", "2022", "Thai BL", 9.3, 12, "The student council president secretly helps the school music club survive disbandment.");
        }

        private async Task LoadKoreanDramaAsync()
        {
            try
            {
                var res = await HttpClientService.GetContentsPagedAsync(new GetContentPagedListRequest { PageNo = 1, PageSize = 30 });
                if (res != null && res.IsSuccess && res.Data?.Items != null && res.Data.Items.Any())
                {
                    foreach (var d in res.Data.Items)
                    {
                        var poster = CleanPosterUrl(d.PosterUrl, d.Title);
                        AllItems.Add(new CategoryItemDto
                        {
                            Id = d.ContentId,
                            IsContent = true,
                            Title = d.Title,
                            PosterUrl = poster,
                            Year = d.ReleaseDate?.Year.ToString() ?? "2023",
                            Category = "Korean Drama",
                            Rating = 9.3,
                            TotalEpisodes = 16,
                            Description = d.Description ?? $"{d.Title} is an acclaimed Korean drama series."
                        });
                    }
                }
            }
            catch { }

            // Ensure top K-dramas are present
            AddSeedIfMissing(23, true, "Alchemy of Souls", "https://upload.wikimedia.org/wikipedia/en/b/b5/Alchemy_of_Souls.jpg", "2022", "Korean Drama", 9.4, 30, "A powerful sorceress whose soul is trapped in another body becomes involved with a young nobleman.");
            AddSeedIfMissing(20003, true, "Kingdom", "/images/kingdom.jpg", "2019", "Korean Drama", 9.1, 12, "While strange rumors grip the kingdom, the crown prince investigates a mysterious plague.");
            AddSeedIfMissing(24, true, "Lovely Runner", "https://upload.wikimedia.org/wikipedia/en/6/67/Lovely_Runner.png", "2024", "Korean Drama", 9.4, 16, "A passionate fan travels back in time to save her favorite top idol from a tragic fate.");
            AddSeedIfMissing(15, true, "Crash Landing on You", "https://upload.wikimedia.org/wikipedia/en/5/5e/Crash_Landing_on_You_poster.png", "2019", "Korean Drama", 9.6, 16, "A paragliding mishap drops a South Korean heiress into North Korea into the life of an army officer.");
            AddSeedIfMissing(21, true, "Vincenzo", "https://upload.wikimedia.org/wikipedia/en/5/5b/Vincenzo_TV_series.jpg", "2021", "Korean Drama", 9.3, 20, "A Korean-Italian mafia lawyer gives an untouchable conglomerate a taste of its own medicine.");
            AddSeedIfMissing(22, true, "The Glory", "https://upload.wikimedia.org/wikipedia/en/7/79/The_Glory_TV_series.jpg", "2022", "Korean Drama", 9.3, 16, "Years after surviving horrific high school bullying, a woman puts an elaborate revenge scheme into motion.");
            AddSeedIfMissing(16, true, "Goblin", "https://upload.wikimedia.org/wikipedia/en/6/68/Goblin_Poster.jpg", "2016", "Korean Drama", 9.5, 16, "An immortal goblin yearns to end his cursed immortality and seeks his mortal bride.");
            AddSeedIfMissing(17, true, "Itaewon Class", "https://upload.wikimedia.org/wikipedia/en/9/99/Itaewon_Class.jpg", "2020", "Korean Drama", 9.1, 16, "An ex-con and his ambitious crew open a small street bar in Itaewon to defeat a rival food giant.");
            AddSeedIfMissing(18, true, "Business Proposal", "https://upload.wikimedia.org/wikipedia/en/1/19/A_Business_Proposal.jpg", "2022", "Korean Drama", 9.2, 12, "An employee goes on a blind date in place of her friend, only to discover the date is her company CEO.");
            AddSeedIfMissing(19, true, "True Beauty", "https://upload.wikimedia.org/wikipedia/en/7/76/True_Beauty_TV_series_poster.jpg", "2020", "Korean Drama", 9.0, 16, "A high school girl masters makeup artistry to transform herself into the most popular girl at school.");
            AddSeedIfMissing(20, true, "Twenty-Five Twenty-One", "https://upload.wikimedia.org/wikipedia/en/1/15/Twenty-Five_Twenty-One.jpg", "2022", "Korean Drama", 9.2, 16, "In a time when dreams seem out of reach, a teen fencer pursues big ambitions and meets a hardworking young man.");
        }

        private async Task LoadMoviesAsync()
        {
            try
            {
                var res = await HttpClientService.GetMoviesPagedAsync(new GetMoviePagedListRequest { PageNo = 1, PageSize = 30 });
                if (res != null && res.IsSuccess && res.Data?.Items != null && res.Data.Items.Any())
                {
                    foreach (var m in res.Data.Items)
                    {
                        var poster = CleanPosterUrl(m.PosterUrl, m.Title);
                        AllItems.Add(new CategoryItemDto
                        {
                            Id = m.Id,
                            IsContent = false,
                            Title = m.Title,
                            PosterUrl = poster,
                            Year = m.ReleaseYear?.ToString() ?? "2024",
                            Category = "Movie",
                            Rating = 9.0,
                            TotalEpisodes = 1,
                            Description = $"{m.Title} is an exciting cinematic feature film."
                        });
                    }
                }
            }
            catch { }

            // Ensure top movies are present
            AddSeedIfMissing(10002, false, "Your Name", "https://upload.wikimedia.org/wikipedia/en/0/0b/Your_Name_poster.png", "2016", "Movie", 9.5, 1, "Two teenagers share a profound, magical connection upon discovering they are swapping bodies.");
            AddSeedIfMissing(10003, false, "Spirited Away", "https://upload.wikimedia.org/wikipedia/en/d/db/Spirited_Away_Japanese_poster.png", "2001", "Movie", 9.6, 1, "A sullen ten-year-old girl wanders into a world ruled by gods, witches, and spirits.");
            AddSeedIfMissing(10004, false, "Train to Busan", "https://upload.wikimedia.org/wikipedia/en/9/95/Train_to_Busan.jpg", "2016", "Movie", 9.4, 1, "While a zombie virus breaks out in South Korea, passengers struggle to survive on a high-speed train.");
            AddSeedIfMissing(30001, false, "Parasite", "https://upload.wikimedia.org/wikipedia/en/5/53/Parasite_%282019_film%29.png", "2019", "Movie", 9.6, 1, "Greed and class discrimination threaten the newly formed symbiotic relationship between two families.");
            AddSeedIfMissing(30003, false, "The Childe", "https://upload.wikimedia.org/wikipedia/en/9/90/The_Childe.jpg", "2023", "Movie", 9.0, 1, "A young boxer of Korean-Filipino descent is relentlessly pursued by various underground forces.");
            AddSeedIfMissing(30004, false, "Bad Genius", "https://upload.wikimedia.org/wikipedia/en/0/0e/Bad_Genius_poster.jpg", "2017", "Movie", 9.2, 1, "A straight-A student invents an intricate exam-cheating scheme that escalates to international levels.");
            AddSeedIfMissing(30005, false, "Along with the Gods", "https://upload.wikimedia.org/wikipedia/en/b/b5/Along_With_the_Gods_The_Two_Worlds.jpg", "2017", "Movie", 9.1, 1, "A firefighter passes away unexpectedly and is escorted through seven trials in the afterlife.");
            AddSeedIfMissing(30006, false, "Decibel", "https://upload.wikimedia.org/wikipedia/en/5/52/Decibel_film_poster.jpg", "2022", "Movie", 8.9, 1, "A former naval submarine commander races to stop a sound-activated explosive terrorism plot in the city.");
        }

        private async Task LoadNewReleasesAsync()
        {
            // Collect candidates across Thai BL, Korean Drama, and Cinema
            await LoadThaiBLAsync();
            await LoadKoreanDramaAsync();
            await LoadMoviesAsync();

            // Strictly filter for NEW releases (2024 - 2026) so old titles (Parasite 2019, Goblin 2016) NEVER appear in New Releases!
            AllItems = AllItems
                .Where(i => int.TryParse(i.Year, out int y) && y >= 2024)
                .OrderByDescending(i => i.Year)
                .ThenByDescending(i => i.Rating)
                .ToList();
        }

        private static string? CleanPosterUrl(string? url, string? title = null)
        {
            if (title != null && title.Contains("Kingdom", StringComparison.OrdinalIgnoreCase))
                return "/images/kingdom.jpg";
            if (title != null && title.Contains("Daung", StringComparison.OrdinalIgnoreCase))
                return "/images/daung_with_you.jpg";
            if (title != null && title.Contains("Khem", StringComparison.OrdinalIgnoreCase))
                return "/images/khemjira.jpg";

            if (string.IsNullOrWhiteSpace(url)) return null;
            if (url.Contains("static.tvmaze.com") && url.Contains("1245252"))
                return "/images/kingdom.jpg";
            if (url.Contains("static.tvmaze.com") && url.Contains("1528703"))
                return "/images/daung_with_you.jpg";
            if (url.Contains("static.tvmaze.com") && url.Contains("1454779"))
                return "/images/khemjira.jpg";
            return url;
        }

        private void SeedFallbackForCurrentSlug()
        {
            if (CurrentSlug == "bl-series")
            {
                _ = LoadThaiBLAsync();
            }
            else if (CurrentSlug == "movies")
            {
                _ = LoadMoviesAsync();
            }
            else if (CurrentSlug == "korean-drama")
            {
                _ = LoadKoreanDramaAsync();
            }
            else
            {
                _ = LoadNewReleasesAsync();
            }
        }

        private void AddSeedIfMissing(long id, bool isContent, string title, string poster, string year, string category, double rating, int eps, string desc)
        {
            if (!AllItems.Any(i => i.Id == id && i.IsContent == isContent))
            {
                AllItems.Add(new CategoryItemDto
                {
                    Id = id,
                    IsContent = isContent,
                    Title = title,
                    PosterUrl = CleanPosterUrl(poster, title),
                    Year = year,
                    Category = category,
                    Rating = rating,
                    TotalEpisodes = eps,
                    Description = desc
                });
            }
        }

        protected void OnSearchChanged(ChangeEventArgs e)
        {
            SearchQuery = e.Value?.ToString() ?? "";
            ApplyFilters();
        }

        protected void OnSortChanged(ChangeEventArgs e)
        {
            SortBy = e.Value?.ToString() ?? "year";
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var query = AllItems.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var q = SearchQuery.Trim().ToLower();
                query = query.Where(i => (i.Title != null && i.Title.ToLower().Contains(q)) ||
                                         (i.Description != null && i.Description.ToLower().Contains(q)) ||
                                         (i.Category != null && i.Category.ToLower().Contains(q)));
            }

            if (SortBy == "title")
            {
                query = query.OrderBy(i => i.Title);
            }
            else if (SortBy == "rating")
            {
                query = query.OrderByDescending(i => i.Rating).ThenByDescending(i => i.Year);
            }
            else // "year" (default for New Releases)
            {
                query = query.OrderByDescending(i => i.Year).ThenByDescending(i => i.Rating);
            }

            FilteredItems = query.ToList();
        }

        public async Task ToggleFavorite(CategoryItemDto item)
        {
            var favItem = new FavoriteItem
            {
                Id = item.Id,
                IsContent = item.IsContent,
                Title = item.Title,
                PosterUrl = item.PosterUrl,
                Category = item.Category,
                Description = item.Description,
                Year = item.Year,
                Rating = item.Rating,
                TotalEpisodes = item.TotalEpisodes,
                AddedAt = DateTime.UtcNow
            };

            bool added = await FavoriteService.ToggleFavoriteAsync(favItem);
            ToastMessage = added ? $"Added \"{item.Title}\" to Favorites" : $"Removed \"{item.Title}\" from Favorites";
            StateHasChanged();

            await Task.Delay(2600);
            ToastMessage = null;
            StateHasChanged();
        }

        public async Task ToggleWatchLater(CategoryItemDto item)
        {
            var wlItem = new WatchLaterItem
            {
                Id = item.Id,
                IsContent = item.IsContent,
                Title = item.Title,
                PosterUrl = item.PosterUrl,
                Category = item.Category,
                Description = item.Description,
                TotalEpisodes = item.TotalEpisodes,
                CurrentEpisode = 1,
                Status = "Want to Watch",
                AddedAt = DateTime.UtcNow
            };

            bool added = await WatchLaterService.ToggleWatchLaterAsync(wlItem);
            ToastMessage = added ? $"Added \"{item.Title}\" to Watch Later" : $"Removed \"{item.Title}\" from Watch Later";
            StateHasChanged();

            await Task.Delay(2600);
            ToastMessage = null;
            StateHasChanged();
        }

        private async void HandleFavoritesChanged()
        {
            await RefreshFavoriteCacheAsync();
            await InvokeAsync(StateHasChanged);
        }

        private async void HandleWatchLaterChanged()
        {
            await RefreshWatchLaterCacheAsync();
            await InvokeAsync(StateHasChanged);
        }

        public void NavigateToDetail(CategoryItemDto item)
        {
            Nav.NavigateTo($"/drama/{item.Id}");
        }

        public void NavigateToItem(CategoryItemDto item) => NavigateToDetail(item);

        public void Dispose()
        {
            Nav.LocationChanged -= HandleLocationChanged;
            FavoriteService.OnChange -= HandleFavoritesChanged;
            WatchLaterService.OnChange -= HandleWatchLaterChanged;
            AuthService.OnAuthStateChanged -= HandleAuthChanged;
        }
    }
}
