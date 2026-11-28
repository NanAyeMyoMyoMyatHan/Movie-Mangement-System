using DotNetProject.Balzor.Components.Shared;
using Microsoft.AspNetCore.Components;

namespace DotNetProject.Balzor.Components.Pages.Admin
{
    public partial class AdminMovieCreate
    {
        [Inject] private HttpClientService ApiClient { get; set; } = default!;
        [Inject] private NavigationManager Nav { get; set; } = default!;
        [Inject] private AuditLogService AuditLog { get; set; } = default!;

        private readonly MovieFormModel _form = new() { IsManual = false };
        private string? _titleError;
        private string? _success;
        private string? _error;
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

            if (!_form.TmdbId.HasValue)
                _form.IsManual = true;

            _submitting = true;
            try
            {
                var result = await ApiClient.CreateMovieAsync(_form.ToCreateRequest(), _form.PosterFile);
                if (result?.IsSuccess == true)
                {
                    await AuditLog.LogActionAsync("CREATE_MOVIE", "Movie", null, $"Created movie '{_form.Title}'");
                    _success = $"Movie \"{_form.Title}\" created successfully!";
                    StateHasChanged();
                    await Task.Delay(1000);
                    Nav.NavigateTo("/admin/movies");
                }
                else
                {
                    _error = result?.Message ?? "Failed to create movie.";
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
