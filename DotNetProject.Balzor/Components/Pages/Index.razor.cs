using DotNetProject.Balzor.Components.Shared;
using DotNetProject.Domain.Features.Contents;
using DotNetProject.Domain.Features.Movies;
using DotNetProject.Domain.Features.Thai;
using Microsoft.AspNetCore.Components;

namespace DotNetProject.Balzor.Components.Pages
{
    public partial class Index
    {
        [Inject]
        private HttpClientService HttpClientService { get; set; } = default!;

        [Inject]
        private NavigationManager Nav { get; set; } = default!;

        protected List<ContentListItemResponse> Dramas { get; set; } = new();
        protected List<MovieListItemResponse> Movies { get; set; } = new();

        protected List<ThaiListItemResponse> Thais { get; set; } = new();
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

            IsLoading = false;
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

        public void PlayContent(string title, string? desc, string? posterUrl, int ep = 1, string? videoUrl = null)
        {
            if (title.Contains("Fool In Love", StringComparison.OrdinalIgnoreCase)) NavigateToDrama(20002);
            else if (title.Contains("Kingdom", StringComparison.OrdinalIgnoreCase)) NavigateToDrama(20003);
            else if (title.Contains("Daung", StringComparison.OrdinalIgnoreCase)) NavigateToDrama(20004);
            else if (title.Contains("Khem", StringComparison.OrdinalIgnoreCase)) NavigateToDrama(20005);
            else if (title.Contains("Bad Buddy", StringComparison.OrdinalIgnoreCase)) NavigateToDrama(6);
            else if (title.Contains("2gether", StringComparison.OrdinalIgnoreCase)) NavigateToDrama(5);
            else if (title.Contains("KinnPorsche", StringComparison.OrdinalIgnoreCase)) NavigateToDrama(7);
            else if (title.Contains("TharnType", StringComparison.OrdinalIgnoreCase)) NavigateToDrama(8);
            else if (title.Contains("SOTUS", StringComparison.OrdinalIgnoreCase)) NavigateToDrama(9);
            else if (title.Contains("Thousand Stars", StringComparison.OrdinalIgnoreCase)) NavigateToDrama(10);
            else if (title.Contains("Crash Landing", StringComparison.OrdinalIgnoreCase)) NavigateToDrama(15);
            else if (title.Contains("Vincenzo", StringComparison.OrdinalIgnoreCase)) NavigateToDrama(21);
            else if (title.Contains("The Glory", StringComparison.OrdinalIgnoreCase)) NavigateToDrama(22);
            else if (title.Contains("Lovely Runner", StringComparison.OrdinalIgnoreCase)) NavigateToDrama(24);
            else if (title.Contains("Alchemy", StringComparison.OrdinalIgnoreCase)) NavigateToDrama(23);
            else NavigateToDrama(23);
        }
    }
}
