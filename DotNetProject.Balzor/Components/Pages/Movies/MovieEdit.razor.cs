using DotNetProject.Balzor.Components.Shared;
using DotNetProject.Domain.Features.Movies;
using Microsoft.AspNetCore.Components;

namespace DotNetProject.Balzor.Components.Pages.Movies
{
    public partial class MovieEdit
    {
        [Parameter] public int Id { get; set; }

        [Parameter, SupplyParameterFromQuery(Name = "returnUrl")]
        public string? ReturnUrl { get; set; }

        private readonly UpdateMovieRequest _form = new();
        private string? _titleError;
        private string? _error;
        private string? _success;
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

            var m = result.Data.Movie;
            _form.Id = m.Id;
            _form.Title = m.Title;
            _form.Genre = m.Genre;
            _form.ReleaseYear = m.ReleaseYear;
            _form.Description = m.Description;
            _form.PosterUrl = m.PosterUrl;
            _form.TmdbId = m.TmdbId;
            _form.IsManual = m.IsManual;
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

            if (result == null || !result.IsSuccess)
            {
                _error = result?.Message ?? "Failed to update movie.";
                return;
            }

            _success = "Movie updated successfully!";
        }
    }
}
