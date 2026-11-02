using DotNetProject.Balzor.Components.Shared;
using DotNetProject.Domain.Features.Contents;
using Microsoft.AspNetCore.Components;

namespace DotNetProject.Balzor.Components.Pages.Contents
{
    public partial class ContentDelete
    {
        [Parameter] public long Id { get; set; }

        private ContentDetailResponse? _content;
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
                _content = res.Data.Content;
            }
            else
            {
                ErrorMessage = res?.Message ?? "Content not found.";
            }

            IsLoading = false;
        }

        private async Task HandleDeleteAsync()
        {
            IsSubmitting = true;
            ErrorMessage = null;

            var res = await ApiClient.DeleteContentAsync(Id);

            IsSubmitting = false;

            if (res != null && res.IsSuccess)
            {
                Nav.NavigateTo("/contents");
            }
            else
            {
                ErrorMessage = res?.Message ?? "Failed to delete content.";
            }
        }
    }
}
