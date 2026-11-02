using DotNetProject.Balzor.Components.Shared;
using DotNetProject.Domain.Features.Contents;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DotNetProject.Balzor.Components.Pages
{
    public partial class DramaDetail
    {
        [Inject]
        private HttpClientService ApiClient { get; set; } = default!;

        [Inject]
        private NavigationManager Nav { get; set; } = default!;

        [Inject]
        private IJSRuntime JS { get; set; } = default!;

        [Parameter]
        public long Id { get; set; }

        protected ContentDetailDto? Drama { get; set; }
        protected List<EpisodeDto> Episodes { get; set; } = new();
        protected bool IsLoading { get; set; } = true;
        protected string? ErrorMessage { get; set; }

        protected bool IsPlayingVideo { get; set; } = false;
        protected bool IsWatchlisted { get; set; } = false;

        protected string ActiveVideoTitle { get; set; } = "";
        protected string ActiveVideoDescription { get; set; } = "";
        protected string ActiveVideoUrl { get; set; } = "";
        protected string? ActiveVideoPoster { get; set; }
        protected int ActiveEpisodeNumber { get; set; } = 1;

        protected string DramaYear { get; set; } = "2022";
        protected string DramaTotalEpisodesText { get; set; } = "30 Episodes";
        protected string DramaGenres { get; set; } = "Romance, Fantasy, Action";
        protected string DramaAgeRating { get; set; } = "15+";
        protected string DramaLanguage { get; set; } = "Korean";
        protected string DramaStarring { get; set; } = "Lee Jae-wook, Jung So-min, Go Youn-jung, Hwang Min-hyun";
        protected string DramaCreators { get; set; } = "Hong Jung-eun, Hong Mi-ran, Park Joon-hwa";

        protected override async Task OnParametersSetAsync()
        {
            IsLoading = true;
            IsPlayingVideo = false;
            ErrorMessage = null;

            try
            {
                var result = await ApiClient.GetContentWithEpisodesAsync(Id);
                if (result != null && result.IsSuccess && result.Data != null)
                {
                    Drama = result.Data;
                    Episodes = Drama.Episodes?.Where(e => !string.IsNullOrEmpty(e.Status) && e.Status != "Deleted").OrderBy(e => e.EpisodeNumber).ToList() ?? new();

                    DramaYear = Drama.ReleaseDate?.Year.ToString() ?? "2022";
                    DramaLanguage = !string.IsNullOrWhiteSpace(Drama.Language) ? Drama.Language : "Korean";
                    DramaGenres = (Drama.Genres != null && Drama.Genres.Any()) ? string.Join(", ", Drama.Genres) : "Romance, Fantasy, Action";

                    if (Episodes.Count == 0)
                    {
                        Episodes = GenerateFallbackEpisodes(Drama.Title, 16);
                    }

                    DramaTotalEpisodesText = $"{Episodes.Count} Episodes";
                    ConfigureDramaCredits(Drama.Title);

                    var firstEp = Episodes.FirstOrDefault();
                    if (firstEp != null)
                    {
                        ActiveEpisodeNumber = firstEp.EpisodeNumber;
                        ActiveVideoTitle = !string.IsNullOrWhiteSpace(firstEp.Title) ? firstEp.Title : $"{Drama.Title} - Episode {firstEp.EpisodeNumber}";
                        ActiveVideoDescription = !string.IsNullOrWhiteSpace(firstEp.Description) ? firstEp.Description : Drama.Description ?? "";
                        ActiveVideoUrl = !string.IsNullOrWhiteSpace(firstEp.VideoUrl) ? firstEp.VideoUrl : "/videos/video_2026-08-04_13-49-33.mp4";
                        ActiveVideoPoster = !string.IsNullOrWhiteSpace(firstEp.ThumbnailUrl) ? firstEp.ThumbnailUrl : Drama.PosterUrl;
                    }
                }
                else
                {
                    ErrorMessage = result?.Message ?? $"Failed to load drama details for ID {Id}.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected async Task PlayEpisode(EpisodeDto ep)
        {
            ActiveEpisodeNumber = ep.EpisodeNumber;
            ActiveVideoTitle = !string.IsNullOrWhiteSpace(ep.Title) ? ep.Title : $"{Drama?.Title} - Episode {ep.EpisodeNumber}";
            ActiveVideoDescription = !string.IsNullOrWhiteSpace(ep.Description) ? ep.Description : Drama?.Description ?? "";
            ActiveVideoUrl = !string.IsNullOrWhiteSpace(ep.VideoUrl) ? ep.VideoUrl : "/videos/video_2026-08-04_13-49-33.mp4";
            ActiveVideoPoster = !string.IsNullOrWhiteSpace(ep.ThumbnailUrl) ? ep.ThumbnailUrl : Drama?.PosterUrl;
            IsPlayingVideo = true;
            StateHasChanged();

            // Smoothly auto-scroll up to the video player so user immediately sees it playing
            await Task.Delay(50);
            try
            {
                await JS.InvokeVoidAsync("eval", @"
                    const el = document.getElementById('videoPlayerBox');
                    if (el) {
                        el.scrollIntoView({ behavior: 'smooth', block: 'start' });
                    }
                ");
            }
            catch { }
        }

        protected async Task StartPlayingFirstEpisode()
        {
            var ep = Episodes.FirstOrDefault(e => e.EpisodeNumber == ActiveEpisodeNumber) ?? Episodes.FirstOrDefault();
            if (ep != null)
            {
                await PlayEpisode(ep);
            }
            else
            {
                IsPlayingVideo = true;
                StateHasChanged();
                await Task.Delay(50);
                try
                {
                    await JS.InvokeVoidAsync("eval", @"
                        const el = document.getElementById('videoPlayerBox');
                        if (el) {
                            el.scrollIntoView({ behavior: 'smooth', block: 'start' });
                        }
                    ");
                }
                catch { }
            }
        }

        protected async Task ScrollToPlayer()
        {
            try
            {
                await JS.InvokeVoidAsync("eval", @"
                    const el = document.getElementById('videoPlayerBox');
                    if (el) {
                        el.scrollIntoView({ behavior: 'smooth', block: 'start' });
                    }
                ");
            }
            catch { }
        }

        protected void StopPlaying()
        {
            IsPlayingVideo = false;
            StateHasChanged();
        }

        protected void GoBack()
        {
            Nav.NavigateTo("/");
        }

        protected void ToggleWatchlist()
        {
            IsWatchlisted = !IsWatchlisted;
            StateHasChanged();
        }

        protected string GetEpisodeThumbnail(EpisodeDto ep)
        {
            if (!string.IsNullOrWhiteSpace(ep.ThumbnailUrl)) return ep.ThumbnailUrl;
            if (!string.IsNullOrWhiteSpace(Drama?.PosterUrl)) return Drama.PosterUrl;
            return "https://upload.wikimedia.org/wikipedia/en/9/9e/KinnPorsche_series_poster.jpg";
        }

        private void ConfigureDramaCredits(string title)
        {
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
                DramaGenres = "Action, Horror, Historical, Mystery";
                DramaAgeRating = "18+";
                DramaLanguage = "Korean";
                DramaStarring = "Ju Ji-hoon, Ryu Seung-ryong, Bae Doona, Kim Sung-kyu";
                DramaCreators = "Kim Eun-hee, Kim Seong-hun";
            }
            else if (title.Contains("Fool In Love", StringComparison.OrdinalIgnoreCase))
            {
                DramaYear = "2026";
                DramaGenres = "Romance, Drama, Comedy";
                DramaAgeRating = "18+";
                DramaLanguage = "Thai";
                DramaStarring = "Bank Artit Tangwiboonpanit, Copter Nuntapong, Guy Wasu";
                DramaCreators = "Stacy Baek, Shui Qiancheng";
            }
            else if (title.Contains("Daung", StringComparison.OrdinalIgnoreCase))
            {
                DramaYear = "2026";
                DramaGenres = "Romance, Comedy, Youth";
                DramaAgeRating = "13+";
                DramaLanguage = "Thai";
                DramaStarring = "TeeTee Wanpichit, Por Suppakarn, Wave Thanaphat, North Chatchapon";
                DramaCreators = "Kla Nathawat Piyanonpong, peachhplease";
            }
            else if (title.Contains("Khem", StringComparison.OrdinalIgnoreCase))
            {
                DramaYear = "2025";
                DramaGenres = "Supernatural, Horror, Mystery, Romance";
                DramaAgeRating = "18+";
                DramaLanguage = "Thai";
                DramaStarring = "Namping Napatsakorn, Keng Harit Buayoi";
                DramaCreators = "Cali, Domundi TV";
            }
            else if (title.Contains("KinnPorsche", StringComparison.OrdinalIgnoreCase))
            {
                DramaYear = "2022";
                DramaGenres = "Action, Romance, Crime";
                DramaAgeRating = "18+";
                DramaLanguage = "Thai";
                DramaStarring = "Mile Phakphum, Apo Nattawin, Bible Sumettikul, Build Jakapan";
                DramaCreators = "Kongkiat Komesiri, Krisda Witthayakhajorndet";
            }
            else if (title.Contains("2gether", StringComparison.OrdinalIgnoreCase))
            {
                DramaYear = "2020";
                DramaGenres = "Romance, Comedy";
                DramaAgeRating = "13+";
                DramaLanguage = "Thai";
                DramaStarring = "Bright Vachirawit, Win Metawin";
                DramaCreators = "Champ Weerachit Thongjila";
            }
            else if (title.Contains("Bad Buddy", StringComparison.OrdinalIgnoreCase))
            {
                DramaYear = "2021";
                DramaGenres = "Romance, Comedy, Youth";
                DramaAgeRating = "13+";
                DramaLanguage = "Thai";
                DramaStarring = "Ohm Pawat, Nanon Korapat";
                DramaCreators = "Backaof Noppharnach";
            }
            else if (title.Contains("Crash Landing", StringComparison.OrdinalIgnoreCase))
            {
                DramaYear = "2019";
                DramaGenres = "Romance, Comedy, Drama";
                DramaAgeRating = "15+";
                DramaLanguage = "Korean";
                DramaStarring = "Hyun Bin, Son Ye-jin, Seo Ji-hye, Kim Jung-hyun";
                DramaCreators = "Park Ji-eun, Lee Jeong-hyo";
            }
            else
            {
                DramaAgeRating = "15+";
                DramaStarring = "Cast Ensemble";
                DramaCreators = "Production Team";
            }
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
                    Description = $"Follow the dramatic events and shocking revelations in Episode {i} of {seriesTitle}.",
                    Duration = 65 + (i % 15),
                    VideoUrl = "/videos/video_2026-08-04_13-49-33.mp4"
                });
            }
            return list;
        }
    }
}
