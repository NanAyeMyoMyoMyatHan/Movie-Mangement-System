using DotNetProject.Balzor.Components.Shared;
using DotNetProject.Domain.Features.Movies;
using Microsoft.AspNetCore.Components;

namespace DotNetProject.Balzor.Components.Pages.Admin
{
    public partial class AdminMovieCreate
    {
        [Inject] private HttpClientService ApiClient { get; set; } = default!;
        [Inject] private NavigationManager Nav { get; set; } = default!;

        private readonly CreateMovieRequest _form = new();
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

            _submitting = true;
            var result = await ApiClient.CreateMovieAsync(_form);
            _submitting = false;

            if (result?.IsSuccess == true)
            {
                _success = $"Movie \"{_form.Title}\" created successfully!";
                await Task.Delay(1200);
                Nav.NavigateTo("/admin/movies");
            }
            else
            {
                _error = result?.Message ?? "Failed to create movie.";
            }
        }
    }
}
