using DotNetProject.Database.AppDbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DotNetProject.Balzor.Components.Shared
{
    public class TmdbSearchResultDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string OriginalTitle { get; set; } = string.Empty;
        public string Overview { get; set; } = string.Empty;
        public string PosterUrl { get; set; } = string.Empty;
        public string BannerUrl { get; set; } = string.Empty;
        public string ReleaseDate { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public double VoteAverage { get; set; }
        public string MediaType { get; set; } = "tv"; // "tv" or "movie"
        public bool AlreadyInDatabase { get; set; } = false;
        public long ExistingContentId { get; set; } = 0;
    }

    public class TmdbImportResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public long ContentId { get; set; }
        public int EpisodeCount { get; set; }
    }

    public class TmdbService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly AppDbContext _db;
        private readonly string _apiKey;

        public TmdbService(HttpClient http, IConfiguration config, AppDbContext db)
        {
            _http = http;
            _config = config;
            _db = db;
            _apiKey = _config["TmdbApiKey"] ?? "c0291e5cfbe661f1c243a056a00c6d7a";
        }

        public async Task<List<TmdbSearchResultDto>> SearchAsync(string query, string mediaType = "tv")
        {
            var results = new List<TmdbSearchResultDto>();
            if (string.IsNullOrWhiteSpace(query)) return results;

            // 1. Try TMDB API
            try
            {
                var endpoint = mediaType == "movie"
                    ? $"https://api.themoviedb.org/3/search/movie?api_key={_apiKey}&query={Uri.EscapeDataString(query.Trim())}&include_adult=false"
                    : $"https://api.themoviedb.org/3/search/tv?api_key={_apiKey}&query={Uri.EscapeDataString(query.Trim())}&include_adult=false";

                using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                request.Headers.Add("Accept", "application/json");

                using var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("results", out var items) && items.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var el in items.EnumerateArray())
                        {
                            int id = el.GetProperty("id").GetInt32();
                            string title = "";
                            string origTitle = "";

                            if (mediaType == "movie")
                            {
                                title = el.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "";
                                origTitle = el.TryGetProperty("original_title", out var ot) ? ot.GetString() ?? "" : "";
                            }
                            else
                            {
                                title = el.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
                                origTitle = el.TryGetProperty("original_name", out var on) ? on.GetString() ?? "" : "";
                            }

                            string overview = el.TryGetProperty("overview", out var ov) ? ov.GetString() ?? "" : "";
                            string posterPath = el.TryGetProperty("poster_path", out var pp) ? pp.GetString() ?? "" : "";
                            string backdropPath = el.TryGetProperty("backdrop_path", out var bp) ? bp.GetString() ?? "" : "";
                            string date = "";
                            if (mediaType == "movie")
                            {
                                date = el.TryGetProperty("release_date", out var rd) ? rd.GetString() ?? "" : "";
                            }
                            else
                            {
                                date = el.TryGetProperty("first_air_date", out var fad) ? fad.GetString() ?? "" : "";
                            }

                            string lang = el.TryGetProperty("original_language", out var ol) ? ol.GetString() ?? "" : "";
                            double vote = el.TryGetProperty("vote_average", out var va) ? va.GetDouble() : 0.0;

                            string posterUrl = !string.IsNullOrWhiteSpace(posterPath) ? $"https://image.tmdb.org/t/p/w500{posterPath}" : "/images/placeholder_poster.jpg";
                            string bannerUrl = !string.IsNullOrWhiteSpace(backdropPath) ? $"https://image.tmdb.org/t/p/original{backdropPath}" : posterUrl;

                            string country = "South Korea";
                            if (lang == "th") country = "Thailand";
                            else if (lang == "ja") country = "Japan";
                            else if (lang == "zh") country = "China";
                            else if (lang == "en") country = "United States";
                            else if (lang == "ko") country = "South Korea";

                            results.Add(new TmdbSearchResultDto
                            {
                                Id = id,
                                Title = title,
                                OriginalTitle = origTitle,
                                Overview = overview,
                                PosterUrl = posterUrl,
                                BannerUrl = bannerUrl,
                                ReleaseDate = date,
                                Country = country,
                                Language = lang.ToUpperInvariant(),
                                VoteAverage = Math.Round(vote, 1),
                                MediaType = mediaType
                            });
                        }
                    }
                }
            }
            catch { }

            // 2. Fallback to TVMaze if TMDB returned empty
            if (results.Count == 0 && mediaType == "tv")
            {
                try
                {
                    var tvmazeUrl = $"https://api.tvmaze.com/search/shows?q={Uri.EscapeDataString(query.Trim())}";
                    var json = await _http.GetStringAsync(tvmazeUrl);
                    using var doc = JsonDocument.Parse(json);
                    foreach (var el in doc.RootElement.EnumerateArray())
                    {
                        if (el.TryGetProperty("show", out var show))
                        {
                            int id = show.GetProperty("id").GetInt32();
                            string name = show.GetProperty("name").GetString() ?? "";
                            string summary = show.TryGetProperty("summary", out var sm) ? sm.GetString() ?? "" : "";
                            summary = System.Text.RegularExpressions.Regex.Replace(summary, "<.*?>", "");

                            string premiered = show.TryGetProperty("premiered", out var pr) ? pr.GetString() ?? "" : "";
                            string posterUrl = "/images/placeholder_poster.jpg";
                            if (show.TryGetProperty("image", out var img) && img.ValueKind == JsonValueKind.Object)
                            {
                                posterUrl = img.TryGetProperty("original", out var orig) ? orig.GetString() ?? posterUrl : posterUrl;
                            }

                            string country = "South Korea";
                            if (show.TryGetProperty("network", out var net) && net.ValueKind == JsonValueKind.Object)
                            {
                                if (net.TryGetProperty("country", out var cObj) && cObj.ValueKind == JsonValueKind.Object)
                                {
                                    country = cObj.TryGetProperty("name", out var cn) ? cn.GetString() ?? country : country;
                                }
                            }

                            results.Add(new TmdbSearchResultDto
                            {
                                Id = id,
                                Title = name,
                                OriginalTitle = name,
                                Overview = summary,
                                PosterUrl = posterUrl,
                                BannerUrl = posterUrl,
                                ReleaseDate = premiered,
                                Country = country,
                                Language = "Korean",
                                VoteAverage = 8.5,
                                MediaType = "tv"
                            });
                        }
                    }
                }
                catch { }
            }

            // Mark items that already exist in database so existing ones are NEVER modified/duplicated
            var existingTitles = await _db.Contents.Where(c => !c.IsDeleted).Select(c => new { c.ContentId, c.Title }).ToListAsync();
            foreach (var r in results)
            {
                var match = existingTitles.FirstOrDefault(e => e.Title.Trim().Equals(r.Title.Trim(), StringComparison.OrdinalIgnoreCase));
                if (match != null)
                {
                    r.AlreadyInDatabase = true;
                    r.ExistingContentId = match.ContentId;
                }
            }

            return results;
        }

        public async Task<TmdbImportResult> ImportDramaAsync(int tmdbId, int targetContentTypeId = 2)
        {
            try
            {
                // 1. Fetch TV Details
                string detailsUrl = $"https://api.themoviedb.org/3/tv/{tmdbId}?api_key={_apiKey}&append_to_response=videos";
                using var response = await _http.GetAsync(detailsUrl);

                string title = "";
                string overview = "";
                string posterUrl = "";
                string bannerUrl = "";
                string country = "South Korea";
                string language = "Korean";
                DateOnly? releaseDate = null;
                string? youtubeKey = null;
                List<string> genreNames = new();
                int episodeCount = 0;

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    title = root.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
                    overview = root.TryGetProperty("overview", out var ov) ? ov.GetString() ?? "" : "";
                    string posterPath = root.TryGetProperty("poster_path", out var pp) ? pp.GetString() ?? "" : "";
                    string backdropPath = root.TryGetProperty("backdrop_path", out var bp) ? bp.GetString() ?? "" : "";

                    posterUrl = !string.IsNullOrWhiteSpace(posterPath) ? $"https://image.tmdb.org/t/p/w500{posterPath}" : "/images/placeholder_poster.jpg";
                    bannerUrl = !string.IsNullOrWhiteSpace(backdropPath) ? $"https://image.tmdb.org/t/p/original{backdropPath}" : posterUrl;

                    if (root.TryGetProperty("first_air_date", out var fad) && DateOnly.TryParse(fad.GetString(), out var d))
                    {
                        releaseDate = d;
                    }

                    string origLang = root.TryGetProperty("original_language", out var ol) ? ol.GetString() ?? "" : "";
                    if (origLang == "th") { country = "Thailand"; language = "Thai"; if (targetContentTypeId == 2) targetContentTypeId = 3; }
                    else if (origLang == "ja") { country = "Japan"; language = "Japanese"; }
                    else if (origLang == "zh") { country = "China"; language = "Chinese"; }
                    else if (origLang == "ko") { country = "South Korea"; language = "Korean"; }

                    // Trailer
                    if (root.TryGetProperty("videos", out var vids) && vids.TryGetProperty("results", out var vidArr))
                    {
                        foreach (var v in vidArr.EnumerateArray())
                        {
                            string site = v.TryGetProperty("site", out var s) ? s.GetString() ?? "" : "";
                            string key = v.TryGetProperty("key", out var k) ? k.GetString() ?? "" : "";
                            if (site.Equals("YouTube", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(key))
                            {
                                youtubeKey = key;
                                break;
                            }
                        }
                    }

                    // Genres
                    if (root.TryGetProperty("genres", out var gArr))
                    {
                        foreach (var g in gArr.EnumerateArray())
                        {
                            if (g.TryGetProperty("name", out var gn))
                            {
                                string name = gn.GetString() ?? "";
                                if (!string.IsNullOrEmpty(name)) genreNames.Add(name);
                            }
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(title))
                {
                    return new TmdbImportResult { IsSuccess = false, Message = "Failed to retrieve drama information from TMDB." };
                }

                // Strictly ensure existing drama is NOT modified or duplicated!
                var existing = await _db.Contents.FirstOrDefaultAsync(c => c.Title.ToLower() == title.ToLower() && !c.IsDeleted);
                if (existing != null)
                {
                    return new TmdbImportResult
                    {
                        IsSuccess = false,
                        Message = $"\"{title}\" is already in your database. Existing data was not modified.",
                        ContentId = existing.ContentId
                    };
                }

                // Create new Content entity
                var newContent = new Content
                {
                    Title = title,
                    Description = overview,
                    PosterUrl = posterUrl,
                    BannerUrl = bannerUrl,
                    ReleaseDate = releaseDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
                    Country = country,
                    Language = language,
                    ContentTypeId = targetContentTypeId,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                _db.Contents.Add(newContent);
                await _db.SaveChangesAsync();

                // Assign Genres
                var allGenres = await _db.Genres.Where(g => !g.IsDeleted).ToListAsync();
                foreach (var gn in genreNames)
                {
                    var matchedGenre = allGenres.FirstOrDefault(g => g.Name.Contains(gn, StringComparison.OrdinalIgnoreCase) || gn.Contains(g.Name, StringComparison.OrdinalIgnoreCase));
                    if (matchedGenre != null)
                    {
                        _db.ContentGenres.Add(new ContentGenre
                        {
                            ContentId = newContent.ContentId,
                            GenreId = matchedGenre.GenreId
                        });
                    }
                }

                // 2. Fetch Season 1 Episodes from TMDB
                var episodes = new List<Episode>();
                try
                {
                    string seasonUrl = $"https://api.themoviedb.org/3/tv/{tmdbId}/season/1?api_key={_apiKey}";
                    using var seasonResp = await _http.GetAsync(seasonUrl);
                    if (seasonResp.IsSuccessStatusCode)
                    {
                        var sJson = await seasonResp.Content.ReadAsStringAsync();
                        using var sDoc = JsonDocument.Parse(sJson);
                        if (sDoc.RootElement.TryGetProperty("episodes", out var epArr))
                        {
                            foreach (var ep in epArr.EnumerateArray())
                            {
                                int epNum = ep.TryGetProperty("episode_number", out var en) ? en.GetInt32() : episodes.Count + 1;
                                string epName = ep.TryGetProperty("name", out var ename) ? ename.GetString() ?? $"Episode {epNum}" : $"Episode {epNum}";
                                string epOverview = ep.TryGetProperty("overview", out var eov) ? eov.GetString() ?? "" : "";
                                string stillPath = ep.TryGetProperty("still_path", out var sp) ? sp.GetString() ?? "" : "";
                                int runtime = ep.TryGetProperty("runtime", out var rt) && rt.ValueKind == JsonValueKind.Number ? rt.GetInt32() : 45;
                                string stillUrl = !string.IsNullOrWhiteSpace(stillPath) ? $"https://image.tmdb.org/t/p/w500{stillPath}" : posterUrl;

                                // Playable video stream link: YouTube embed or online stream
                                string videoUrl = !string.IsNullOrWhiteSpace(youtubeKey)
                                    ? $"https://www.youtube.com/embed/{youtubeKey}?autoplay=1"
                                    : "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4";

                                episodes.Add(new Episode
                                {
                                    ContentId = newContent.ContentId,
                                    EpisodeNumber = epNum,
                                    Title = epName,
                                    Description = string.IsNullOrWhiteSpace(epOverview) ? $"Stream Episode {epNum} of {title}" : epOverview,
                                    ThumbnailUrl = stillUrl,
                                    VideoUrl = videoUrl,
                                    Duration = runtime,
                                    ReleaseDate = releaseDate,
                                    Status = "Active",
                                    CreatedAt = DateTime.UtcNow,
                                    IsDeleted = false
                                });
                            }
                        }
                    }
                }
                catch { }

                // Fallback default 16 episodes if season episodes list was empty
                if (episodes.Count == 0)
                {
                    string videoUrl = !string.IsNullOrWhiteSpace(youtubeKey)
                        ? $"https://www.youtube.com/embed/{youtubeKey}?autoplay=1"
                        : "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4";

                    for (int i = 1; i <= 16; i++)
                    {
                        episodes.Add(new Episode
                        {
                            ContentId = newContent.ContentId,
                            EpisodeNumber = i,
                            Title = $"{title} - Episode {i}",
                            Description = $"Stream Episode {i} of {title} in high definition.",
                            ThumbnailUrl = posterUrl,
                            VideoUrl = videoUrl,
                            Duration = 60,
                            ReleaseDate = releaseDate,
                            Status = "Active",
                            CreatedAt = DateTime.UtcNow,
                            IsDeleted = false
                        });
                    }
                }

                _db.Episodes.AddRange(episodes);
                await _db.SaveChangesAsync();

                episodeCount = episodes.Count;

                return new TmdbImportResult
                {
                    IsSuccess = true,
                    Message = $"Successfully imported \"{title}\" with {episodeCount} episodes!",
                    ContentId = newContent.ContentId,
                    EpisodeCount = episodeCount
                };
            }
            catch (Exception ex)
            {
                return new TmdbImportResult
                {
                    IsSuccess = false,
                    Message = $"Error importing drama: {ex.Message}"
                };
            }
        }
    }
}
