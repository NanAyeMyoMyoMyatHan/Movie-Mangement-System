using DotNetProject.Balzor.Components.Shared;
using Microsoft.AspNetCore.Components;

namespace DotNetProject.Balzor.Components.Pages.Admin
{
    public partial class AdminContentDelete
    {
        [Parameter] public long Id { get; set; }
        [Inject] private HttpClientService ApiClient { get; set; } = default!;
        [Inject] private NavigationManager Nav { get; set; } = default!;
        [Inject] private AuditLogService AuditLog { get; set; } = default!;

        private bool _loading = true;
        private string? _loadError;
        private string? _title;
        private string? _error;
        private bool _deleting;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var result = await ApiClient.GetContentDetailAsync(Id);
                if (result?.IsSuccess != true || result.Data?.Content == null)
                {
                    _loadError = result?.Message ?? "Content not found.";
                    return;
                }

                _title = result.Data.Content.Title;
            }
            catch (Exception ex)
            {
                _loadError = $"Failed to load content: {ex.Message}";
            }
            finally
            {
                _loading = false;
            }
        }

        private async Task ConfirmDelete()
        {
            _error = null;
            _deleting = true;
            try
            {
                var result = await ApiClient.DeleteContentAsync(Id);
                if (result?.IsSuccess == true)
                {
                    await AuditLog.LogActionAsync("DELETE_CONTENT", "Content", Id.ToString(), $"Deleted content '{_title}' (ID: {Id})");
                    Nav.NavigateTo("/admin/contents");
                }
                else
                {
                    _error = result?.Message ?? "Failed to delete content.";
                }
            }
            catch (Exception ex)
            {
                _error = $"An unexpected error occurred: {ex.Message}";
            }
            finally
            {
                _deleting = false;
                StateHasChanged();
            }
        }
    }
}
