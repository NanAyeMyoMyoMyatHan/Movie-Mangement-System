using DotNetProject.Balzor.Components.Shared;
using DotNetProject.Domain.Features.Contents;
using DotNetProject.Domain.Features.Movies;
using DotNetProject.Domain.Features.Thai;
using Microsoft.AspNetCore.Components;

namespace DotNetProject.Balzor.Components.Pages
{
    public partial class Index
    {
        [Inject]
        private HttpClientService HttpClientService { get; set; } = default!;

        protected List<ContentListItemResponse> Dramas { get; set; } = new();
        protected List<MovieListItemResponse> Movies { get; set; } = new();

        protected List<ThaiListItemResponse> Thais { get; set; } = new();
        protected bool IsLoading { get; set; } = true;
        protected string? ErrorMessage { get; set; }

        protected override async Task OnInitializedAsync()
        {
            IsLoading = true;
            //Drama Section
            var request = new GetContentPagedListRequest
            {
                PageNo = 1,
                PageSize = 6,
                SearchTerm = null
            };

            var result = await HttpClientService.GetContentsPagedAsync(request);

            if (result != null && result.IsSuccess && result.Data != null)
            {
                Dramas = result.Data.Items?.ToList() ?? new();
            }
            else
            {
                ErrorMessage = result?.Message ?? "Failed to load contents.";
            }
            // Movie Section
            var movieRequest = new GetMoviePagedListRequest
            {
                PageNo = 1,
                PageSize = 6,
                SearchTerm = null
            };

            var moviesResult = await HttpClientService.GetMoviesPagedAsync(movieRequest);
            if (moviesResult != null && moviesResult.IsSuccess && moviesResult.Data != null)
            {
                Movies = moviesResult.Data.Items?.ToList() ?? new();
            }
            else
            {
                ErrorMessage = moviesResult?.Message ?? "Failed to load contents.";
            }
            //Thai Section
            var thaiRequest = new GetThaiPagedListRequest
            {
                PageNo = 1,
                PageSize = 6,
                SearchTerm = null,
            };

            var thaiResult = await HttpClientService.GetThaiPagedAsync(thaiRequest);
            if (thaiResult != null && thaiResult.IsSuccess && thaiResult.Data != null)
            {
                Thais = thaiResult.Data.Items?.ToList() ?? new();
            }
            else
            {
                ErrorMessage = thaiResult?.Message ?? "Failed to load Thai contents.";
            }

            IsLoading = false;
        }

        protected bool IsVideoModalOpen { get; set; } = false;
        protected string ActiveVideoTitle { get; set; } = "KinnPorsche: The Series";
        protected string ActiveVideoDescription { get; set; } = "The second son of a mafia family, Kinn, is assigned a bodyguard, Porsche. Their relationship starts with distrust and slowly develops into something more.";
        protected string ActiveVideoUrl { get; set; } = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/TearsOfSteel.mp4";
        protected string? ActiveVideoPoster { get; set; } = "https://upload.wikimedia.org/wikipedia/en/9/9e/KinnPorsche_series_poster.jpg";
        protected int ActiveEpisodeNumber { get; set; } = 1;
        protected List<EpisodeDto> ActiveEpisodes { get; set; } = new();

        public async Task PlayDramaAsync(long contentId, string title, string? desc, string? posterUrl)
        {
            ActiveVideoTitle = title;
            ActiveVideoDescription = desc ?? ("Enjoy watching " + title + " in high definition.");
            ActiveVideoPoster = posterUrl;
            ActiveEpisodeNumber = 1;

            var detailResult = await HttpClientService.GetContentWithEpisodesAsync(contentId);
            if (detailResult != null && detailResult.IsSuccess && detailResult.Data?.Episodes != null && detailResult.Data.Episodes.Count > 0)
            {
                ActiveEpisodes = detailResult.Data.Episodes;
                var firstEp = ActiveEpisodes.First();
                ActiveEpisodeNumber = firstEp.EpisodeNumber;
                ActiveVideoUrl = !string.IsNullOrWhiteSpace(firstEp.VideoUrl) ? firstEp.VideoUrl : "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4";
                if (!string.IsNullOrWhiteSpace(firstEp.Title))
                {
                    ActiveVideoTitle = firstEp.Title;
                }
                if (!string.IsNullOrWhiteSpace(firstEp.Description))
                {
                    ActiveVideoDescription = firstEp.Description;
                }
            }
            else
            {
                ActiveEpisodes = new();
                ActiveVideoUrl = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4";
            }

            IsVideoModalOpen = true;
            StateHasChanged();
        }

        public void SelectDramaEpisode(EpisodeDto ep)
        {
            ActiveEpisodeNumber = ep.EpisodeNumber;
            ActiveVideoTitle = !string.IsNullOrWhiteSpace(ep.Title) ? ep.Title : $"{ActiveVideoTitle} - EP {ep.EpisodeNumber}";
            if (!string.IsNullOrWhiteSpace(ep.Description))
            {
                ActiveVideoDescription = ep.Description;
            }
            ActiveVideoUrl = !string.IsNullOrWhiteSpace(ep.VideoUrl)
                ? ep.VideoUrl
                : "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4";
            StateHasChanged();
        }

        public void PlayKinnPorsche()
        {
            ActiveVideoTitle = "KinnPorsche: The Series";
            ActiveVideoDescription = "The second son of a mafia family, Kinn, is assigned a bodyguard, Porsche. Their relationship starts with distrust and slowly develops into something more.";
            ActiveVideoUrl = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/TearsOfSteel.mp4";
            ActiveVideoPoster = "https://upload.wikimedia.org/wikipedia/en/9/9e/KinnPorsche_series_poster.jpg";
            ActiveEpisodeNumber = 1;
            IsVideoModalOpen = true;
            StateHasChanged();
        }

        public void PlayContent(string title, string? desc, string? posterUrl, int ep = 1, string? videoUrl = null)
        {
            ActiveVideoTitle = title;
            ActiveVideoDescription = desc ?? "Enjoy watching " + title + " in high definition on NangWorld.";
            ActiveVideoPoster = posterUrl;
            ActiveEpisodeNumber = ep;
            ActiveVideoUrl = !string.IsNullOrWhiteSpace(videoUrl)
                ? videoUrl
                : "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4";
            IsVideoModalOpen = true;
            StateHasChanged();
        }

        public void SelectEpisode(int epNumber)
        {
            ActiveEpisodeNumber = epNumber;
            // Cycle sample MP4 streams
            ActiveVideoUrl = epNumber % 2 == 0
                ? "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ElephantsDream.mp4"
                : "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/TearsOfSteel.mp4";
            StateHasChanged();
        }

        public void CloseVideoPlayer()
        {
            IsVideoModalOpen = false;
            StateHasChanged();
        }
    }
}
