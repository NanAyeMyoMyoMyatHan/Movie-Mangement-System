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
        protected bool IsPlayingVideo { get; set; } = false;
        protected bool IsWatchlisted { get; set; } = false;

        protected string ActiveDramaTitle { get; set; } = "Alchemy of Souls";
        protected string ActiveDramaDescription { get; set; } = "A powerful sorceress in a blind woman's body encounters a man from a prestigious family, who wants her help to change his destiny.";
        protected string? ActiveDramaPoster { get; set; } = "https://upload.wikimedia.org/wikipedia/en/9/9e/KinnPorsche_series_poster.jpg";

        protected string DramaYear { get; set; } = "2022";
        protected string DramaTotalEpisodesText { get; set; } = "30 Episodes";
        protected string DramaGenres { get; set; } = "Romance, Fantasy, Action";
        protected string DramaAgeRating { get; set; } = "15+";
        protected string DramaLanguage { get; set; } = "Korean";
        protected string DramaStarring { get; set; } = "Lee Jae-wook, Jung So-min, Go Youn-jung, Hwang Min-hyun";
        protected string DramaCreators { get; set; } = "Hong Jung-eun, Hong Mi-ran, Park Joon-hwa";

        protected string ActiveVideoTitle { get; set; } = "Episode 1";
        protected string ActiveVideoDescription { get; set; } = "";
        protected string ActiveVideoUrl { get; set; } = "";
        protected string? ActiveVideoPoster { get; set; }
        protected int ActiveEpisodeNumber { get; set; } = 1;
        protected List<EpisodeDto> ActiveEpisodes { get; set; } = new();

        public async Task PlayDramaAsync(long contentId, string title, string? desc, string? posterUrl)
        {
            ActiveDramaTitle = title;
            ActiveDramaDescription = !string.IsNullOrWhiteSpace(desc)
                ? desc
                : "A young master of a prestigious family and a fierce warrior trapped in a blind woman's body alter each other's fates in the mystical country of Daeho.";
            ActiveDramaPoster = !string.IsNullOrWhiteSpace(posterUrl) ? posterUrl : "https://upload.wikimedia.org/wikipedia/en/9/9e/KinnPorsche_series_poster.jpg";
            ActiveVideoPoster = ActiveDramaPoster;
            ActiveEpisodeNumber = 1;
            IsPlayingVideo = false;

            if (title.Contains("Alchemy", StringComparison.OrdinalIgnoreCase))
            {
                DramaYear = "2022";
                DramaGenres = "Romance, Fantasy, Action";
                DramaAgeRating = "15+";
                DramaLanguage = "Korean";
                DramaStarring = "Lee Jae-wook, Jung So-min, Go Youn-jung, Hwang Min-hyun";
                DramaCreators = "Hong Jung-eun, Hong Mi-ran, Park Joon-hwa";
            }
            else if (title.Contains("Moonlight", StringComparison.OrdinalIgnoreCase))
            {
                DramaYear = "2016";
                DramaGenres = "Romance, Historical";
                DramaAgeRating = "7+";
                DramaLanguage = "Korean";
                DramaStarring = "Park Bo-gum, Kim Yoo-jung, Jinyoung, Chae Soo-bin";
                DramaCreators = "Baek Sang-hoon, Kim Min-jung";
            }
            else if (title.Contains("Kingdom", StringComparison.OrdinalIgnoreCase))
            {
                DramaYear = "2019";
                DramaGenres = "Action, Horror, Period";
                DramaAgeRating = "18+";
                DramaLanguage = "Korean";
                DramaStarring = "Ju Ji-hoon, Ryu Seung-ryong, Bae Doona";
                DramaCreators = "Kim Eun-hee, Kim Seong-hun";
            }
            else
            {
                DramaYear = "2023";
                DramaGenres = "Drama, Romance";
                DramaAgeRating = "13+";
                DramaLanguage = "Asian";
                DramaStarring = "Cast Ensemble";
                DramaCreators = "Production Team";
            }

            var detailResult = await HttpClientService.GetContentWithEpisodesAsync(contentId);
            if (detailResult != null && detailResult.IsSuccess && detailResult.Data?.Episodes != null && detailResult.Data.Episodes.Count > 0)
            {
                ActiveEpisodes = detailResult.Data.Episodes;
                DramaTotalEpisodesText = $"{ActiveEpisodes.Count} Episodes";
                var firstEp = ActiveEpisodes.First();
                ActiveEpisodeNumber = firstEp.EpisodeNumber;
                ActiveVideoUrl = !string.IsNullOrWhiteSpace(firstEp.VideoUrl)
                    ? firstEp.VideoUrl
                    : "/videos/video_2026-08-04_13-49-33.mp4";
                ActiveVideoTitle = !string.IsNullOrWhiteSpace(firstEp.Title) ? firstEp.Title : $"{title} - Episode 1";
                ActiveVideoDescription = !string.IsNullOrWhiteSpace(firstEp.Description) ? firstEp.Description : ActiveDramaDescription;
            }
            else
            {
                ActiveEpisodes = GenerateFallbackEpisodes(title, 30);
                DramaTotalEpisodesText = $"{ActiveEpisodes.Count} Episodes";
                ActiveVideoUrl = "/videos/video_2026-08-04_13-49-33.mp4";
                ActiveVideoTitle = $"{title} - Episode 1";
                ActiveVideoDescription = ActiveDramaDescription;
            }

            IsVideoModalOpen = true;
            StateHasChanged();
        }

        public void StartPlaying()
        {
            IsPlayingVideo = true;
            if (ActiveEpisodes.Any())
            {
                var ep = ActiveEpisodes.FirstOrDefault(e => e.EpisodeNumber == ActiveEpisodeNumber) ?? ActiveEpisodes.First();
                PlayEpisodeFromRow(ep);
            }
            StateHasChanged();
        }

        public void PlayEpisodeFromRow(EpisodeDto ep)
        {
            ActiveEpisodeNumber = ep.EpisodeNumber;
            ActiveVideoTitle = !string.IsNullOrWhiteSpace(ep.Title) ? ep.Title : $"{ActiveDramaTitle} - Episode {ep.EpisodeNumber}";
            ActiveVideoDescription = !string.IsNullOrWhiteSpace(ep.Description) ? ep.Description : ActiveDramaDescription;
            ActiveVideoUrl = !string.IsNullOrWhiteSpace(ep.VideoUrl) ? ep.VideoUrl : "/videos/video_2026-08-04_13-49-33.mp4";
            IsPlayingVideo = true;
            StateHasChanged();
        }

        public void StopPlayingVideo()
        {
            IsPlayingVideo = false;
            StateHasChanged();
        }

        public void ToggleWatchlist()
        {
            IsWatchlisted = !IsWatchlisted;
            StateHasChanged();
        }

        public void ShareDrama()
        {
            // Share action
        }

        public string GetEpisodeThumbnail(EpisodeDto ep)
        {
            if (!string.IsNullOrWhiteSpace(ep.ThumbnailUrl)) return ep.ThumbnailUrl;
            if (!string.IsNullOrWhiteSpace(ActiveDramaPoster)) return ActiveDramaPoster;
            return "https://upload.wikimedia.org/wikipedia/en/9/9e/KinnPorsche_series_poster.jpg";
        }

        public void PlayKinnPorsche()
        {
            ActiveDramaTitle = "KinnPorsche: The Series";
            ActiveDramaDescription = "The second son of a mafia family, Kinn, is assigned a bodyguard, Porsche. Their relationship starts with distrust and slowly develops into something more.";
            ActiveDramaPoster = "https://upload.wikimedia.org/wikipedia/en/9/9e/KinnPorsche_series_poster.jpg";
            ActiveVideoPoster = ActiveDramaPoster;
            DramaYear = "2022";
            DramaGenres = "Action, Romance, Crime";
            DramaAgeRating = "18+";
            DramaLanguage = "Thai";
            DramaStarring = "Mile Phakphum, Apo Nattawin, Bible Sumettikul, Build Jakapan";
            DramaCreators = "Kongkiat Komesiri, Krisda Witthayakhajorndet";
            ActiveEpisodeNumber = 1;
            ActiveVideoTitle = "Episode 1";
            ActiveVideoUrl = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/TearsOfSteel.mp4";
            ActiveEpisodes = GenerateFallbackEpisodes("KinnPorsche", 14);
            DramaTotalEpisodesText = "14 Episodes";
            IsPlayingVideo = false;
            IsVideoModalOpen = true;
            StateHasChanged();
        }

        public void PlayContent(string title, string? desc, string? posterUrl, int ep = 1, string? videoUrl = null)
        {
            ActiveDramaTitle = title;
            ActiveDramaDescription = desc ?? ("Enjoy watching " + title + " in high definition on NangWorld.");
            ActiveDramaPoster = posterUrl;
            ActiveVideoPoster = posterUrl;
            DramaYear = "2023";
            DramaGenres = "Drama, Romance";
            DramaAgeRating = "13+";
            DramaLanguage = "Asian";
            DramaStarring = "Cast Ensemble";
            DramaCreators = "Production Team";
            ActiveEpisodeNumber = ep;
            ActiveVideoTitle = $"{title} - Episode {ep}";
            ActiveVideoUrl = !string.IsNullOrWhiteSpace(videoUrl)
                ? videoUrl
                : "/videos/video_2026-08-04_13-49-33.mp4";
            ActiveEpisodes = GenerateFallbackEpisodes(title, 16);
            DramaTotalEpisodesText = "16 Episodes";
            IsPlayingVideo = false;
            IsVideoModalOpen = true;
            StateHasChanged();
        }

        public void SelectEpisode(int epNumber)
        {
            ActiveEpisodeNumber = epNumber;
            ActiveVideoUrl = epNumber % 2 == 0
                ? "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ElephantsDream.mp4"
                : "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/TearsOfSteel.mp4";
            StateHasChanged();
        }

        public void CloseVideoPlayer()
        {
            IsVideoModalOpen = false;
            IsPlayingVideo = false;
            StateHasChanged();
        }

        private List<EpisodeDto> GenerateFallbackEpisodes(string seriesTitle, int count)
        {
            var list = new List<EpisodeDto>();
            for (int i = 1; i <= count; i++)
            {
                list.Add(new EpisodeDto
                {
                    EpisodeNumber = i,
                    Title = $"Episode {i}",
                    Description = $"Follow the dramatic events and revelations in Episode {i} of {seriesTitle}.",
                    Duration = 65 + (i % 15),
                    VideoUrl = "/videos/video_2026-08-04_13-49-33.mp4"
                });
            }
            return list;
        }
    }
}