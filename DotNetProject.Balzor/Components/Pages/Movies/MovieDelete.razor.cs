using DotNetProject.Balzor.Components.Shared;
using DotNetProject.Domain.Features.Movies;
using Microsoft.AspNetCore.Components;

namespace DotNetProject.Balzor.Components.Pages.Movies
{
    public partial class MovieDelete
    {
        [Parameter] public int Id { get; set; }

        [Parameter, SupplyParameterFromQuery(Name = "returnUrl")]
        public string? ReturnUrl { get; set; }

        private string? _movieTitle;
        private string? _error;
        private bool _loading = true;
        private bool _notFound;
        private bool _submitting;

        protected override async Task OnInitializedAsync()
        {
            var result = await ApiClient.GetMovieDetailAsync(Id);
            _loading = false;

            if (result == null || !result.IsSuccess || result.Data?.Movie == null)
            {
                _notFound = true;
                return;
            }

            _movieTitle = result.Data.Movie.Title;
        }

        private async Task ConfirmDeleteAsync()
        {
            _error = null;
            _submitting = true;

            var result = await ApiClient.DeleteMovieAsync(Id);
            _submitting = false;

            if (result == null || !result.IsSuccess)
            {
                _error = result?.Message ?? "Failed to delete movie.";
                return;
            }

            // Navigate back - adjust page if we were on the last item of last page
            var destination = string.IsNullOrWhiteSpace(ReturnUrl)
                ? "/movies"
                : Uri.UnescapeDataString(ReturnUrl);

            Nav.NavigateTo(destination, forceLoad: false);
        }
    }
}
