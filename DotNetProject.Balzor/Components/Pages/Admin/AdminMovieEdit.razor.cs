using DotNetProject.Balzor.Components.Shared;
using Microsoft.AspNetCore.Components;

namespace DotNetProject.Balzor.Components.Pages.Admin
{
    public partial class AdminMovieEdit
    {
        [Parameter] public int Id { get; set; }
        [Inject] private HttpClientService ApiClient { get; set; } = default!;
        [Inject] private NavigationManager Nav { get; set; } = default!;
        [Inject] private AuditLogService AuditLog { get; set; } = default!;

        private MovieFormModel _form = new();
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

            _form = MovieFormModel.FromDetail(result.Data.Movie);
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

            if (!_form.TmdbId.HasValue)
                _form.IsManual = true;

            _submitting = true;
            try
            {
                var result = await ApiClient.UpdateMovieAsync(_form.ToUpdateRequest(Id), _form.PosterFile);
                if (result?.IsSuccess == true)
                {
                    await AuditLog.LogActionAsync("UPDATE_MOVIE", "Movie", Id.ToString(), $"Updated movie '{_form.Title}' (ID: {Id})");
                    _success = $"Movie \"{_form.Title}\" updated successfully!";
                    StateHasChanged();
                    await Task.Delay(1000);
                    Nav.NavigateTo("/admin/movies");
                }
                else
                {
                    _error = result?.Message ?? "Failed to update movie.";
                }
            }
            catch (NavigationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _error = $"An unexpected error occurred: {ex.Message}";
            }
            finally
            {
                _submitting = false;
                StateHasChanged();
            }
        }
    }
}
