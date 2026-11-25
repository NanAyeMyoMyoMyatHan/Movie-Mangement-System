using DotNetProject.Database.AppDbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
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
        public bool IsFromTmdb { get; set; } = true;
        public bool AlreadyInDatabase { get; set; } = false;
        public long ExistingContentId { get; set; } = 0;

        public int? ReleaseYear =>
            DateOnly.TryParse(ReleaseDate, out var d) ? d.Year
            : ReleaseDate.Length >= 4 && int.TryParse(ReleaseDate[..4], out var y) ? y
            : null;
    }

    public class TmdbMovieDetailsDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string OriginalTitle { get; set; } = string.Empty;
        public string? Genre { get; set; }
        public List<string> Genres { get; set; } = new();
        public int? ReleaseYear { get; set; }
        public DateOnly? ReleaseDate { get; set; }
        public string? Description { get; set; }
        public string? PosterUrl { get; set; }
        public string? BannerUrl { get; set; }
        public string Country { get; set; } = "United States";
        public string Language { get; set; } = "English";
        public int? Runtime { get; set; }
        public double VoteAverage { get; set; }
    }

    public sealed record TmdbLookupResponse<T>(bool IsSuccess, T? Data, string? ErrorMessage);

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

        public async Task<TmdbLookupResponse<List<TmdbSearchResultDto>>> SearchMovieSuggestionsAsync(
            string query,
            int maxResults = 6,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new TmdbLookupResponse<List<TmdbSearchResultDto>>(true, new List<TmdbSearchResultDto>(), null);

            try
            {
                var endpoint = $"https://api.themoviedb.org/3/search/movie?api_key={_apiKey}&query={Uri.EscapeDataString(query.Trim())}&include_adult=false";
                using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                request.Headers.Add("Accept", "application/json");

                using var response = await _http.SendAsync(request, cancellationToken);
                var body = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                    return new TmdbLookupResponse<List<TmdbSearchResultDto>>(false, new List<TmdbSearchResultDto>(), FormatTmdbError(response.StatusCode, body));

                using var doc = JsonDocument.Parse(body);
                var results = ParseSearchResults(doc.RootElement, "movie", maxResults);
                return new TmdbLookupResponse<List<TmdbSearchResultDto>>(true, results, null);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return new TmdbLookupResponse<List<TmdbSearchResultDto>>(false, new List<TmdbSearchResultDto>(), $"Could not reach TMDB: {ex.Message}");
            }
        }

        public async Task<TmdbLookupResponse<TmdbMovieDetailsDto>> GetMovieByIdAsync(
            int tmdbId,
            CancellationToken cancellationToken = default)
        {
            if (tmdbId <= 0)
                return new TmdbLookupResponse<TmdbMovieDetailsDto>(false, null, "Please enter a valid positive TMDB ID.");

            try
            {
                var url = $"https://api.themoviedb.org/3/movie/{tmdbId}?api_key={_apiKey}";
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Accept", "application/json");

                using var response = await _http.SendAsync(request, cancellationToken);
                var body = await response.Content.ReadAsStringAsync(cancellationToken);

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return new TmdbLookupResponse<TmdbMovieDetailsDto>(true, null, null);

                if (!response.IsSuccessStatusCode)
                    return new TmdbLookupResponse<TmdbMovieDetailsDto>(false, null, FormatTmdbError(response.StatusCode, body));

                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                var title = GetJsonString(root, "title") ?? string.Empty;
                if (string.IsNullOrWhiteSpace(title))
                    return new TmdbLookupResponse<TmdbMovieDetailsDto>(true, null, null);

                var id = GetJsonInt(root, "id") ?? tmdbId;
                var origTitle = GetJsonString(root, "original_title") ?? title;
                var overview = GetJsonString(root, "overview");
                var posterPath = GetJsonString(root, "poster_path");
                var backdropPath = GetJsonString(root, "backdrop_path");
                var releaseDateStr = GetJsonString(root, "release_date");
                var origLang = GetJsonString(root, "original_language");
                var runtime = GetJsonInt(root, "runtime");
                var vote = GetJsonDouble(root, "vote_average");

                DateOnly? releaseDate = DateOnly.TryParse(releaseDateStr, out var parsedDate) ? parsedDate : null;
                int? releaseYear = releaseDate?.Year ??
                    (!string.IsNullOrWhiteSpace(releaseDateStr) && releaseDateStr.Length >= 4 && int.TryParse(releaseDateStr[..4], out var y)
                        ? y
                        : null);

                var genres = new List<string>();
                if (root.TryGetProperty("genres", out var gArr) && gArr.ValueKind == JsonValueKind.Array)
                {
                    foreach (var g in gArr.EnumerateArray())
                    {
                        var name = GetJsonString(g, "name");
                        if (!string.IsNullOrWhiteSpace(name))
                            genres.Add(name.Trim());
                    }
                }

                var (country, language) = MapLanguage(origLang, "United States", "English");
                if (root.TryGetProperty("production_countries", out var cArr) && cArr.ValueKind == JsonValueKind.Array)
                {
                    foreach (var c in cArr.EnumerateArray())
                    {
                        var cName = GetJsonString(c, "name");
                        if (!string.IsNullOrWhiteSpace(cName))
                        {
                            country = cName == "United States of America" ? "United States" : cName;
                            break;
                        }
                    }
                }

                var posterUrl = !string.IsNullOrWhiteSpace(posterPath)
                    ? $"https://image.tmdb.org/t/p/w500{posterPath}"
                    : null;
                var bannerUrl = !string.IsNullOrWhiteSpace(backdropPath)
                    ? $"https://image.tmdb.org/t/p/original{backdropPath}"
                    : posterUrl;

                var details = new TmdbMovieDetailsDto
                {
                    Id = id,
                    Title = title.Trim(),
                    OriginalTitle = origTitle.Trim(),
                    Genres = genres,
                    Genre = genres.Count > 0 ? string.Join(", ", genres) : null,
                    ReleaseDate = releaseDate,
                    ReleaseYear = releaseYear,
                    Description = string.IsNullOrWhiteSpace(overview) ? null : overview.Trim(),
                    PosterUrl = posterUrl,
                    BannerUrl = bannerUrl,
                    Country = country,
                    Language = language,
                    Runtime = runtime is > 0 ? runtime : null,
                    VoteAverage = Math.Round(vote, 1)
                };

                return new TmdbLookupResponse<TmdbMovieDetailsDto>(true, details, null);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return new TmdbLookupResponse<TmdbMovieDetailsDto>(false, null, $"Could not reach TMDB: {ex.Message}");
            }
        }

        public async Task<TmdbLookupResponse<List<TmdbSearchResultDto>>> SearchAsync(string query, string mediaType = "tv")
        {
            var results = new List<TmdbSearchResultDto>();
            if (string.IsNullOrWhiteSpace(query))
                return new TmdbLookupResponse<List<TmdbSearchResultDto>>(true, results, null);

            string? tmdbError = null;

            // 1. Try TMDB API
            try
            {
                var endpoint = mediaType == "movie"
                    ? $"https://api.themoviedb.org/3/search/movie?api_key={_apiKey}&query={Uri.EscapeDataString(query.Trim())}&include_adult=false"
                    : $"https://api.themoviedb.org/3/search/tv?api_key={_apiKey}&query={Uri.EscapeDataString(query.Trim())}&include_adult=false";

                using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                request.Headers.Add("Accept", "application/json");

                using var response = await _http.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(body);
                    results = ParseSearchResults(doc.RootElement, mediaType);
                }
                else
                {
                    tmdbError = FormatTmdbError(response.StatusCode, body);
                }
            }
            catch (Exception ex)
            {
                tmdbError = $"Could not reach TMDB: {ex.Message}";
            }

            // 2. Fallback to TVMaze if TMDB returned empty for TV
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
                            string summary = GetJsonString(show, "summary") ?? "";
                            summary = System.Text.RegularExpressions.Regex.Replace(summary, "<.*?>", "");

                            string premiered = GetJsonString(show, "premiered") ?? "";
                            string posterUrl = "/images/placeholder_poster.jpg";
                            if (show.TryGetProperty("image", out var img) && img.ValueKind == JsonValueKind.Object)
                            {
                                posterUrl = GetJsonString(img, "original") ?? posterUrl;
                            }

                            string country = "South Korea";
                            if (show.TryGetProperty("network", out var net) && net.ValueKind == JsonValueKind.Object)
                            {
                                if (net.TryGetProperty("country", out var cObj) && cObj.ValueKind == JsonValueKind.Object)
                                {
                                    country = GetJsonString(cObj, "name") ?? country;
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
                                MediaType = "tv",
                                IsFromTmdb = false
                            });
                        }
                    }
                }
                catch { }
            }

            if (results.Count == 0 && !string.IsNullOrWhiteSpace(tmdbError))
            {
                return new TmdbLookupResponse<List<TmdbSearchResultDto>>(false, results, tmdbError);
            }

            // Mark items that already exist in database so existing ones are NEVER modified/duplicated
            var existingContents = await _db.Contents
                .Where(c => !c.IsDeleted && (mediaType == "movie" ? c.ContentTypeId == 1 : c.ContentTypeId != 1))
                .Select(c => new { c.ContentId, c.Title, c.TmdbId })
                .ToListAsync();

            foreach (var r in results)
            {
                var match = existingContents.FirstOrDefault(e =>
                    (r.IsFromTmdb && e.TmdbId.HasValue && e.TmdbId.Value == r.Id) ||
                    e.Title.Trim().Equals(r.Title.Trim(), StringComparison.OrdinalIgnoreCase));
                if (match != null)
                {
                    r.AlreadyInDatabase = true;
                    r.ExistingContentId = match.ContentId;
                }
            }

            return new TmdbLookupResponse<List<TmdbSearchResultDto>>(true, results, tmdbError);
        }

        public async Task<TmdbImportResult> ImportMovieAsync(int tmdbId)
        {
            try
            {
                var lookup = await GetMovieByIdAsync(tmdbId);
                if (!lookup.IsSuccess)
                {
                    return new TmdbImportResult { IsSuccess = false, Message = lookup.ErrorMessage ?? "Failed to retrieve movie information from TMDB." };
                }

                var details = lookup.Data;
                if (details == null || string.IsNullOrWhiteSpace(details.Title))
                {
                    return new TmdbImportResult { IsSuccess = false, Message = "Failed to retrieve movie information from TMDB." };
                }

                var title = details.Title.Length > 255 ? details.Title[..255] : details.Title;

                var existing = await _db.Contents.FirstOrDefaultAsync(c =>
                    !c.IsDeleted &&
                    c.ContentTypeId == 1 &&
                    (c.TmdbId == tmdbId || c.Title.ToLower() == title.ToLower()));

                if (existing != null)
                {
                    return new TmdbImportResult
                    {
                        IsSuccess = false,
                        Message = $"\"{title}\" is already in your database. Existing data was not modified.",
                        ContentId = existing.ContentId
                    };
                }

                var newContent = new Content
                {
                    ContentTypeId = 1,
                    Title = title,
                    Description = details.Description,
                    PosterUrl = details.PosterUrl ?? "/images/placeholder_poster.jpg",
                    BannerUrl = details.BannerUrl ?? details.PosterUrl ?? "/images/placeholder_poster.jpg",
                    ReleaseDate = details.ReleaseDate,
                    Country = details.Country,
                    Language = details.Language,
                    Status = "Completed",
                    TmdbId = details.Id,
                    IsManual = false,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                _db.Contents.Add(newContent);
                await _db.SaveChangesAsync();

                if (details.Genres.Count > 0)
                {
                    var allGenres = await _db.Genres.Where(g => !g.IsDeleted).ToListAsync();
                    foreach (var gn in details.Genres.Distinct(StringComparer.OrdinalIgnoreCase))
                    {
                        var genre = allGenres.FirstOrDefault(g => g.Name.Equals(gn, StringComparison.OrdinalIgnoreCase));
                        if (genre == null)
                        {
                            genre = new Genre { Name = gn, CreatedAt = DateTime.UtcNow, IsDeleted = false };
                            _db.Genres.Add(genre);
                            await _db.SaveChangesAsync();
                            allGenres.Add(genre);
                        }

                        _db.ContentGenres.Add(new ContentGenre
                        {
                            ContentId = newContent.ContentId,
                            GenreId = genre.GenreId,
                            IsDeleted = false
                        });
                    }
                }

                _db.Episodes.Add(new Episode
                {
                    ContentId = newContent.ContentId,
                    EpisodeNumber = 1,
                    Title = "Full Movie",
                    Description = details.Description,
                    ThumbnailUrl = newContent.PosterUrl,
                    VideoUrl = string.Empty,
                    Duration = details.Runtime ?? 120,
                    ReleaseDate = details.ReleaseDate,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                });
                await _db.SaveChangesAsync();

                return new TmdbImportResult
                {
                    IsSuccess = true,
                    Message = $"Successfully imported movie \"{title}\" from TMDB!",
                    ContentId = newContent.ContentId,
                    EpisodeCount = 1
                };
            }
            catch (Exception ex)
            {
                return new TmdbImportResult
                {
                    IsSuccess = false,
                    Message = $"Error importing movie: {ex.Message}"
                };
            }
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
                List<string> genreNames = new();
                int episodeCount = 0;

                if (!response.IsSuccessStatusCode)
                {
                    var errBody = await response.Content.ReadAsStringAsync();
                    return new TmdbImportResult { IsSuccess = false, Message = FormatTmdbError(response.StatusCode, errBody) };
                }

                var json = await response.Content.ReadAsStringAsync();
                using (var doc = JsonDocument.Parse(json))
                {
                    var root = doc.RootElement;

                    title = GetJsonString(root, "name") ?? "";
                    overview = GetJsonString(root, "overview") ?? "";
                    string posterPath = GetJsonString(root, "poster_path") ?? "";
                    string backdropPath = GetJsonString(root, "backdrop_path") ?? "";

                    posterUrl = !string.IsNullOrWhiteSpace(posterPath) ? $"https://image.tmdb.org/t/p/w500{posterPath}" : "/images/placeholder_poster.jpg";
                    bannerUrl = !string.IsNullOrWhiteSpace(backdropPath) ? $"https://image.tmdb.org/t/p/original{backdropPath}" : posterUrl;

                    if (DateOnly.TryParse(GetJsonString(root, "first_air_date"), out var d))
                    {
                        releaseDate = d;
                    }

                    string origLang = GetJsonString(root, "original_language") ?? "";
                    (country, language) = MapLanguage(origLang);
                    if (origLang == "th" && targetContentTypeId == 2) targetContentTypeId = 3;

                    // Genres
                    if (root.TryGetProperty("genres", out var gArr) && gArr.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var g in gArr.EnumerateArray())
                        {
                            var name = GetJsonString(g, "name");
                            if (!string.IsNullOrEmpty(name)) genreNames.Add(name);
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(title))
                {
                    return new TmdbImportResult { IsSuccess = false, Message = "Failed to retrieve drama information from TMDB." };
                }

                // Strictly ensure existing drama is NOT modified or duplicated!
                var existing = await _db.Contents.FirstOrDefaultAsync(c =>
                    !c.IsDeleted &&
                    c.ContentTypeId != 1 &&
                    (c.TmdbId == tmdbId || c.Title.ToLower() == title.ToLower()));
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
                    TmdbId = tmdbId,
                    IsManual = false,
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
                        if (sDoc.RootElement.TryGetProperty("episodes", out var epArr) && epArr.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var ep in epArr.EnumerateArray())
                            {
                                int epNum = GetJsonInt(ep, "episode_number") ?? episodes.Count + 1;
                                string epName = GetJsonString(ep, "name") ?? $"Episode {epNum}";
                                string epOverview = GetJsonString(ep, "overview") ?? "";
                                string stillPath = GetJsonString(ep, "still_path") ?? "";
                                int runtime = GetJsonInt(ep, "runtime") ?? 45;
                                string stillUrl = !string.IsNullOrWhiteSpace(stillPath) ? $"https://image.tmdb.org/t/p/w500{stillPath}" : posterUrl;

                                // Episodes start with empty videoUrl so user/admin can link real episode video files or stream links
                                string videoUrl = "";

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
                    string videoUrl = "";

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

        private static List<TmdbSearchResultDto> ParseSearchResults(JsonElement root, string mediaType, int? maxResults = null)
        {
            var results = new List<TmdbSearchResultDto>();
            if (!root.TryGetProperty("results", out var items) || items.ValueKind != JsonValueKind.Array)
                return results;

            foreach (var el in items.EnumerateArray())
            {
                if (maxResults.HasValue && results.Count >= maxResults.Value)
                    break;

                var id = GetJsonInt(el, "id") ?? 0;
                if (id <= 0) continue;

                var title = mediaType == "movie"
                    ? GetJsonString(el, "title") ?? ""
                    : GetJsonString(el, "name") ?? "";
                var origTitle = mediaType == "movie"
                    ? GetJsonString(el, "original_title") ?? ""
                    : GetJsonString(el, "original_name") ?? "";

                var overview = GetJsonString(el, "overview") ?? "";
                var posterPath = GetJsonString(el, "poster_path") ?? "";
                var backdropPath = GetJsonString(el, "backdrop_path") ?? "";
                var date = mediaType == "movie"
                    ? GetJsonString(el, "release_date") ?? ""
                    : GetJsonString(el, "first_air_date") ?? "";
                var lang = GetJsonString(el, "original_language") ?? "";
                var vote = GetJsonDouble(el, "vote_average");

                var posterUrl = !string.IsNullOrWhiteSpace(posterPath) ? $"https://image.tmdb.org/t/p/w500{posterPath}" : "/images/placeholder_poster.jpg";
                var bannerUrl = !string.IsNullOrWhiteSpace(backdropPath) ? $"https://image.tmdb.org/t/p/original{backdropPath}" : posterUrl;
                var (country, _) = MapLanguage(lang);

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
                    MediaType = mediaType,
                    IsFromTmdb = true
                });
            }

            return results;
        }

        private static string FormatTmdbError(HttpStatusCode statusCode, string body)
        {
            string? detail = null;
            try
            {
                using var doc = JsonDocument.Parse(body);
                detail = GetJsonString(doc.RootElement, "status_message");
            }
            catch { }

            if (statusCode == HttpStatusCode.Unauthorized)
            {
                return $"TMDB API key is invalid or unauthorized (401){(detail != null ? $": {detail}" : ".")} Please update TmdbApiKey in appsettings.json.";
            }

            return $"TMDB request failed ({(int)statusCode}){(detail != null ? $": {detail}" : ".")}";
        }

        private static (string Country, string Language) MapLanguage(
            string? origLang,
            string defaultCountry = "South Korea",
            string defaultLanguage = "Korean")
        {
            return (origLang ?? string.Empty).ToLowerInvariant() switch
            {
                "th" => ("Thailand", "Thai"),
                "ja" => ("Japan", "Japanese"),
                "zh" => ("China", "Chinese"),
                "ko" => ("South Korea", "Korean"),
                "en" => ("United States", "English"),
                "fr" => ("France", "French"),
                "es" => ("Spain", "Spanish"),
                "hi" => ("India", "Hindi"),
                _ => (defaultCountry, defaultLanguage)
            };
        }

        private static string? GetJsonString(JsonElement el, string prop) =>
            el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

        private static int? GetJsonInt(JsonElement el, string prop) =>
            el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.Number && v.TryGetInt32(out var val) ? val : null;

        private static double GetJsonDouble(JsonElement el, string prop) =>
            el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.Number && v.TryGetDouble(out var val) ? val : 0.0;
    }
}
