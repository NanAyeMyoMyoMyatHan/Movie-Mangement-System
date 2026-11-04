using DotNetProject.Balzor.Components.Shared;
using DotNetProject.Domain.Features.Movies;
using Microsoft.AspNetCore.Components;

namespace DotNetProject.Balzor.Components.Pages.Admin
{
    public partial class AdminDashboard
    {
        [Inject] private HttpClientService ApiClient { get; set; } = default!;
        [Inject] private AuthService AuthService { get; set; } = default!;

        private bool _loading = true;
        private int _movieCount;
        private int _contentCount;
        private int _userCount;
        private List<MovieListItemResponse> _recentMovies = new();

        protected override async Task OnInitializedAsync()
        {
            // Load real user count from DB
            try
            {
                var users = await AuthService.GetAllUsersFromDbAsync();
                _userCount = users.Count(u => !u.IsDeleted);
            }
            catch { }
            // Load movies (first page, 5 items for recent list)
            var movieResult = await ApiClient.GetMoviesPagedAsync(new GetMoviePagedListRequest
            {
                PageNo = 1,
                PageSize = 5
            });

            if (movieResult?.IsSuccess == true && movieResult.Data != null)
            {
                _movieCount = movieResult.Data.TotalCount;
                _recentMovies = movieResult.Data.Items?.ToList() ?? new();
            }

            // Load content count (first page, 1 item just to get total)
            var contentResult = await ApiClient.GetContentsPagedAsync(new DotNetProject.Domain.Features.Contents.GetContentPagedListRequest
            {
                PageNo = 1,
                PageSize = 1
            });

            if (contentResult?.IsSuccess == true && contentResult.Data != null)
            {
                _contentCount = contentResult.Data.TotalCount;
            }

            _loading = false;
        }
    }
}
