using DotNetProject.Balzor.Components.Shared;
using DotNetProject.Domain.Features.Contents;
using DotNetProject.Domain.Features.Movies;
using DotNetProject.Domain.Features.Thai;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetProject.Balzor.Components.Pages.Home
{
    public class HeroSlideItem
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Script { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PosterUrl { get; set; } = string.Empty;
        public string Badge { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsContent { get; set; } = true;
    }

    public class SearchResultItem
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? PosterUrl { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? Year { get; set; }
        public double Rating { get; set; } = 9.0;
        public bool IsContent { get; set; }
    }

    public partial class Index : ComponentBase, IDisposable
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
        private WatchHistoryService WatchHistoryService { get; set; } = default!;

        [Inject]
        public AuthService AuthService { get; set; } = default!;

        protected List<ContentListItemResponse> Dramas { get; set; } = new();
        protected List<MovieListItemResponse> Movies { get; set; } = new();
        protected List<ThaiListItemResponse> Thais { get; set; } = new();
        protected List<WatchHistoryItem> ContinueWatchingItems { get; set; } = new();

        protected HashSet<long> FavoriteDramaIds { get; set; } = new();
        protected HashSet<long> FavoriteThaiIds { get; set; } = new();
        protected HashSet<long> FavoriteMovieIds { get; set; } = new();

        protected HashSet<long> WatchLaterDramaIds { get; set; } = new();
        protected HashSet<long> WatchLaterThaiIds { get; set; } = new();
        protected HashSet<long> WatchLaterMovieIds { get; set; } = new();

        protected bool IsLoading { get; set; } = true;
        protected string? ErrorMessage { get; set; }

        // Live Search State
        protected string SearchQuery { get; set; } = string.Empty;
        protected bool HasSearch => !string.IsNullOrWhiteSpace(SearchQuery);

        // Filter Pills State
        protected string SelectedFilter { get; set; } = "All";

        // Toast Feedback State
        protected string? ToastMessage { get; set; }
        protected bool ShowToast { get; set; }
        private System.Threading.CancellationTokenSource? _toastCts;

        // Hero Carousel
        protected int CurrentSlideIndex { get; set; } = 0;
        protected List<HeroSlideItem> HeroSlides { get; set; } = new()
        {
            new HeroSlideItem
            {
                Id = 7,
                Title = "KinnPorsche",
                Script = "The Series",
                Description = "The second son of a prominent mafia family, Kinn, is assigned an exceptional bodyguard, Porsche. Their bond starts with friction and evolves into an unbreakable romance.",
                PosterUrl = "https://upload.wikimedia.org/wikipedia/en/9/9e/KinnPorsche_series_poster.jpg",
                Badge = "Trending #1 Worldwide",
                Category = "Thai BL",
                IsContent = true
            },
            new HeroSlideItem
            {
                Id = 20003,
                Title = "Kingdom",
                Script = "Crown Prince",
                Description = "While strange rumors grip the Joseon kingdom, the crown prince investigates a mysterious plague that turns victims into undead creatures in this thrilling Korean period epic.",
                PosterUrl = "/images/kingdom.jpg",
                Badge = "Top Rated Series (13 Episodes)",
                Category = "Korean Drama",
                IsContent = true
            },
            new HeroSlideItem
            {
                Id = 10002,
                Title = "Your Name",
                Script = "Kimi no Na wa",
                Description = "Two high school students, Mitsuha and Taki, discover a magical bond that allows them to swap bodies across time, culminating in an emotional, unforgettable journey.",
                PosterUrl = "https://upload.wikimedia.org/wikipedia/en/0/0b/Your_Name_poster.png",
                Badge = "4K Cinematic Masterpiece",
                Category = "Movie",
                IsContent = false
            },
            new HeroSlideItem
            {
                Id = 23,
                Title = "Alchemy of Souls",
                Script = "Fantasy Epic",
                Description = "A powerful sorceress in a blind woman's body encounters a young man from a prestigious family who seeks her help to alter his fate.",
                PosterUrl = "https://upload.wikimedia.org/wikipedia/en/3/36/Alchemy_of_Souls.jpg",
                Badge = "Complete Series (30 Episodes)",
                Category = "Korean Drama",
                IsContent = true
            },
            new HeroSlideItem
            {
                Id = 30001,
                Title = "Parasite",
                Script = "Academy Award Winner",
                Description = "Greed and class disparity threaten the symbiotic relationship between the wealthy Park family and the destitute Kim clan in Bong Joon-ho's historic masterpiece.",
                PosterUrl = "https://upload.wikimedia.org/wikipedia/en/5/53/Parasite_%282019_film%29.png",
                Badge = "Best Picture Winner",
                Category = "Movie",
                IsContent = false
            },
            new HeroSlideItem
            {
                Id = 20002,
                Title = "Fool In Love",
                Script = "The Series",
                Description = "A romantic comedy Thai BL drama exploring tender affection and youthful dilemmas.",
                PosterUrl = "https://upload.wikimedia.org/wikipedia/en/3/3c/Fool_in_love_the_series.jpg",
                Badge = "1080P Full HD (11 Episodes)",
                Category = "Thai BL",
                IsContent = true
            },
            new HeroSlideItem
            {
                Id = 20004,
                Title = "Daung With You",
                Script = "Heartwarming Romance",
                Description = "A heartwarming journey of love, friendship, and self-discovery in a modern university setting.",
                PosterUrl = "/images/daung_with_you.jpg",
                Badge = "Full Season (12 Episodes)",
                Category = "Thai BL",
                IsContent = true
            },
            new HeroSlideItem
            {
                Id = 20005,
                Title = "KhemJira",
                Script = "Supernatural Mystery",
                Description = "A young man cursed by ancient karma seeks protection from a powerful sorcerer, sparking a supernatural and emotional adventure.",
                PosterUrl = "/images/khemjira.jpg",
                Badge = "Complete Season (12 Episodes)",
                Category = "Thai BL",
                IsContent = true
            }
        };

        protected HeroSlideItem CurrentHeroSlide =>
            HeroSlides.Count > 0 ? HeroSlides[CurrentSlideIndex % HeroSlides.Count] : HeroSlides[0];

        protected override async Task OnInitializedAsync()
        {
            IsLoading = true;

            // Subscribe to reactive service changes
            FavoriteService.OnChange += HandleFavoritesChanged;
            WatchLaterService.OnChange += HandleWatchLaterChanged;
            WatchHistoryService.OnChange += HandleHistoryChange;
            AuthService.OnAuthStateChanged += HandleAuthChange;

            // Drama Section
            try
            {
                var request = new GetContentPagedListRequest
                {
                    PageNo = 1,
                    PageSize = 20,
                    SearchTerm = null
                };

                var result = await HttpClientService.GetContentsPagedAsync(request);
                if (result != null && result.IsSuccess && result.Data?.Items != null && result.Data.Items.Any())
                {
                    Dramas = result.Data.Items.Select(d => new ContentListItemResponse(
                        d.ContentId,
                        d.Title,
                        d.Description,
                        CleanPoster(d.PosterUrl, d.Title),
                        d.BannerUrl,
                        d.ReleaseDate,
                        d.Country,
                        d.Language,
                        d.ContentType,
                        d.Status
                    )).ToList();
                }
            }
            catch { }

            // Ensure Kingdom is present and has working poster
            if (!Dramas.Any(d => d.Title.Contains("Kingdom", StringComparison.OrdinalIgnoreCase)))
            {
                Dramas.Insert(0, new ContentListItemResponse(
                    20003,
                    "Kingdom",
                    "While strange rumors grip the kingdom, the crown prince investigates a mysterious plague.",
                    "/images/kingdom.jpg",
                    null,
                    null,
                    "South Korea",
                    "Korean",
                    "Drama",
                    "Ongoing"
                ));
            }

            // Ensure Alchemy of Souls is present
            if (!Dramas.Any(d => d.Title.Contains("Alchemy", StringComparison.OrdinalIgnoreCase)))
            {
                Dramas.Add(new ContentListItemResponse(
                    23,
                    "Alchemy of Souls",
                    "A powerful sorceress in a blind woman's body encounters a man from a prestigious family who wants her help to change his destiny.",
                    "https://upload.wikimedia.org/wikipedia/en/3/36/Alchemy_of_Souls.jpg",
                    null,
                    null,
                    "South Korea",
                    "Korean",
                    "Drama",
                    "Completed"
                ));
            }

            // Movie Section
            try
            {
                var movieRequest = new GetMoviePagedListRequest
                {
                    PageNo = 1,
                    PageSize = 20,
                    SearchTerm = null
                };

                var moviesResult = await HttpClientService.GetMoviesPagedAsync(movieRequest);
                if (moviesResult != null && moviesResult.IsSuccess && moviesResult.Data?.Items != null && moviesResult.Data.Items.Any())
                {
                    Movies = moviesResult.Data.Items.Select(m => new MovieListItemResponse(
                        m.Id,
                        m.Title,
                        m.ReleaseYear,
                        CleanPoster(m.PosterUrl, m.Title)
                    )).ToList();
                }
            }
            catch { }

            // Ensure existing movie posters are preserved
            if (Movies == null || !Movies.Any())
            {
                Movies = new List<MovieListItemResponse>
                {
                    new(10002, "Your Name", 2016, "https://upload.wikimedia.org/wikipedia/en/0/0b/Your_Name_poster.png"),
                    new(10003, "Spirited Away", 2001, "https://upload.wikimedia.org/wikipedia/en/d/db/Spirited_Away_Japanese_poster.png"),
                    new(10004, "Train to Busan", 2016, "https://upload.wikimedia.org/wikipedia/en/9/95/Train_to_Busan.jpg"),
                    new(30001, "Parasite", 2019, "https://upload.wikimedia.org/wikipedia/en/5/53/Parasite_%282019_film%29.png")
                };
            }

            // Thai Section
            try
            {
                var thaiRequest = new GetThaiPagedListRequest
                {
                    PageNo = 1,
                    PageSize = 20,
                    SearchTerm = null,
                };

                var thaiResult = await HttpClientService.GetThaiPagedAsync(thaiRequest);
                if (thaiResult != null && thaiResult.IsSuccess && thaiResult.Data?.Items != null && thaiResult.Data.Items.Any())
                {
                    Thais = thaiResult.Data.Items.Select(t => new ThaiListItemResponse(
                        t.Id,
                        t.Title,
                        t.ReleaseYear,
                        CleanPoster(t.PosterUrl, t.Title)
                    )).ToList();
                }
            }
            catch { }

            // Fallback for Thai BL if empty
            if (Thais == null || !Thais.Any())
            {
                Thais = GetFallbackThaiList();
            }

            // Load user-specific caches
            await RefreshFavoritesCacheAsync();
            await RefreshWatchLaterCacheAsync();
            await LoadContinueWatchingAsync();

            IsLoading = false;
        }

        // Live Search Results
        protected List<SearchResultItem> SearchResults
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SearchQuery)) return new();
                var q = SearchQuery.Trim().ToLowerInvariant();
                var list = new List<SearchResultItem>();

                foreach (var d in Dramas.Where(d => d.Title != null && d.Title.ToLowerInvariant().Contains(q)))
                {
                    list.Add(new SearchResultItem
                    {
                        Id = d.ContentId,
                        Title = d.Title,
                        PosterUrl = d.PosterUrl,
                        Category = "Korean Drama",
                        Year = d.ReleaseDate?.Year.ToString() ?? "2023",
                        Rating = 9.4,
                        IsContent = true
                    });
                }

                foreach (var t in Thais.Where(t => t.Title != null && t.Title.ToLowerInvariant().Contains(q)))
                {
                    list.Add(new SearchResultItem
                    {
                        Id = t.Id,
                        Title = t.Title,
                        PosterUrl = t.PosterUrl,
                        Category = "Thai BL",
                        Year = t.ReleaseYear?.ToString() ?? "2024",
                        Rating = 9.2,
                        IsContent = true
                    });
                }

                foreach (var m in Movies.Where(m => m.Title != null && m.Title.ToLowerInvariant().Contains(q)))
                {
                    list.Add(new SearchResultItem
                    {
                        Id = m.Id,
                        Title = m.Title,
                        PosterUrl = m.PosterUrl,
                        Category = "Movie",
                        Year = m.ReleaseYear?.ToString() ?? "2023",
                        Rating = 9.1,
                        IsContent = false
                    });
                }

                return list;
            }
        }

        protected void ClearSearch()
        {
            SearchQuery = string.Empty;
        }

        protected void SelectFilter(string filter)
        {
            SelectedFilter = filter;
        }

        // Hero Slide Methods
        protected void NextSlide()
        {
            CurrentSlideIndex = (CurrentSlideIndex + 1) % HeroSlides.Count;
        }

        protected void PrevSlide()
        {
            CurrentSlideIndex = (CurrentSlideIndex - 1 + HeroSlides.Count) % HeroSlides.Count;
        }

        protected void GoToSlide(int index)
        {
            CurrentSlideIndex = index;
        }

        public void PlayCurrentHeroSlide()
        {
            var slide = CurrentHeroSlide;
            if (!AuthService.IsAuthenticated())
            {
                Nav.NavigateTo($"/login?returnUrl=/drama/{slide.Id}");
                return;
            }
            if (slide.IsContent)
            {
                NavigateToDrama(slide.Id);
            }
            else
            {
                NavigateToDrama(slide.Id);
            }
        }

        public async Task ToggleCurrentHeroWatchLater()
        {
            var slide = CurrentHeroSlide;
            if (!AuthService.IsAuthenticated())
            {
                Nav.NavigateTo("/login?returnUrl=/");
                return;
            }

            var item = new WatchLaterItem
            {
                Id = slide.Id,
                IsContent = slide.IsContent,
                Title = slide.Title,
                PosterUrl = slide.PosterUrl,
                Category = slide.Category,
                Description = slide.Description,
                TotalEpisodes = 14,
                CurrentEpisode = 1,
                Status = "Want to Watch",
                AddedAt = DateTime.UtcNow
            };

            bool added = await WatchLaterService.ToggleWatchLaterAsync(item);
            if (slide.Category.Contains("Thai", StringComparison.OrdinalIgnoreCase))
            {
                if (added) WatchLaterThaiIds.Add(slide.Id);
                else WatchLaterThaiIds.Remove(slide.Id);
            }
            else if (slide.Category.Contains("Korean", StringComparison.OrdinalIgnoreCase))
            {
                if (added) WatchLaterDramaIds.Add(slide.Id);
                else WatchLaterDramaIds.Remove(slide.Id);
            }
            else
            {
                if (added) WatchLaterMovieIds.Add(slide.Id);
                else WatchLaterMovieIds.Remove(slide.Id);
            }

            TriggerToast(added ? $"✓ '{slide.Title}' added to your Watch Later list!" : $"Removed '{slide.Title}' from Watch Later.");
            StateHasChanged();
        }

        protected bool IsCurrentHeroInWatchLater()
        {
            var slide = CurrentHeroSlide;
            if (slide.Category.Contains("Thai", StringComparison.OrdinalIgnoreCase))
                return WatchLaterThaiIds.Contains(slide.Id);
            if (slide.Category.Contains("Korean", StringComparison.OrdinalIgnoreCase))
                return WatchLaterDramaIds.Contains(slide.Id);
            return WatchLaterMovieIds.Contains(slide.Id);
        }

        // Toast Feedback System
        protected void TriggerToast(string message)
        {
            _toastCts?.Cancel();
            _toastCts = new System.Threading.CancellationTokenSource();
            var token = _toastCts.Token;

            ToastMessage = message;
            ShowToast = true;
            StateHasChanged();

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(3200, token);
                    if (!token.IsCancellationRequested)
                    {
                        ShowToast = false;
                        await InvokeAsync(StateHasChanged);
                    }
                }
                catch { }
            }, token);
        }

        private async Task RefreshFavoritesCacheAsync()
        {
            var favs = await FavoriteService.GetFavoritesAsync();
            FavoriteDramaIds = favs.Where(f => f.IsContent && f.Category != null && f.Category.Contains("Korean", StringComparison.OrdinalIgnoreCase)).Select(f => f.Id).ToHashSet();
            FavoriteThaiIds = favs.Where(f => f.IsContent && f.Category != null && f.Category.Contains("Thai", StringComparison.OrdinalIgnoreCase)).Select(f => f.Id).ToHashSet();
            FavoriteMovieIds = favs.Where(f => !f.IsContent || (f.Category != null && f.Category.Contains("Movie", StringComparison.OrdinalIgnoreCase))).Select(f => f.Id).ToHashSet();
        }

        private async Task RefreshWatchLaterCacheAsync()
        {
            var items = await WatchLaterService.GetItemsAsync();
            WatchLaterDramaIds = items.Where(w => w.IsContent && w.Category != null && w.Category.Contains("Korean", StringComparison.OrdinalIgnoreCase)).Select(w => w.Id).ToHashSet();
            WatchLaterThaiIds = items.Where(w => w.IsContent && w.Category != null && w.Category.Contains("Thai", StringComparison.OrdinalIgnoreCase)).Select(w => w.Id).ToHashSet();
            WatchLaterMovieIds = items.Where(w => !w.IsContent || (w.Category != null && w.Category.Contains("Movie", StringComparison.OrdinalIgnoreCase))).Select(w => w.Id).ToHashSet();
        }

        private static string? CleanPoster(string? url, string? title)
        {
            if (title != null)
            {
                if (title.Contains("Kingdom", StringComparison.OrdinalIgnoreCase))
                    return "/images/kingdom.jpg";
                if (title.Contains("Daung", StringComparison.OrdinalIgnoreCase))
                    return "/images/daung_with_you.jpg";
                if (title.Contains("Khem", StringComparison.OrdinalIgnoreCase))
                    return "/images/khemjira.jpg";
                if (title.Contains("Bad Genius", StringComparison.OrdinalIgnoreCase))
                    return "/images/bad_genius.jpg";
                if (title.Contains("Along with the Gods", StringComparison.OrdinalIgnoreCase))
                    return "/images/along_with_the_gods.jpg";
                if (title.Contains("The Childe", StringComparison.OrdinalIgnoreCase))
                    return "/images/the_childe.jpg";
                if (title.Contains("Decibel", StringComparison.OrdinalIgnoreCase))
                    return "/images/decibel.jpg";
            }

            if (string.IsNullOrWhiteSpace(url)) return "/images/poster_fallback.svg";
            if (url.Contains("static.tvmaze.com") && url.Contains("1245252"))
                return "/images/kingdom.jpg";
            if (url.Contains("static.tvmaze.com") && url.Contains("1528703"))
                return "/images/daung_with_you.jpg";
            if (url.Contains("static.tvmaze.com") && url.Contains("1454779"))
                return "/images/khemjira.jpg";
            return url;
        }

        private static List<ThaiListItemResponse> GetFallbackThaiList()
        {
            return new List<ThaiListItemResponse>
            {
                new(20002, "Fool In Love", 2026, "https://upload.wikimedia.org/wikipedia/en/3/3c/Fool_in_love_the_series.jpg"),
                new(20004, "Daung With You", 2026, "/images/daung_with_you.jpg"),
                new(20005, "KhemJira", 2025, "/images/khemjira.jpg"),
                new(6, "Bad Buddy", 2021, "https://upload.wikimedia.org/wikipedia/en/b/b7/Bad_Buddy_Series_2021_poster.jpg"),
                new(7, "KinnPorsche", 2022, "https://upload.wikimedia.org/wikipedia/en/9/9e/KinnPorsche_series_poster.jpg"),
                new(5, "2gether: The Series", 2020, "https://upload.wikimedia.org/wikipedia/en/6/68/2gether_The_Series_2020_poster.jpg"),
                new(8, "TharnType: The Series", 2019, "https://upload.wikimedia.org/wikipedia/en/7/73/TharnType_The_Series_2019_poster.jpg"),
                new(10, "A Tale of Thousand Stars", 2021, "https://upload.wikimedia.org/wikipedia/en/0/0e/A_Tale_of_Thousand_Stars_2021_poster.jpg"),
                new(11, "I Told Sunset About You", 2020, "https://upload.wikimedia.org/wikipedia/en/a/ab/I_Told_Sunset_About_You_poster.jpg"),
                new(12, "My School President", 2022, "https://upload.wikimedia.org/wikipedia/en/2/2b/My_School_President_GMMTV_Poster.png")
            };
        }

        private async Task LoadContinueWatchingAsync()
        {
            ContinueWatchingItems = await WatchHistoryService.GetHistoryAsync(5);
        }

        private async void HandleHistoryChange()
        {
            await LoadContinueWatchingAsync();
            await InvokeAsync(StateHasChanged);
        }

        private async void HandleFavoritesChanged()
        {
            await RefreshFavoritesCacheAsync();
            await InvokeAsync(StateHasChanged);
        }

        private async void HandleWatchLaterChanged()
        {
            await RefreshWatchLaterCacheAsync();
            await InvokeAsync(StateHasChanged);
        }

        private async void HandleAuthChange()
        {
            await RefreshFavoritesCacheAsync();
            await RefreshWatchLaterCacheAsync();
            await LoadContinueWatchingAsync();
            await InvokeAsync(StateHasChanged);
        }

        public async Task ToggleFavoriteDrama(ContentListItemResponse drama)
        {
            if (!AuthService.IsAuthenticated())
            {
                Nav.NavigateTo("/login?returnUrl=/");
                return;
            }
            var item = new FavoriteItem
            {
                Id = drama.ContentId,
                IsContent = true,
                Title = drama.Title,
                PosterUrl = CleanPoster(drama.PosterUrl, drama.Title),
                Category = "Korean Drama",
                Description = drama.Description,
                Year = drama.ReleaseDate?.Year.ToString() ?? "2023",
                Rating = 9.4,
                TotalEpisodes = 16
            };
            bool added = await FavoriteService.ToggleFavoriteAsync(item);
            if (added) FavoriteDramaIds.Add(drama.ContentId);
            else FavoriteDramaIds.Remove(drama.ContentId);
            TriggerToast(added ? $"✓ '{drama.Title}' added to Favorites!" : $"Removed '{drama.Title}' from Favorites.");
            StateHasChanged();
        }

        public async Task ToggleWatchLaterDrama(ContentListItemResponse drama)
        {
            if (!AuthService.IsAuthenticated())
            {
                Nav.NavigateTo("/login?returnUrl=/");
                return;
            }
            var item = new WatchLaterItem
            {
                Id = drama.ContentId,
                IsContent = true,
                Title = drama.Title,
                PosterUrl = CleanPoster(drama.PosterUrl, drama.Title),
                Category = "Korean Drama",
                Description = drama.Description,
                TotalEpisodes = 16,
                CurrentEpisode = 1,
                Status = "Want to Watch",
                AddedAt = DateTime.UtcNow
            };
            bool added = await WatchLaterService.ToggleWatchLaterAsync(item);
            if (added) WatchLaterDramaIds.Add(drama.ContentId);
            else WatchLaterDramaIds.Remove(drama.ContentId);
            TriggerToast(added ? $"✓ '{drama.Title}' saved to Watch Later!" : $"Removed '{drama.Title}' from Watch Later.");
            StateHasChanged();
        }

        public async Task ToggleFavoriteThai(ThaiListItemResponse thai)
        {
            if (!AuthService.IsAuthenticated())
            {
                Nav.NavigateTo("/login?returnUrl=/");
                return;
            }
            var item = new FavoriteItem
            {
                Id = thai.Id,
                IsContent = true,
                Title = thai.Title,
                PosterUrl = CleanPoster(thai.PosterUrl, thai.Title),
                Category = "Thai BL",
                Description = $"{thai.Title} is an emotional Thai romance drama.",
                Year = thai.ReleaseYear?.ToString() ?? "2024",
                Rating = 9.2,
                TotalEpisodes = 12
            };
            bool added = await FavoriteService.ToggleFavoriteAsync(item);
            if (added) FavoriteThaiIds.Add(thai.Id);
            else FavoriteThaiIds.Remove(thai.Id);
            TriggerToast(added ? $"✓ '{thai.Title}' added to Favorites!" : $"Removed '{thai.Title}' from Favorites.");
            StateHasChanged();
        }

        public async Task ToggleWatchLaterThai(ThaiListItemResponse thai)
        {
            if (!AuthService.IsAuthenticated())
            {
                Nav.NavigateTo("/login?returnUrl=/");
                return;
            }
            var item = new WatchLaterItem
            {
                Id = thai.Id,
                IsContent = true,
                Title = thai.Title,
                PosterUrl = CleanPoster(thai.PosterUrl, thai.Title),
                Category = "Thai BL",
                Description = $"{thai.Title} is an emotional Thai romance drama.",
                TotalEpisodes = 12,
                CurrentEpisode = 1,
                Status = "Want to Watch",
                AddedAt = DateTime.UtcNow
            };
            bool added = await WatchLaterService.ToggleWatchLaterAsync(item);
            if (added) WatchLaterThaiIds.Add(thai.Id);
            else WatchLaterThaiIds.Remove(thai.Id);
            TriggerToast(added ? $"✓ '{thai.Title}' saved to Watch Later!" : $"Removed '{thai.Title}' from Watch Later.");
            StateHasChanged();
        }

        public async Task ToggleFavoriteMovie(MovieListItemResponse movie)
        {
            if (!AuthService.IsAuthenticated())
            {
                Nav.NavigateTo("/login?returnUrl=/");
                return;
            }
            var item = new FavoriteItem
            {
                Id = movie.Id,
                IsContent = false,
                Title = movie.Title,
                PosterUrl = CleanPoster(movie.PosterUrl, movie.Title),
                Category = "Movie",
                Description = $"{movie.Title} is an exciting cinematic feature film.",
                Year = movie.ReleaseYear?.ToString() ?? "2023",
                Rating = 9.1,
                TotalEpisodes = 1
            };
            bool added = await FavoriteService.ToggleFavoriteAsync(item);
            if (added) FavoriteMovieIds.Add(movie.Id);
            else FavoriteMovieIds.Remove(movie.Id);
            TriggerToast(added ? $"✓ '{movie.Title}' added to Favorites!" : $"Removed '{movie.Title}' from Favorites.");
            StateHasChanged();
        }

        public async Task ToggleWatchLaterMovie(MovieListItemResponse movie)
        {
            if (!AuthService.IsAuthenticated())
            {
                Nav.NavigateTo("/login?returnUrl=/");
                return;
            }
            var item = new WatchLaterItem
            {
                Id = movie.Id,
                IsContent = false,
                Title = movie.Title,
                PosterUrl = CleanPoster(movie.PosterUrl, movie.Title),
                Category = "Movie",
                Description = $"{movie.Title} is an exciting cinematic feature film.",
                TotalEpisodes = 1,
                CurrentEpisode = 1,
                Status = "Want to Watch",
                AddedAt = DateTime.UtcNow
            };
            bool added = await WatchLaterService.ToggleWatchLaterAsync(item);
            if (added) WatchLaterMovieIds.Add(movie.Id);
            else WatchLaterMovieIds.Remove(movie.Id);
            TriggerToast(added ? $"✓ '{movie.Title}' saved to Watch Later!" : $"Removed '{movie.Title}' from Watch Later.");
            StateHasChanged();
        }

        public void NavigateToDrama(long contentId)
        {
            Nav.NavigateTo($"/drama/{contentId}");
        }

        public Task PlayDramaAsync(long contentId, string title, string? desc, string? posterUrl)
        {
            if (!AuthService.IsAuthenticated())
            {
                Nav.NavigateTo($"/login?returnUrl=/drama/{contentId}");
                return Task.CompletedTask;
            }
            NavigateToDrama(contentId);
            return Task.CompletedTask;
        }

        public void PlayKinnPorsche()
        {
            if (!AuthService.IsAuthenticated())
            {
                Nav.NavigateTo("/login?returnUrl=/drama/7");
                return;
            }
            NavigateToDrama(7); // KinnPorsche
        }

        public void ResumePlayback(WatchHistoryItem item)
        {
            Nav.NavigateTo($"/drama/{item.ContentId}?ep={item.EpisodeNumber}&autoplay=true");
        }

        public void Dispose()
        {
            _toastCts?.Cancel();
            _toastCts?.Dispose();
            FavoriteService.OnChange -= HandleFavoritesChanged;
            WatchLaterService.OnChange -= HandleWatchLaterChanged;
            WatchHistoryService.OnChange -= HandleHistoryChange;
            AuthService.OnAuthStateChanged -= HandleAuthChange;
        }
    }
}
