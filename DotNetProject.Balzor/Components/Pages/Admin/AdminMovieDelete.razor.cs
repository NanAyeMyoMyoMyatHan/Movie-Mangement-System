using DotNetProject.Balzor.Components.Shared;
using Microsoft.AspNetCore.Components;

namespace DotNetProject.Balzor.Components.Pages.Admin
{
    public partial class AdminMovieDelete
    {
        [Parameter] public int Id { get; set; }
        [Inject] private HttpClientService ApiClient { get; set; } = default!;
        [Inject] private NavigationManager Nav { get; set; } = default!;

        private bool _loading = true;
        private string? _loadError;
        private string? _title;
        private string? _error;
        private bool _deleting;

        protected override async Task OnInitializedAsync()
        {
            var result = await ApiClient.GetMovieDetailAsync(Id);
            _loading = false;

            if (result?.IsSuccess != true || result.Data == null)
            {
                _loadError = result?.Message ?? "Movie not found.";
                return;
            }

            _title = result.Data.Movie.Title;
        }

        private async Task ConfirmDelete()
        {
            _deleting = true;
            var result = await ApiClient.DeleteMovieAsync(Id);
            _deleting = false;

            if (result?.IsSuccess == true)
            {
                Nav.NavigateTo("/admin/movies");
            }
            else
            {
                _error = result?.Message ?? "Failed to delete movie.";
            }
        }
    }
}
