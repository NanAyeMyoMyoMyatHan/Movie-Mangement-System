using DotNetProject.Balzor.Components.Shared;
using DotNetProject.Domain.Features.Contents;
using Microsoft.AspNetCore.Components;

namespace DotNetProject.Balzor.Components.Pages.Admin
{
    public partial class AdminContentCreate
    {
        [Inject] private HttpClientService ApiClient { get; set; } = default!;
        [Inject] private NavigationManager Nav { get; set; } = default!;
        [Inject] private AuditLogService AuditLog { get; set; } = default!;

        private readonly CreateContentRequest _form = new() { ContentTypeId = 2, Status = "Ongoing" };
        private DateOnly? _releaseDateValue;
        private UploadedFilePayload? _posterFile;
        private UploadedFilePayload? _bannerFile;
        private string? _success;
        private string? _error;
        private bool _submitting;

        private async Task SubmitAsync()
        {
            _error = null;
            _success = null;

            if (string.IsNullOrWhiteSpace(_form.Title))
            {
                _error = "Title is required.";
                return;
            }

            _form.ReleaseDate = _releaseDateValue;

            _submitting = true;
            try
            {
                var result = await ApiClient.CreateContentAsync(_form, _posterFile, _bannerFile);
                if (result?.IsSuccess == true)
                {
                    var createdId = result.Data?.Content?.ContentId.ToString();
                    await AuditLog.LogActionAsync("CREATE_CONTENT", "Content", createdId, $"Created content '{_form.Title}'");
                    _success = $"\"{_form.Title}\" created successfully!";
                    StateHasChanged();
                    await Task.Delay(1000);
                    Nav.NavigateTo("/admin/contents");
                }
                else
                {
                    _error = result?.Message ?? "Failed to create content.";
                }
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
