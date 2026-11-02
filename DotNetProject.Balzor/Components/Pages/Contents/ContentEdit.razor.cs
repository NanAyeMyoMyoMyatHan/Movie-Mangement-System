using DotNetProject.Balzor.Components.Shared;
using DotNetProject.Domain.Features.Contents;
using Microsoft.AspNetCore.Components;

namespace DotNetProject.Balzor.Components.Pages.Contents
{
    public partial class ContentEdit
    {
        [Parameter] public long Id { get; set; }

        private UpdateContentRequest _form = new();
        private string? _releaseDateStr;
        private bool IsLoading { get; set; } = true;
        private bool IsSubmitting { get; set; } = false;
        private string? ErrorMessage { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            IsLoading = true;
            ErrorMessage = null;

            var res = await ApiClient.GetContentDetailAsync(Id);

            if (res != null && res.IsSuccess && res.Data?.Content != null)
            {
                var c = res.Data.Content;
                _form = new UpdateContentRequest
                {
                    ContentId    = c.ContentId,
                    ContentTypeId = c.ContentTypeId,
                    Title        = c.Title,
                    Description  = c.Description,
                    PosterUrl    = c.PosterUrl,
                    BannerUrl    = c.BannerUrl,
                    Country      = c.Country,
                    Language     = c.Language,
                    ReleaseDate  = c.ReleaseDate,
                    Status       = c.Status
                };
                _releaseDateStr = c.ReleaseDate?.ToString("yyyy-MM-dd");
            }
            else
            {
                ErrorMessage = res?.Message ?? "Content not found.";
            }

            IsLoading = false;
        }

        private async Task HandleSubmitAsync()
        {
            if (string.IsNullOrWhiteSpace(_form.Title))
            {
                ErrorMessage = "Title is required.";
                return;
            }

            _form.ReleaseDate = DateOnly.TryParse(_releaseDateStr, out var d) ? d : null;

            IsSubmitting = true;
            ErrorMessage = null;

            var res = await ApiClient.UpdateContentAsync(_form);

            IsSubmitting = false;

            if (res != null && res.IsSuccess)
            {
                Nav.NavigateTo("/contents");
            }
            else
            {
                ErrorMessage = res?.Message ?? "Failed to update content.";
            }
        }
    }
}
