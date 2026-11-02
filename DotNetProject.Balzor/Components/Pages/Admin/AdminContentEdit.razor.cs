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

        private UpdateContentRequest _form = new();
        private string? _releaseDateStr;
        private bool _loading = true;
        private string? _loadError;
        private string? _success;
        private string? _error;
        private bool _submitting;

        protected override async Task OnParametersSetAsync()
        {
            _loading = true;
            _loadError = null;

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
                _releaseDateStr = c.ReleaseDate?.ToString("yyyy-MM-dd");
            }
            else
            {
                _loadError = res?.Message ?? "Content not found.";
            }

            _loading = false;
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

            _form.ReleaseDate = DateOnly.TryParse(_releaseDateStr, out var d) ? d : null;

            _submitting = true;
            var result = await ApiClient.UpdateContentAsync(_form);
            _submitting = false;

            if (result?.IsSuccess == true)
            {
                _success = $"\"{_form.Title}\" updated successfully!";
                await Task.Delay(1200);
                Nav.NavigateTo("/admin/contents");
            }
            else
            {
                _error = result?.Message ?? "Failed to update content.";
            }
        }
    }
}
