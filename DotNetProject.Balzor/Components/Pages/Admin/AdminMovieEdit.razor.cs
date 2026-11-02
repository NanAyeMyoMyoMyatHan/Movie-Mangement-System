using DotNetProject.Balzor.Components.Shared;
using DotNetProject.Domain.Features.Movies;
using Microsoft.AspNetCore.Components;

namespace DotNetProject.Balzor.Components.Pages.Admin
{
    public partial class AdminMovieEdit
    {
        [Parameter] public int Id { get; set; }
        [Inject] private HttpClientService ApiClient { get; set; } = default!;
        [Inject] private NavigationManager Nav { get; set; } = default!;

        private readonly UpdateMovieRequest _form = new();
        private bool _loading = true;
        private string? _loadError;
        private string? _titleError;
        private string? _success;
        private string? _error;
        private bool _submitting;

        protected override async Task OnInitializedAsync()
        {
            var result = await ApiClient.GetMovieDetailAsync(Id);
            _loading = false;

            if (result?.IsSuccess != true || result.Data == null)
            {
                _loadError = result?.Message ?? "Movie not found.";
                return;
            }

            var d = result.Data.Movie;
            _form.Id          = d.Id;
            _form.Title       = d.Title;
            _form.Genre       = d.Genre;
            _form.ReleaseYear = d.ReleaseYear;
            _form.Description = d.Description;
            _form.PosterUrl   = d.PosterUrl;
            _form.TmdbId      = d.TmdbId;
            _form.IsManual    = d.IsManual;
        }

        private async Task SubmitAsync()
        {
            _titleError = null;
            _error = null;
            _success = null;

            if (string.IsNullOrWhiteSpace(_form.Title))
            {
                _titleError = "Title is required.";
                return;
            }

            _submitting = true;
            var result = await ApiClient.UpdateMovieAsync(_form);
            _submitting = false;

            if (result?.IsSuccess == true)
            {
                _success = $"Movie \"{_form.Title}\" updated successfully!";
                await Task.Delay(1200);
                Nav.NavigateTo("/admin/movies");
            }
            else
            {
                _error = result?.Message ?? "Failed to update movie.";
            }
        }
    }
}
