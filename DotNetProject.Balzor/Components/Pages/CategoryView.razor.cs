using DotNetProject.Balzor.Components.Shared;
using DotNetProject.Domain.Features.Contents;
using DotNetProject.Domain.Features.Movies;
using DotNetProject.Domain.Features.Thai;
using Microsoft.AspNetCore.Components;
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

        [Parameter]
        public string? CategorySlug { get; set; }

        protected string CurrentSlug { get; set; } = "new-releases";
        protected string CategoryTitle { get; set; } = "New Releases";
        protected string CategorySubtitle { get; set; } = "Explore our latest collection of hit dramas, movies, and series.";
        protected string CategoryBadge { get; set; } = "Curated Releases";

        protected List<CategoryItemDto> AllItems { get; set; } = new();
        protected List<CategoryItemDto> FilteredItems { get; set; } = new();

        protected HashSet<long> FavoriteIds { get; set; } = new();
        protected string SearchQuery { get; set; } = "";
        protected string SortBy { get; set; } = "rating";
        protected bool IsLoading { get; set; } = true;
        protected string? ToastMessage { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            var uri = Nav.Uri.ToLower();
            if (uri.Contains("bl-series") || uri.Contains("thai-bl"))
            {
                CurrentSlug = "bl-series";
            }
            else if (uri.Contains("korean-drama") || uri.Contains("k-drama"))
            {
                CurrentSlug = "korean-drama";
            }
            else if (uri.Contains("movies") || uri.Contains("movie"))
            {
                CurrentSlug = "movies";
            }
            else if (uri.Contains("new-releases"))
            {
                CurrentSlug = "new-releases";
            }
            else if (!string.IsNullOrWhiteSpace(CategorySlug))
            {
                CurrentSlug = CategorySlug.Trim().ToLower();
            }
            else
            {
                CurrentSlug = "new-releases";
            }

            ConfigureCategoryMeta();
            await LoadCategoryDataAsync();
        }

        protected override async Task OnInitializedAsync()
        {
            FavoriteService.OnChange += HandleFavoritesChanged;
            await RefreshFavoriteCacheAsync();
        }

        private async Task RefreshFavoriteCacheAsync()
        {
            var favs = await FavoriteService.GetFavoritesAsync();
            FavoriteIds = favs.Select(f => f.Id).ToHashSet();
        }

        private void ConfigureCategoryMeta()
        {
            switch (CurrentSlug)
            {
                case "bl-series":
                case "thai-bl":
                    CategoryTitle = "Thai BL Series";
                    CategorySubtitle = "Discover the most beloved Thai Boys' Love dramas, romantic journeys, and heart-fluttering romances.";
                    CategoryBadge = "BL Series • Curated Collection";
                    break;

                case "korean-drama":
                case "k-drama":
                    CategoryTitle = "Korean Dramas";
                    CategorySubtitle = "Immerse yourself in gripping storylines, unforgettable romantic comedies, and thrilling K-drama masterpieces.";
                    CategoryBadge = "KR Korean Drama • All Episodes";
                    break;

                case "movies":
                case "movie":
                    CategoryTitle = "Movies & Cinema";
                    CategorySubtitle = "Stream award-winning cinema, blockbuster thrillers, action hits, and celebrated Asian feature films.";
                    CategoryBadge = "Feature Films • HD Quality";
                    break;

                case "new-releases":
                default:
                    CurrentSlug = "new-releases";
                    CategoryTitle = "New Releases";
                    CategorySubtitle = "Freshly released episodes, trending series, and the latest additions to NangWorld.";
                    CategoryBadge = "Latest & Trending 2024 - 2026";
                    break;
            }
        }

        protected async Task SetCategory(string slug)
        {
            Nav.NavigateTo($"category/{slug}");
            await Task.CompletedTask;
        }

        private async Task LoadCategoryDataAsync()
        {
            IsLoading = true;
            AllItems.Clear();

            try
            {
                if (CurrentSlug == "bl-series" || CurrentSlug == "thai-bl")
                {
                    await LoadThaiBLAsync();
                }
                else if (CurrentSlug == "korean-drama" || CurrentSlug == "k-drama")
                {
                    await LoadKoreanDramaAsync();
                }
                else if (CurrentSlug == "movies" || CurrentSlug == "movie")
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
                // Fallbacks guarantee high-quality content even on network hiccups
                SeedFallbackForCurrentSlug();
            }

            ApplyFilters();
            IsLoading = false;
        }

        private async Task LoadThaiBLAsync()
        {
            var res = await HttpClientService.GetThaiPagedAsync(new GetThaiPagedListRequest { PageNo = 1, PageSize = 30 });
            if (res != null && res.IsSuccess && res.Data?.Items != null && res.Data.Items.Any())
            {
                foreach (var t in res.Data.Items)
                {
                    AllItems.Add(new CategoryItemDto
                    {
                        Id = t.Id,
                        IsContent = true,
                        Title = t.Title,
                        PosterUrl = t.PosterUrl,
                        Year = t.ReleaseYear?.ToString() ?? "2024",
                        Category = "Thai BL",
                        Rating = 9.2,
                        TotalEpisodes = 12,
                        Description = $"{t.Title} is a popular Thai romance drama full of emotion and charming chemistry."
                    });
                }
            }

            // Ensure our top Thai BL series exist in the list
            AddSeedIfMissing(20002, true, "Fool In Love", "https://upload.wikimedia.org/wikipedia/en/3/3c/Fool_in_love_the_series.jpg", "2026", "Thai BL", 9.3, 6, "The confident heir of Araya Group meets an intelligent young successor with a boxer past.");
            AddSeedIfMissing(7, true, "KinnPorsche", "https://upload.wikimedia.org/wikipedia/en/9/9e/KinnPorsche_series_poster.jpg", "2022", "Thai BL", 9.5, 14, "The second son of a major mafia family is assigned an extraordinary bodyguard.");
            AddSeedIfMissing(6, true, "Bad Buddy", "https://upload.wikimedia.org/wikipedia/en/b/b7/Bad_Buddy_Series_2021_poster.jpg", "2021", "Thai BL", 9.2, 12, "Two rival families live next door, but their sons secretly develop deep feelings.");
            AddSeedIfMissing(20004, true, "Daung With You", "https://static.tvmaze.com/uploads/images/original_untouched/611/1528703.jpg", "2026", "Thai BL", 9.0, 12, "A cheerful Decorative Arts student sparks a sweet campus romance with a jazz musician.");
            AddSeedIfMissing(20005, true, "KhemJira", "https://static.tvmaze.com/uploads/images/original_untouched/581/1454779.jpg", "2025", "Thai BL", 9.2, 12, "Born under a curse, Khemjira journeys to Isan to seek salvation from a powerful shaman.");
            AddSeedIfMissing(5, true, "2gether: The Series", "https://upload.wikimedia.org/wikipedia/en/2/23/2gether_The_Series_poster.jpg", "2020", "Thai BL", 9.1, 13, "A college student asks an attractive guitarist to be his fake boyfriend, leading to unexpected love.");
            AddSeedIfMissing(8, true, "TharnType: The Series", "https://upload.wikimedia.org/wikipedia/en/6/69/TharnType_The_Series_poster.jpg", "2019", "Thai BL", 8.9, 12, "Two college freshmen rooming together gradually overcome deep prejudices.");
            AddSeedIfMissing(10, true, "A Tale of Thousand Stars", "https://upload.wikimedia.org/wikipedia/en/a/a2/1000stars_poster.jpg", "2021", "Thai BL", 9.4, 10, "A young man receives a heart transplant and travels to a cliffside village to fulfill a promise.");
        }

        private async Task LoadKoreanDramaAsync()
        {
            var res = await HttpClientService.GetContentsPagedAsync(new GetContentPagedListRequest { PageNo = 1, PageSize = 30 });
            if (res != null && res.IsSuccess && res.Data?.Items != null && res.Data.Items.Any())
            {
                foreach (var d in res.Data.Items)
                {
                    AllItems.Add(new CategoryItemDto
                    {
                        Id = d.ContentId,
                        IsContent = true,
                        Title = d.Title,
                        PosterUrl = d.PosterUrl,
                        Year = d.ReleaseDate?.Year.ToString() ?? "2023",
                        Category = "Korean Drama",
                        Rating = 9.3,
                        TotalEpisodes = 16,
                        Description = d.Description ?? $"{d.Title} is an acclaimed Korean drama series."
                    });
                }
            }

            // Top seeds
            AddSeedIfMissing(23, true, "Alchemy of Souls", "https://upload.wikimedia.org/wikipedia/en/b/b5/Alchemy_of_Souls.jpg", "2022", "Korean Drama", 9.4, 30, "A powerful sorceress whose soul is trapped in another body becomes involved with a young nobleman.");
            AddSeedIfMissing(20003, true, "Kingdom", "https://static.tvmaze.com/uploads/images/original_untouched/498/1245252.jpg", "2019", "Korean Drama", 9.1, 12, "While strange rumors grip the kingdom, the crown prince investigates a mysterious plague.");
            AddSeedIfMissing(15, true, "Crash Landing on You", "https://upload.wikimedia.org/wikipedia/en/5/5e/Crash_Landing_on_You_poster.png", "2019", "Korean Drama", 9.6, 16, "A paragliding mishap drops a South Korean heiress into North Korea into the life of an army officer.");
            AddSeedIfMissing(24, true, "Lovely Runner", "https://upload.wikimedia.org/wikipedia/en/6/67/Lovely_Runner.png", "2024", "Korean Drama", 9.4, 16, "A passionate fan travels back in time to save her favorite top idol from a tragic fate.");
            AddSeedIfMissing(21, true, "Vincenzo", "https://upload.wikimedia.org/wikipedia/en/5/5b/Vincenzo_TV_series.jpg", "2021", "Korean Drama", 9.3, 20, "A Korean-Italian mafia lawyer gives an untouchable conglomerate a taste of its own medicine.");
            AddSeedIfMissing(22, true, "The Glory", "https://upload.wikimedia.org/wikipedia/en/7/79/The_Glory_TV_series.jpg", "2022", "Korean Drama", 9.3, 16, "Years after surviving horrific high school bullying, a woman puts an elaborate revenge scheme into motion.");
            AddSeedIfMissing(20006, true, "Goblin: The Lonely and Great God", "https://upload.wikimedia.org/wikipedia/en/6/67/Guardian_The_Lonely_and_Great_God_poster.jpg", "2016", "Korean Drama", 9.5, 16, "An immortal goblin yearns to end his cursed immortality and seeks his mortal bride.");
            AddSeedIfMissing(20007, true, "Itaewon Class", "https://upload.wikimedia.org/wikipedia/en/0/02/Itaewon_Class.png", "2020", "Korean Drama", 9.1, 16, "An ex-con and his ambitious crew open a small street bar in Itaewon to defeat a rival food giant.");
        }

        private async Task LoadMoviesAsync()
        {
            var res = await HttpClientService.GetMoviesPagedAsync(new GetMoviePagedListRequest { PageNo = 1, PageSize = 30 });
            if (res != null && res.IsSuccess && res.Data?.Items != null && res.Data.Items.Any())
            {
                foreach (var m in res.Data.Items)
                {
                    AllItems.Add(new CategoryItemDto
                    {
                        Id = m.Id,
                        IsContent = false,
                        Title = m.Title,
                        PosterUrl = m.PosterUrl,
                        Year = m.ReleaseYear?.ToString() ?? "2024",
                        Category = "Movie",
                        Rating = 9.0,
                        TotalEpisodes = 1,
                        Description = $"{m.Title} is an exciting cinematic feature film."
                    });
                }
            }

            // High rated movie seeds
            AddSeedIfMissing(30001, false, "Parasite", "https://upload.wikimedia.org/wikipedia/en/5/53/Parasite_%282019_film%29.png", "2019", "Movie", 9.6, 1, "Greed and class discrimination threaten the newly formed symbiotic relationship between two families.");
            AddSeedIfMissing(30002, false, "Train to Busan", "https://upload.wikimedia.org/wikipedia/en/9/95/Train_to_Busan.jpg", "2016", "Movie", 9.4, 1, "While a zombie virus breaks out in South Korea, passengers struggle to survive on a high-speed train.");
            AddSeedIfMissing(30003, false, "The Childe", "https://upload.wikimedia.org/wikipedia/en/9/90/The_Childe.jpg", "2023", "Movie", 9.0, 1, "A young boxer of Korean-Filipino descent is relentlessly pursued by various underground forces.");
            AddSeedIfMissing(30004, false, "Bad Genius", "https://upload.wikimedia.org/wikipedia/en/0/0e/Bad_Genius_poster.jpg", "2017", "Movie", 9.2, 1, "A straight-A student invents an intricate exam-cheating scheme that escalates to international levels.");
            AddSeedIfMissing(30005, false, "Along with the Gods: The Two Worlds", "https://upload.wikimedia.org/wikipedia/en/b/b5/Along_With_the_Gods_The_Two_Worlds.jpg", "2017", "Movie", 9.1, 1, "A firefighter passes away unexpectedly and is escorted through seven trials in the afterlife.");
            AddSeedIfMissing(30006, false, "Decibel", "https://upload.wikimedia.org/wikipedia/en/5/52/Decibel_film_poster.jpg", "2022", "Movie", 8.9, 1, "A former naval submarine commander races to stop a sound-activated explosive terrorism plot in the city.");
        }

        private async Task LoadNewReleasesAsync()
        {
            // Combines latest from Korean, Thai BL, and Cinema
            await LoadThaiBLAsync();
            await LoadKoreanDramaAsync();
            await LoadMoviesAsync();

            // Filter for recent releases
            AllItems = AllItems.OrderByDescending(i => i.Year).ThenByDescending(i => i.Rating).ToList();
        }

        private void SeedFallbackForCurrentSlug()
        {
            if (CurrentSlug == "bl-series" || CurrentSlug == "thai-bl")
            {
                _ = LoadThaiBLAsync();
            }
            else if (CurrentSlug == "movies" || CurrentSlug == "movie")
            {
                _ = LoadMoviesAsync();
            }
            else
            {
                _ = LoadKoreanDramaAsync();
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
                    PosterUrl = poster,
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
            SortBy = e.Value?.ToString() ?? "rating";
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
            else if (SortBy == "year")
            {
                query = query.OrderByDescending(i => i.Year).ThenByDescending(i => i.Rating);
            }
            else
            {
                query = query.OrderByDescending(i => i.Rating).ThenByDescending(i => i.Year);
            }

            FilteredItems = query.ToList();
        }

        protected void NavigateToItem(CategoryItemDto item)
        {
            if (item.IsContent)
            {
                Nav.NavigateTo($"/drama/{item.Id}");
            }
            else
            {
                Nav.NavigateTo($"/drama/{item.Id}");
            }
        }

        protected async Task ToggleFavorite(CategoryItemDto item)
        {
            var fav = new FavoriteItem
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

            bool added = await FavoriteService.ToggleFavoriteAsync(fav);
            if (added)
            {
                FavoriteIds.Add(item.Id);
                ShowToast($"Added \"{item.Title}\" to Favorites.");
            }
            else
            {
                FavoriteIds.Remove(item.Id);
                ShowToast($"Removed \"{item.Title}\" from Favorites.");
            }
            StateHasChanged();
        }

        private async void HandleFavoritesChanged()
        {
            await RefreshFavoriteCacheAsync();
            await InvokeAsync(StateHasChanged);
        }

        private void ShowToast(string message)
        {
            ToastMessage = message;
            _ = Task.Run(async () =>
            {
                await Task.Delay(2800);
                ToastMessage = null;
                await InvokeAsync(StateHasChanged);
            });
        }

        public void Dispose()
        {
            FavoriteService.OnChange -= HandleFavoritesChanged;
        }
    }
}
