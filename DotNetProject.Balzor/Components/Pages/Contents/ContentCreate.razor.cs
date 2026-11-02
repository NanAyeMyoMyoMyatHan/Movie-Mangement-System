using DotNetProject.Balzor.Components.Shared;
using DotNetProject.Domain.Features.Contents;
using Microsoft.AspNetCore.Components;

namespace DotNetProject.Balzor.Components.Pages.Contents
{
    public partial class ContentCreate
    {
        private readonly CreateContentRequest _form = new();
        private string? _releaseDateStr;
        private bool IsSubmitting { get; set; } = false;
        private string? ErrorMessage { get; set; }

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

            var res = await ApiClient.CreateContentAsync(_form);

            IsSubmitting = false;

            if (res != null && res.IsSuccess)
            {
                Nav.NavigateTo("/contents");
            }
            else
            {
                ErrorMessage = res?.Message ?? "Failed to create content.";
            }
        }
    }
}
