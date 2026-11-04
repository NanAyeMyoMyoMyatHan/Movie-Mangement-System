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

            // Fallback for Movies if empty
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
                Rating = 9.2,
                TotalEpisodes = 16
            };
            bool added = await FavoriteService.ToggleFavoriteAsync(item);
            if (added) FavoriteDramaIds.Add(drama.ContentId);
            else FavoriteDramaIds.Remove(drama.ContentId);
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
                Rating = 9.0,
                TotalEpisodes = 1
            };
            bool added = await FavoriteService.ToggleFavoriteAsync(item);
            if (added) FavoriteMovieIds.Add(movie.Id);
            else FavoriteMovieIds.Remove(movie.Id);
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
            StateHasChanged();
        }

        public async Task ToggleHeroWatchLater()
        {
            if (!AuthService.IsAuthenticated())
            {
                Nav.NavigateTo("/login?returnUrl=/");
                return;
            }
            long kinnPorscheId = 7;
            var item = new WatchLaterItem
            {
                Id = kinnPorscheId,
                IsContent = true,
                Title = "KinnPorsche",
                PosterUrl = "https://upload.wikimedia.org/wikipedia/en/9/9e/KinnPorsche_series_poster.jpg",
                Category = "Thai BL",
                Description = "The second son of a mafia family, Kinn, is assigned a bodyguard, Porsche.",
                TotalEpisodes = 14,
                CurrentEpisode = 1,
                Status = "Want to Watch",
                AddedAt = DateTime.UtcNow
            };
            bool added = await WatchLaterService.ToggleWatchLaterAsync(item);
            if (added) WatchLaterThaiIds.Add(kinnPorscheId);
            else WatchLaterThaiIds.Remove(kinnPorscheId);
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
            if (item.IsContent)
            {
                Nav.NavigateTo($"/drama/{item.ContentId}?ep={item.EpisodeNumber}&autoplay=true");
            }
            else
            {
                Nav.NavigateTo($"/movie/{item.ContentId}");
            }
        }

        public void Dispose()
        {
            FavoriteService.OnChange -= HandleFavoritesChanged;
            WatchLaterService.OnChange -= HandleWatchLaterChanged;
            WatchHistoryService.OnChange -= HandleHistoryChange;
            AuthService.OnAuthStateChanged -= HandleAuthChange;
        }
    }
}
