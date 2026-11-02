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
            var request = new GetContentPagedListRequest
            {
                PageNo = 1,
                PageSize = 20,
                SearchTerm = null
            };

            var result = await HttpClientService.GetContentsPagedAsync(request);

            if (result != null && result.IsSuccess && result.Data != null)
            {
                Dramas = result.Data.Items?.ToList() ?? new();
            }
            else
            {
                ErrorMessage = result?.Message ?? "Failed to load contents.";
            }

            // Movie Section
            var movieRequest = new GetMoviePagedListRequest
            {
                PageNo = 1,
                PageSize = 20,
                SearchTerm = null
            };

            var moviesResult = await HttpClientService.GetMoviesPagedAsync(movieRequest);
            if (moviesResult != null && moviesResult.IsSuccess && moviesResult.Data != null)
            {
                Movies = moviesResult.Data.Items?.ToList() ?? new();
            }
            else
            {
                ErrorMessage = moviesResult?.Message ?? "Failed to load contents.";
            }

            // Thai Section
            var thaiRequest = new GetThaiPagedListRequest
            {
                PageNo = 1,
                PageSize = 20,
                SearchTerm = null,
            };

            var thaiResult = await HttpClientService.GetThaiPagedAsync(thaiRequest);
            if (thaiResult != null && thaiResult.IsSuccess && thaiResult.Data != null)
            {
                Thais = thaiResult.Data.Items?.ToList() ?? new();
            }
            else
            {
                ErrorMessage = thaiResult?.Message ?? "Failed to load Thai contents.";
            }

            // Load Favorites cache
            var favs = await FavoriteService.GetFavoritesAsync();
            FavoriteDramaIds = favs.Where(f => f.IsContent && f.Category != null && f.Category.Contains("Korean", StringComparison.OrdinalIgnoreCase)).Select(f => f.Id).ToHashSet();
            FavoriteThaiIds = favs.Where(f => f.IsContent && f.Category != null && f.Category.Contains("Thai", StringComparison.OrdinalIgnoreCase)).Select(f => f.Id).ToHashSet();
            FavoriteMovieIds = favs.Where(f => !f.IsContent || (f.Category != null && f.Category.Contains("Movie", StringComparison.OrdinalIgnoreCase))).Select(f => f.Id).ToHashSet();

            // Load Continue Watching history & subscribe to reactive changes
            WatchHistoryService.OnChange += HandleHistoryChange;
            await LoadContinueWatchingAsync();

            IsLoading = false;
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
                PosterUrl = drama.PosterUrl,
                Category = "Korean Drama",
                Description = drama.Description,
                Year = "2023",
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
                PosterUrl = thai.PosterUrl,
                Category = "Thai BL",
                Description = thai.Title,
                Year = thai.ReleaseYear?.ToString() ?? "2024",
                Rating = 9.1,
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
                PosterUrl = movie.PosterUrl,
                Category = "Movie",
                Description = movie.Title,
                Year = movie.ReleaseYear?.ToString() ?? "2024",
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

        public void Dispose()
        {
            WatchHistoryService.OnChange -= HandleHistoryChange;
        }
    }
}
