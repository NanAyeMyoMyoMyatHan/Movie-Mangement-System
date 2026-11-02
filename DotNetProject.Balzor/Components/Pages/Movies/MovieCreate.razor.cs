using DotNetProject.Balzor.Components.Shared;
using DotNetProject.Domain.Features.Movies;
using Microsoft.AspNetCore.Components;

namespace DotNetProject.Balzor.Components.Pages.Movies
{
    public partial class MovieCreate
    {
        private readonly CreateMovieRequest _form = new();
        private string? _titleError;
        private string? _error;
        private string? _success;
        private bool _submitting;

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
            var result = await ApiClient.CreateMovieAsync(_form);
            _submitting = false;

            if (result == null || !result.IsSuccess)
            {
                _error = result?.Message ?? "Failed to create movie.";
                return;
            }

            _success = "Movie created successfully!";
            // Reset form
            _form.Title = null;
            _form.Genre = null;
            _form.ReleaseYear = null;
            _form.Description = null;
            _form.PosterUrl = null;
            _form.TmdbId = null;
            _form.IsManual = false;
        }
    }
}
