using DotNetProject.Balzor.Components.Shared;
using DotNetProject.Domain.Features.Movies;
using Microsoft.AspNetCore.Components;

namespace DotNetProject.Balzor.Components.Pages.Movies
{
    public partial class MovieList
    {
        [Parameter, SupplyParameterFromQuery(Name = "page")]
        public int Page { get; set; } = 1;

        [Parameter, SupplyParameterFromQuery(Name = "pageSize")]
        public int PageSize { get; set; } = 10;

        [Parameter, SupplyParameterFromQuery(Name = "search")]
        public string? Search { get; set; }

        private List<MovieListItemResponse> _items = new();
        private int _totalCount;
        private int _totalPages;
        private int _currentPage;
        private bool _loading = true;
        private string? _error;

        private string ReturnUrl =>
            Uri.EscapeDataString($"/movies?page={_currentPage}&pageSize={PageSize}&search={Search}");

        protected override async Task OnInitializedAsync()
        {
            _currentPage = Page < 1 ? 1 : Page;
            var pageSize = PageSize < 1 ? 10 : PageSize;

            var result = await ApiClient.GetMoviesPagedAsync(new GetMoviePagedListRequest
            {
                PageNo = _currentPage,
                PageSize = pageSize,
                SearchTerm = Search
            });

            _loading = false;

            if (result == null || !result.IsSuccess || result.Data == null)
            {
                _error = result?.Message ?? "Failed to load movies.";
                return;
            }

            _items = result.Data.Items?.ToList() ?? new();
            _totalCount = result.Data.TotalCount;
            _currentPage = result.Data.PageNo;
            _totalPages = _totalCount == 0 ? 1 : (int)Math.Ceiling((double)_totalCount / result.Data.PageSize);
        }

        private string PageUrl(int p) =>
            $"/movies?page={p}&pageSize={PageSize}" +
            (string.IsNullOrWhiteSpace(Search) ? "" : $"&search={Uri.EscapeDataString(Search!)}");
    }
}
