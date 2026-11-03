using DotNetProject.Balzor.Components.Shared;
using DotNetProject.Domain.Features.Contents;
using DotNetProject.Domain.Features.Movies;
using DotNetProject.Domain.Features.Thai;
using Microsoft.AspNetCore.Components;

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

        protected bool IsLoading { get; set; } = true;
        protected string? ErrorMessage { get; set; }

        protected override async Task OnInitializedAsync()
        {
            IsLoading = true;

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

            // Load Favorites cache
            var favs = await FavoriteService.GetFavoritesAsync();
            FavoriteDramaIds = favs.Where(f => f.IsContent && f.Category != null && f.Category.Contains("Korean", StringComparison.OrdinalIgnoreCase)).Select(f => f.Id).ToHashSet();
            FavoriteThaiIds = favs.Where(f => f.IsContent && f.Category != null && f.Category.Contains("Thai", StringComparison.OrdinalIgnoreCase)).Select(f => f.Id).ToHashSet();
            FavoriteMovieIds = favs.Where(f => !f.IsContent || (f.Category != null && f.Category.Contains("Movie", StringComparison.OrdinalIgnoreCase))).Select(f => f.Id).ToHashSet();

            // Load Continue Watching history & subscribe to reactive changes
            WatchHistoryService.OnChange += HandleHistoryChange;
            AuthService.OnAuthStateChanged += HandleAuthChange;
            await LoadContinueWatchingAsync();

            IsLoading = false;
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

        public async Task ToggleFavoriteDrama(ContentListItemResponse drama)
        {
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

        public async Task ToggleFavoriteThai(ThaiListItemResponse thai)
        {
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

        public async Task ToggleFavoriteMovie(MovieListItemResponse movie)
        {
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

        public void NavigateToDrama(long contentId)
        {
            Nav.NavigateTo($"/drama/{contentId}");
        }

        public Task PlayDramaAsync(long contentId, string title, string? desc, string? posterUrl)
        {
            NavigateToDrama(contentId);
            return Task.CompletedTask;
        }

        public void PlayKinnPorsche()
        {
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

        public void PlayContent(string title, string? desc, string? posterUrl, int ep = 1, string? videoUrl = null)
        {
            long id = 23;
            if (title.Contains("Fool In Love", StringComparison.OrdinalIgnoreCase)) id = 20002;
            else if (title.Contains("Kingdom", StringComparison.OrdinalIgnoreCase)) id = 20003;
            else if (title.Contains("Daung", StringComparison.OrdinalIgnoreCase)) id = 20004;
            else if (title.Contains("Khem", StringComparison.OrdinalIgnoreCase)) id = 20005;
            else if (title.Contains("Bad Buddy", StringComparison.OrdinalIgnoreCase)) id = 6;
            else if (title.Contains("2gether", StringComparison.OrdinalIgnoreCase)) id = 5;
            else if (title.Contains("KinnPorsche", StringComparison.OrdinalIgnoreCase)) id = 7;
            else if (title.Contains("TharnType", StringComparison.OrdinalIgnoreCase)) id = 8;
            else if (title.Contains("SOTUS", StringComparison.OrdinalIgnoreCase)) id = 9;
            else if (title.Contains("Thousand Stars", StringComparison.OrdinalIgnoreCase)) id = 10;
            else if (title.Contains("Crash Landing", StringComparison.OrdinalIgnoreCase)) id = 15;
            else if (title.Contains("Vincenzo", StringComparison.OrdinalIgnoreCase)) id = 21;
            else if (title.Contains("The Glory", StringComparison.OrdinalIgnoreCase)) id = 22;
            else if (title.Contains("Lovely Runner", StringComparison.OrdinalIgnoreCase)) id = 24;
            else if (title.Contains("Alchemy", StringComparison.OrdinalIgnoreCase)) id = 23;

            Nav.NavigateTo($"/drama/{id}?ep={ep}&autoplay=true");
        }

        private void HandleAuthChange()
        {
            InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            WatchHistoryService.OnChange -= HandleHistoryChange;
            AuthService.OnAuthStateChanged -= HandleAuthChange;
        }
    }
}
