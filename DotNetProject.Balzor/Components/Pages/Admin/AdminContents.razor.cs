using DotNetProject.Balzor.Components.Shared;
using DotNetProject.Domain.Features.Contents;
using Microsoft.AspNetCore.Components;

namespace DotNetProject.Balzor.Components.Pages.Admin
{
    public partial class AdminContents
    {
        [Inject] private HttpClientService ApiClient { get; set; } = default!;

        private List<ContentListItemResponse> _items = new();
        private int _totalCount;
        private int _totalPages;
        private int _currentPage = 1;
        private int _pageSize = 10;
        private string? _search;
        private bool _loading = true;
        private string? _error;

        protected override async Task OnInitializedAsync() => await LoadAsync();

        private async Task LoadAsync()
        {
            _loading = true;
            _error = null;

            var result = await ApiClient.GetContentsPagedAsync(new GetContentPagedListRequest
            {
                PageNo = _currentPage,
                PageSize = _pageSize,
                SearchTerm = _search
            });

            _loading = false;

            if (result == null || !result.IsSuccess || result.Data == null)
            {
                _error = result?.Message ?? "Failed to load contents.";
                return;
            }

            _items = result.Data.Items?.ToList() ?? new();
            _totalCount = result.Data.TotalCount;
            _currentPage = result.Data.PageNo;
            _totalPages = _totalCount == 0 ? 1 : (int)Math.Ceiling((double)_totalCount / result.Data.PageSize);
        }

        private async Task GoToPage(int page)
        {
            _currentPage = page;
            await LoadAsync();
        }

        private async Task OnPageSizeChange(ChangeEventArgs e)
        {
            _pageSize = int.TryParse(e.Value?.ToString(), out var v) ? v : 10;
            _currentPage = 1;
            await LoadAsync();
        }

        private async Task OnSearchKeyUp(Microsoft.AspNetCore.Components.Web.KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                _currentPage = 1;
                await LoadAsync();
            }
        }

        private async Task ClearSearch()
        {
            _search = null;
            _currentPage = 1;
            await LoadAsync();
        }
    }
}
