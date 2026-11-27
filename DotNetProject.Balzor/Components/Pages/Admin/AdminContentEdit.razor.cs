using DotNetProject.Balzor.Components.Shared;
using DotNetProject.Domain.Features.Contents;
using Microsoft.AspNetCore.Components;

namespace DotNetProject.Balzor.Components.Pages.Admin
{
    public partial class AdminContentEdit
    {
        [Parameter] public long Id { get; set; }
        [Inject] private HttpClientService ApiClient { get; set; } = default!;
        [Inject] private NavigationManager Nav { get; set; } = default!;
        [Inject] private AuditLogService AuditLog { get; set; } = default!;

        private UpdateContentRequest _form = new();
        private UploadedFilePayload? _posterFile;
        private UploadedFilePayload? _bannerFile;
        private bool _loading = true;
        private string? _loadError;
        private string? _success;
        private string? _error;
        private bool _submitting;

        protected override async Task OnParametersSetAsync()
        {
            _loading = true;
            _loadError = null;
            _posterFile = null;
            _bannerFile = null;

            try
            {
                var res = await ApiClient.GetContentDetailAsync(Id);

                if (res?.IsSuccess == true && res.Data?.Content != null)
                {
                    var c = res.Data.Content;
                    _form = new UpdateContentRequest
                    {
                        ContentId     = c.ContentId,
                        ContentTypeId = c.ContentTypeId,
                        Title         = c.Title,
                        Description   = c.Description,
                        PosterUrl     = c.PosterUrl,
                        BannerUrl     = c.BannerUrl,
                        Country       = c.Country,
                        Language      = c.Language,
                        ReleaseDate   = c.ReleaseDate,
                        Status        = c.Status
                    };
                }
                else
                {
                    _loadError = res?.Message ?? "Content not found.";
                }
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

        private async Task SubmitAsync()
        {
            _error = null;
            _success = null;

            if (string.IsNullOrWhiteSpace(_form.Title))
            {
                _error = "Title is required.";
                return;
            }

            _submitting = true;
            try
            {
                var result = await ApiClient.UpdateContentAsync(_form, _posterFile, _bannerFile);
                if (result?.IsSuccess == true)
                {
                    await AuditLog.LogActionAsync("UPDATE_CONTENT", "Content", _form.ContentId.ToString(), $"Updated content '{_form.Title}' (ID: {_form.ContentId})");
                    _success = $"\"{_form.Title}\" updated successfully!";
                    StateHasChanged();
                    await Task.Delay(1000);
                    Nav.NavigateTo("/admin/contents");
                }
                else
                {
                    _error = result?.Message ?? "Failed to update content.";
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
