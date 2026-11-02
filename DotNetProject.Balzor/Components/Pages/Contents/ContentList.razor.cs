using DotNetProject.Balzor.Components.Shared;
using DotNetProject.Domain.Features.Contents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace DotNetProject.Balzor.Components.Pages.Contents
{
    public partial class ContentList
    {
        [Parameter, SupplyParameterFromQuery(Name = "page")]
        public int? PageQuery { get; set; }

        [Parameter, SupplyParameterFromQuery(Name = "q")]
        public string? QuerySearch { get; set; }

        private List<ContentListItemResponse> Items { get; set; } = new();
        private bool IsLoading { get; set; } = true;
        private string? ErrorMessage { get; set; }

        private string SearchTerm { get; set; } = string.Empty;
        private int CurrentPage { get; set; } = 1;
        private int PageSize { get; set; } = 10;
        private int TotalCount { get; set; } = 0;
        private int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        protected override async Task OnParametersSetAsync()
        {
            CurrentPage = PageQuery.HasValue && PageQuery.Value > 0 ? PageQuery.Value : 1;
            SearchTerm = QuerySearch ?? string.Empty;
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            IsLoading = true;
            ErrorMessage = null;

            var req = new GetContentPagedListRequest
            {
                PageNo = CurrentPage,
                PageSize = PageSize,
                SearchTerm = string.IsNullOrWhiteSpace(SearchTerm) ? null : SearchTerm
            };

            var res = await ApiClient.GetContentsPagedAsync(req);

            if (res != null && res.IsSuccess && res.Data != null)
            {
                Items = res.Data.Items?.ToList() ?? new();
                TotalCount = res.Data.TotalCount;
            }
            else
            {
                ErrorMessage = res?.Message ?? "Failed to load contents data.";
                Items = new();
                TotalCount = 0;
            }

            IsLoading = false;
        }

        private void HandleKeyUp(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                SearchAsync();
            }
        }

        private void SearchAsync()
        {
            Nav.NavigateTo(Nav.GetUriWithQueryParameters(new Dictionary<string, object?>
            {
                ["page"] = 1,
                ["q"] = string.IsNullOrWhiteSpace(SearchTerm) ? null : SearchTerm
            }));
        }

        private void ClearSearchAsync()
        {
            SearchTerm = string.Empty;
            SearchAsync();
        }

        private void GoToPageAsync(int page)
        {
            if (page < 1 || page > TotalPages) return;
            Nav.NavigateTo(Nav.GetUriWithQueryParameter("page", page));
        }

        private void PrevPageAsync() => GoToPageAsync(CurrentPage - 1);
        private void NextPageAsync() => GoToPageAsync(CurrentPage + 1);
    }
}
