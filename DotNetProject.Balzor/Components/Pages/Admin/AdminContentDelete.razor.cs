using DotNetProject.Balzor.Components.Shared;
using Microsoft.AspNetCore.Components;

namespace DotNetProject.Balzor.Components.Pages.Admin
{
    public partial class AdminContentDelete
    {
        [Parameter] public long Id { get; set; }
        [Inject] private HttpClientService ApiClient { get; set; } = default!;
        [Inject] private NavigationManager Nav { get; set; } = default!;

        private bool _loading = true;
        private string? _loadError;
        private string? _title;
        private string? _error;
        private bool _deleting;

        protected override async Task OnInitializedAsync()
        {
            var result = await ApiClient.GetContentDetailAsync(Id);
            _loading = false;

            if (result?.IsSuccess != true || result.Data?.Content == null)
            {
                _loadError = result?.Message ?? "Content not found.";
                return;
            }

            _title = result.Data.Content.Title;
        }

        private async Task ConfirmDelete()
        {
            _deleting = true;
            var result = await ApiClient.DeleteContentAsync(Id);
            _deleting = false;

            if (result?.IsSuccess == true)
            {
                Nav.NavigateTo("/admin/contents");
            }
            else
            {
                _error = result?.Message ?? "Failed to delete content.";
            }
        }
    }
}
