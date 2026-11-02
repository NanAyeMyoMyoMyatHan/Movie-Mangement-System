using DotNetProject.Balzor.Components.Shared;
using DotNetProject.Domain.Features.Contents;
using Microsoft.AspNetCore.Components;

namespace DotNetProject.Balzor.Components.Pages.Admin
{
    public partial class AdminContentCreate
    {
        [Inject] private HttpClientService ApiClient { get; set; } = default!;
        [Inject] private NavigationManager Nav { get; set; } = default!;

        private readonly CreateContentRequest _form = new();
        private string? _releaseDateStr;
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

            _form.ReleaseDate = DateOnly.TryParse(_releaseDateStr, out var d) ? d : null;

            _submitting = true;
            var result = await ApiClient.CreateContentAsync(_form);
            _submitting = false;

            if (result?.IsSuccess == true)
            {
                _success = $"\"{_form.Title}\" created successfully!";
                await Task.Delay(1200);
                Nav.NavigateTo("/admin/contents");
            }
            else
            {
                _error = result?.Message ?? "Failed to create content.";
            }
        }
    }
}
