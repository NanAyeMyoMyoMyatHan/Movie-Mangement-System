using DotNetProject.Balzor.Components.Shared;
using DotNetProject.Balzor.Helpers;
using DotNetProject.Domain.Features.Contents;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DotNetProject.Balzor.Components.Pages.Dramas
{
    public partial class DramaDetail
    {
        [Inject]
        private HttpClientService ApiClient { get; set; } = default!;

        [Inject]
        private NavigationManager Nav { get; set; } = default!;

        [Inject]
        private IJSRuntime JS { get; set; } = default!;

        [Inject]
        private WatchLaterService WatchLaterService { get; set; } = default!;

        [Inject]
        private FavoriteService FavoriteService { get; set; } = default!;

        [Inject]
        private WatchHistoryService WatchHistoryService { get; set; } = default!;

        [Inject]
        public AuthService AuthService { get; set; } = default!;

        [Inject]
        private DotNetProject.Database.AppDbModels.AppDbContext Db { get; set; } = default!;

        [Parameter]
        public long Id { get; set; }

        [SupplyParameterFromQuery(Name = "ep")]
        public int? QueryEpisode { get; set; }

        [SupplyParameterFromQuery(Name = "autoplay")]
        public bool? Autoplay { get; set; }

        protected ContentDetailDto? Drama { get; set; }
        protected List<EpisodeDto> Episodes { get; set; } = new();
        protected bool IsLoading { get; set; } = true;
        protected string? ErrorMessage { get; set; }

        protected bool IsPlayingVideo { get; set; } = false;
        protected bool IsWatchlisted { get; set; } = false;
        protected bool IsFavorited { get; set; } = false;
        protected bool UseDirectStream { get; set; } = false;
        protected bool UseDemoStream { get; set; } = false;
        protected long ActiveEpisodeId { get; set; } = 0;

        protected bool ShowQuickUrlModal { get; set; } = false;
        protected string QuickModalVideoUrl { get; set; } = "";
        protected string? QuickModalMessage { get; set; }
        protected bool IsSavingQuickUrl { get; set; } = false;

        protected bool ShowTelegramQrModal { get; set; } = false;

        protected void OpenTelegramQrModal()
        {
            ShowTelegramQrModal = true;
            StateHasChanged();
        }

        protected void CloseTelegramQrModal()
        {
            ShowTelegramQrModal = false;
            StateHasChanged();
        }

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
                IsWatchlisted = await WatchLaterService.IsInWatchLaterAsync(Id, true);
                IsFavorited = await FavoriteService.IsFavoriteAsync(Id, true);
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

                    EpisodeDto? selectedEp = null;
                    if (QueryEpisode.HasValue)
                    {
                        selectedEp = Episodes.FirstOrDefault(e => e.EpisodeNumber == QueryEpisode.Value);
                    }
                    selectedEp ??= Episodes.FirstOrDefault();

                    if (selectedEp != null)
                    {
                        ActiveEpisodeNumber = selectedEp.EpisodeNumber;
                        ActiveEpisodeId = selectedEp.EpisodeId;
                        ActiveVideoTitle = !string.IsNullOrWhiteSpace(selectedEp.Title) ? selectedEp.Title : $"{Drama.Title} - Episode {selectedEp.EpisodeNumber}";
                        ActiveVideoDescription = !string.IsNullOrWhiteSpace(selectedEp.Description) ? selectedEp.Description : Drama.Description ?? "";
                        ActiveVideoUrl = selectedEp.VideoUrl ?? "";
                        ActiveVideoPoster = !string.IsNullOrWhiteSpace(selectedEp.ThumbnailUrl) ? selectedEp.ThumbnailUrl : Drama.PosterUrl;

                        if (Autoplay == true && AuthService.IsAuthenticated())
                        {
                            await PlayEpisode(selectedEp);
                        }
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

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    await JS.InvokeVoidAsync("eval", "window.scrollTo(0, 0);");
                }
                catch { }
            }
        }

        protected static string? CleanPoster(string? url, string? title)
        {
            if (title != null && (title.Contains("Queen", StringComparison.OrdinalIgnoreCase) || title.Contains("Tears", StringComparison.OrdinalIgnoreCase)))
                return "https://upload.wikimedia.org/wikipedia/en/e/e2/Queen_of_Tears_poster.png";
            if (title != null && title.Contains("Kingdom", StringComparison.OrdinalIgnoreCase))
                return "/images/kingdom.jpg";
            if (title != null && title.Contains("Daung", StringComparison.OrdinalIgnoreCase))
                return "/images/daung_with_you.jpg";
            if (title != null && title.Contains("Khem", StringComparison.OrdinalIgnoreCase))
                return "/images/khemjira.jpg";

            if (string.IsNullOrWhiteSpace(url)) return null;
            if (url.Contains("static.tvmaze.com") && url.Contains("1245252"))
                return "/images/kingdom.jpg";
            if (url.Contains("static.tvmaze.com") && url.Contains("1528703"))
                return "/images/daung_with_you.jpg";
            if (url.Contains("static.tvmaze.com") && url.Contains("1454779"))
                return "/images/khemjira.jpg";
            return url;
        }

        protected bool IsYoutubeUrl(string? url)
        {
            return MediaStreamHelper.IsYouTube(url);
        }

        protected string GetYouTubeEmbedUrl(string? url)
        {
            return MediaStreamHelper.ToYouTubeEmbedUrl(url, autoplay: true) ?? (url ?? "");
        }

        protected void TogglePlayerMode()
        {
            UseDirectStream = !UseDirectStream;
            StateHasChanged();
        }

        protected bool HasPlayableVideo()
        {
            if (UseDemoStream) return true;
            return !string.IsNullOrWhiteSpace(ActiveVideoUrl);
        }

        protected string? GetTelegramLink()
        {
            if (Drama?.Title != null && (Drama.Title.Contains("Queen", StringComparison.OrdinalIgnoreCase) || Drama.Title.Contains("Tears", StringComparison.OrdinalIgnoreCase)))
            {
                return "https://t.me/WEAREALLTRYINGHEREMM";
            }
            return null;
        }

        protected void PlayTestStream()
        {
            UseDemoStream = true;
            StateHasChanged();
        }

        protected void OpenQuickUrlModal()
        {
            QuickModalVideoUrl = ActiveVideoUrl ?? "";
            QuickModalMessage = null;
            ShowQuickUrlModal = true;
            StateHasChanged();
        }

        protected void CloseQuickUrlModal()
        {
            ShowQuickUrlModal = false;
            QuickModalMessage = null;
        }

        protected async Task SaveQuickUrlAsync()
        {
            if (ActiveEpisodeId <= 0) return;
            IsSavingQuickUrl = true;
            QuickModalMessage = null;

            try
            {
                var dbEp = await Db.Episodes.FindAsync(ActiveEpisodeId);
                if (dbEp != null)
                {
                    dbEp.VideoUrl = (QuickModalVideoUrl ?? "").Trim();
                    dbEp.UpdatedAt = DateTime.UtcNow;
                    await Db.SaveChangesAsync();

                    var inMem = Episodes.FirstOrDefault(e => e.EpisodeId == ActiveEpisodeId);
                    if (inMem != null)
                    {
                        inMem.VideoUrl = dbEp.VideoUrl;
                    }
                    ActiveVideoUrl = dbEp.VideoUrl;
                    UseDemoStream = false;
                    QuickModalMessage = "Saved successfully!";
                    await Task.Delay(400);
                    ShowQuickUrlModal = false;
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                QuickModalMessage = "Failed: " + ex.Message;
            }
            finally
            {
                IsSavingQuickUrl = false;
            }
        }

        protected string GetPlaybackUrl()
        {
            if (UseDemoStream)
            {
                return "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4";
            }
            if (IsYoutubeUrl(ActiveVideoUrl) && UseDirectStream)
            {
                // Direct stream fallback for regions/networks where YouTube embeds are blocked
                return "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4";
            }
            if (!string.IsNullOrWhiteSpace(ActiveVideoUrl))
            {
                return ActiveVideoUrl;
            }
            if (Drama?.Title != null && Drama.Title.Contains("Alchemy", StringComparison.OrdinalIgnoreCase))
            {
                return "/videos/video_2026-08-04_13-49-33.mp4";
            }
            return string.Empty;
        }

        protected async Task PlayEpisode(EpisodeDto ep)
        {
            if (!AuthService.IsAuthenticated())
            {
                Nav.NavigateTo($"/login?returnUrl=/drama/{Id}?ep={ep.EpisodeNumber}");
                return;
            }

            ActiveEpisodeNumber = ep.EpisodeNumber;
            ActiveEpisodeId = ep.EpisodeId;
            ActiveVideoTitle = !string.IsNullOrWhiteSpace(ep.Title) ? ep.Title : $"{Drama?.Title} - Episode {ep.EpisodeNumber}";
            ActiveVideoDescription = !string.IsNullOrWhiteSpace(ep.Description) ? ep.Description : Drama?.Description ?? "";
            ActiveVideoUrl = ep.VideoUrl ?? "";
            ActiveVideoPoster = !string.IsNullOrWhiteSpace(ep.ThumbnailUrl) ? ep.ThumbnailUrl : Drama?.PosterUrl;
            IsPlayingVideo = true;
            UseDemoStream = false;
            StateHasChanged();

            // Record into Continue Watching history
            if (Drama != null)
            {
                await WatchHistoryService.RecordWatchAsync(
                    Drama.ContentId,
                    Drama.Title,
                    Drama.Description,
                    !string.IsNullOrWhiteSpace(ep.ThumbnailUrl) ? ep.ThumbnailUrl : Drama.PosterUrl,
                    ep.EpisodeNumber,
                    Episodes.Count > 0 ? Episodes.Count : 16,
                    ep.VideoUrl,
                    isContent: true,
                    progressPercentage: 45
                );
            }

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
            if (!AuthService.IsAuthenticated())
            {
                Nav.NavigateTo($"/login?returnUrl=/drama/{Id}");
                return;
            }

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

        protected async Task ToggleWatchlist()
        {
            if (!AuthService.IsAuthenticated())
            {
                Nav.NavigateTo($"/login?returnUrl=/drama/{Id}");
                return;
            }

            if (Drama == null) return;

            var item = new WatchLaterItem
            {
                Id = Drama.ContentId,
                IsContent = true,
                Title = Drama.Title,
                PosterUrl = Drama.PosterUrl,
                Category = DramaLanguage.Contains("Thai", StringComparison.OrdinalIgnoreCase) ? "Thai BL" : "Korean Drama",
                Description = Drama.Description,
                TotalEpisodes = Episodes.Count > 0 ? Episodes.Count : 16,
                CurrentEpisode = ActiveEpisodeNumber,
                Status = IsPlayingVideo ? "Watching" : "Want to Watch",
                AddedAt = DateTime.UtcNow
            };

            IsWatchlisted = await WatchLaterService.ToggleWatchLaterAsync(item);
            StateHasChanged();
        }

        protected async Task ToggleFavorite()
        {
            if (!AuthService.IsAuthenticated())
            {
                Nav.NavigateTo($"/login?returnUrl=/drama/{Id}");
                return;
            }

            if (Drama == null) return;

            var item = new FavoriteItem
            {
                Id = Drama.ContentId,
                IsContent = true,
                Title = Drama.Title,
                PosterUrl = Drama.PosterUrl,
                Category = DramaLanguage.Contains("Thai", StringComparison.OrdinalIgnoreCase) ? "Thai BL" : "Korean Drama",
                Description = Drama.Description,
                Year = DramaYear,
                Rating = 9.4,
                TotalEpisodes = Episodes.Count > 0 ? Episodes.Count : 16,
                AddedAt = DateTime.UtcNow
            };

            IsFavorited = await FavoriteService.ToggleFavoriteAsync(item);
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

        private static string GetFallbackYoutubeUrl(string seriesTitle)
        {
            if (seriesTitle.Contains("Your Name", StringComparison.OrdinalIgnoreCase))
                return "https://www.youtube.com/watch?v=xU47nhruN-Q";
            if (seriesTitle.Contains("Spirited", StringComparison.OrdinalIgnoreCase))
                return "https://www.youtube.com/watch?v=ByXuk9QqQkk";
            if (seriesTitle.Contains("Busan", StringComparison.OrdinalIgnoreCase))
                return "https://www.youtube.com/watch?v=pyWuHv2-Abk";
            if (seriesTitle.Contains("Parasite", StringComparison.OrdinalIgnoreCase))
                return "https://www.youtube.com/watch?v=5xH0RwpEbSU";
            if (seriesTitle.Contains("KinnPorsche", StringComparison.OrdinalIgnoreCase))
                return "https://www.youtube.com/watch?v=1F_U3mN8rJ0";
            if (seriesTitle.Contains("Bad Buddy", StringComparison.OrdinalIgnoreCase))
                return "https://www.youtube.com/watch?v=0kG7uR0qU3Q";
            if (seriesTitle.Contains("2gether", StringComparison.OrdinalIgnoreCase))
                return "https://www.youtube.com/watch?v=U18l74qGiaI";
            if (seriesTitle.Contains("TharnType", StringComparison.OrdinalIgnoreCase))
                return "https://www.youtube.com/watch?v=w6-bNlJ9s_A";
            if (seriesTitle.Contains("Thousand Stars", StringComparison.OrdinalIgnoreCase))
                return "https://www.youtube.com/watch?v=0d80m9_y9Yc";
            if (seriesTitle.Contains("Crash Landing", StringComparison.OrdinalIgnoreCase))
                return "https://www.youtube.com/watch?v=GVQGWgeVc4k";
            if (seriesTitle.Contains("Lovely Runner", StringComparison.OrdinalIgnoreCase))
                return "https://www.youtube.com/watch?v=0-x2w4U7D9g";
            if (seriesTitle.Contains("Goblin", StringComparison.OrdinalIgnoreCase))
                return "https://www.youtube.com/watch?v=8AcNWVpdTHs";
            if (seriesTitle.Contains("Queen", StringComparison.OrdinalIgnoreCase))
                return "https://www.youtube.com/watch?v=4b7_4sN9Uvg";
            if (seriesTitle.Contains("Vincenzo", StringComparison.OrdinalIgnoreCase))
                return "https://www.youtube.com/watch?v=_J8tYxSB_LQ";
            if (seriesTitle.Contains("The Glory", StringComparison.OrdinalIgnoreCase))
                return "https://www.youtube.com/watch?v=tqVVddp_G9Y";

            return "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
        }

        private List<EpisodeDto> GenerateFallbackEpisodes(string seriesTitle, int count)
        {
            var fallbackUrl = GetFallbackYoutubeUrl(seriesTitle);
            var list = new List<EpisodeDto>();
            for (int i = 1; i <= count; i++)
            {
                list.Add(new EpisodeDto
                {
                    EpisodeNumber = i,
                    Title = $"Episode {i}",
                    Description = $"Follow the dramatic events and shocking revelations in Episode {i} of {seriesTitle}.",
                    Duration = 65 + (i % 15),
                    VideoUrl = fallbackUrl
                });
            }
            return list;
        }
    }
}
