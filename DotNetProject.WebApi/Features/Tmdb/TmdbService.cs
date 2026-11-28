using DotNetProject.Database.AppDbModels;
using DotNetProject.Domain.Features.Tmdb;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetProject.WebApi.Features.Tmdb
{
    public class TmdbService : ITmdbService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly AppDbContext _db;
        private readonly ILogger<TmdbService> _logger;
        private readonly string _apiKey;

        public TmdbService(
            HttpClient http,
            IConfiguration config,
            AppDbContext db,
            ILogger<TmdbService> logger)
        {
            _http = http;
            _config = config;
            _db = db;
            _logger = logger;
            _apiKey = _config["TmdbApiKey"] ?? "c0291e5cfbe661f1c243a056a00c6d7a";
        }

        public async Task<TmdbLookupResponse<List<TmdbSearchResultDto>>> SearchMovieSuggestionsAsync(
            string query,
            int maxResults = 6,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[DotNetProject.WebApi] SearchMovieSuggestionsAsync => Searching movies. Query={Query}", query);
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
                _logger.LogError(ex, "[DotNetProject.WebApi] SearchMovieSuggestionsAsync => Error searching TMDB.");
                return new TmdbLookupResponse<List<TmdbSearchResultDto>>(false, new List<TmdbSearchResultDto>(), $"Could not reach TMDB: {ex.Message}");
            }
        }

        public async Task<TmdbLookupResponse<List<TmdbSearchResultDto>>> SearchDramaSuggestionsAsync(
            string query,
            int maxResults = 6,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[DotNetProject.WebApi] SearchDramaSuggestionsAsync => Searching dramas. Query={Query}", query);
            return await SearchAsync(query, "tv");
        }

        public async Task<TmdbLookupResponse<TmdbMovieDetailsDto>> GetMovieDetailsAsync(
            int tmdbId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[DotNetProject.WebApi] GetMovieDetailsAsync => Fetching TMDB movie details. TmdbId={TmdbId}", tmdbId);
            if (tmdbId <= 0)
                return new TmdbLookupResponse<TmdbMovieDetailsDto>(false, null, "Please enter a valid positive TMDB ID.");

            try
            {
                var url = $"https://api.themoviedb.org/3/movie/{tmdbId}?api_key={_apiKey}&append_to_response=videos";
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

                var trailerUrl = ExtractYouTubeTrailerUrl(root) ?? GetFallbackYoutubeUrl(title);

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
                    VoteAverage = Math.Round(vote, 1),
                    TrailerUrl = trailerUrl
                };

                return new TmdbLookupResponse<TmdbMovieDetailsDto>(true, details, null);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DotNetProject.WebApi] GetMovieDetailsAsync => Error reaching TMDB.");
                return new TmdbLookupResponse<TmdbMovieDetailsDto>(false, null, $"Could not reach TMDB: {ex.Message}");
            }
        }

        public async Task<TmdbLookupResponse<TmdbMovieDetailsDto>> GetDramaDetailsAsync(
            int tmdbId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[DotNetProject.WebApi] GetDramaDetailsAsync => Fetching TMDB TV details. TmdbId={TmdbId}", tmdbId);
            if (tmdbId <= 0)
                return new TmdbLookupResponse<TmdbMovieDetailsDto>(false, null, "Please enter a valid positive TMDB ID.");

            try
            {
                var url = $"https://api.themoviedb.org/3/tv/{tmdbId}?api_key={_apiKey}&append_to_response=videos";
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

                var title = GetJsonString(root, "name") ?? string.Empty;
                var origTitle = GetJsonString(root, "original_name") ?? title;
                var overview = GetJsonString(root, "overview");
                var posterPath = GetJsonString(root, "poster_path");
                var backdropPath = GetJsonString(root, "backdrop_path");
                var firstAirDate = GetJsonString(root, "first_air_date");
                var origLang = GetJsonString(root, "original_language");
                var vote = GetJsonDouble(root, "vote_average");

                DateOnly? releaseDate = DateOnly.TryParse(firstAirDate, out var parsedDate) ? parsedDate : null;
                int? releaseYear = releaseDate?.Year;

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

                var (country, language) = MapLanguage(origLang);

                var posterUrl = !string.IsNullOrWhiteSpace(posterPath) ? $"https://image.tmdb.org/t/p/w500{posterPath}" : null;
                var bannerUrl = !string.IsNullOrWhiteSpace(backdropPath) ? $"https://image.tmdb.org/t/p/original{backdropPath}" : posterUrl;

                var trailerUrl = ExtractYouTubeTrailerUrl(root) ?? GetFallbackYoutubeUrl(title);

                var details = new TmdbMovieDetailsDto
                {
                    Id = tmdbId,
                    Title = title.Trim(),
                    OriginalTitle = origTitle.Trim(),
                    Genres = genres,
                    Genre = genres.Count > 0 ? string.Join(", ", genres) : null,
                    ReleaseDate = releaseDate,
                    ReleaseYear = releaseYear,
                    Description = overview,
                    PosterUrl = posterUrl,
                    BannerUrl = bannerUrl,
                    Country = country,
                    Language = language,
                    VoteAverage = Math.Round(vote, 1),
                    TrailerUrl = trailerUrl
                };

                return new TmdbLookupResponse<TmdbMovieDetailsDto>(true, details, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DotNetProject.WebApi] GetDramaDetailsAsync => Error reaching TMDB.");
                return new TmdbLookupResponse<TmdbMovieDetailsDto>(false, null, $"Could not reach TMDB: {ex.Message}");
            }
        }

        public async Task<TmdbImportResult> ImportTmdbMovieAsync(
            int tmdbId,
            string? customCategory = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[DotNetProject.WebApi] ImportTmdbMovieAsync => Importing movie from TMDB. TmdbId={TmdbId}", tmdbId);
            try
            {
                var lookup = await GetMovieDetailsAsync(tmdbId, cancellationToken);
                if (!lookup.IsSuccess)
                    return new TmdbImportResult { IsSuccess = false, Message = lookup.ErrorMessage ?? "Failed to retrieve movie information from TMDB." };

                var details = lookup.Data;
                if (details == null || string.IsNullOrWhiteSpace(details.Title))
                    return new TmdbImportResult { IsSuccess = false, Message = "Failed to retrieve movie information from TMDB." };

                var title = details.Title.Length > 255 ? details.Title[..255] : details.Title;

                var existing = await _db.Contents.FirstOrDefaultAsync(c =>
                    !c.IsDeleted &&
                    c.ContentTypeId == 1 &&
                    (c.TmdbId == tmdbId || c.Title.ToLower() == title.ToLower()), cancellationToken);

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
                    PosterUrl = details.PosterUrl ?? "/images/poster_fallback.svg",
                    BannerUrl = details.BannerUrl ?? details.PosterUrl ?? "/images/poster_fallback.svg",
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
                await _db.SaveChangesAsync(cancellationToken);

                if (details.Genres.Count > 0)
                {
                    var allGenres = await _db.Genres.Where(g => !g.IsDeleted).ToListAsync(cancellationToken);
                    foreach (var gn in details.Genres.Distinct(StringComparer.OrdinalIgnoreCase))
                    {
                        var genre = allGenres.FirstOrDefault(g => g.Name.Equals(gn, StringComparison.OrdinalIgnoreCase));
                        if (genre == null)
                        {
                            genre = new Genre { Name = gn, CreatedAt = DateTime.UtcNow, IsDeleted = false };
                            _db.Genres.Add(genre);
                            await _db.SaveChangesAsync(cancellationToken);
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

                var trailerUrl = details.TrailerUrl ?? GetFallbackYoutubeUrl(title) ?? string.Empty;

                _db.Episodes.Add(new Episode
                {
                    ContentId = newContent.ContentId,
                    EpisodeNumber = 1,
                    Title = !string.IsNullOrWhiteSpace(trailerUrl) ? "Official Trailer" : "Full Movie",
                    Description = details.Description,
                    ThumbnailUrl = newContent.PosterUrl,
                    VideoUrl = trailerUrl,
                    Duration = details.Runtime ?? 120,
                    ReleaseDate = details.ReleaseDate,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                });
                await _db.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("[DotNetProject.WebApi] ImportTmdbMovieAsync => Movie imported successfully. ContentId={ContentId}", newContent.ContentId);
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
                _logger.LogError(ex, "[DotNetProject.WebApi] ImportTmdbMovieAsync => Error importing movie.");
                return new TmdbImportResult
                {
                    IsSuccess = false,
                    Message = $"Error importing movie: {ex.Message}"
                };
            }
        }

        public async Task<TmdbImportResult> ImportTmdbDramaAsync(
            int tmdbId,
            string? customCategory = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[DotNetProject.WebApi] ImportTmdbDramaAsync => Importing drama from TMDB. TmdbId={TmdbId}", tmdbId);
            int targetContentTypeId = 2; // Default Korean Drama
            if (!string.IsNullOrWhiteSpace(customCategory) && customCategory.Contains("Thai", StringComparison.OrdinalIgnoreCase))
            {
                targetContentTypeId = 3; // Thai
            }

            try
            {
                string detailsUrl = $"https://api.themoviedb.org/3/tv/{tmdbId}?api_key={_apiKey}&append_to_response=videos";
                using var response = await _http.GetAsync(detailsUrl, cancellationToken);

                string title = "";
                string overview = "";
                string posterUrl = "";
                string bannerUrl = "";
                string country = "South Korea";
                string language = "Korean";
                DateOnly? releaseDate = null;
                List<string> genreNames = new();
                string? trailerUrl = null;

                if (!response.IsSuccessStatusCode)
                {
                    var errBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    return new TmdbImportResult { IsSuccess = false, Message = FormatTmdbError(response.StatusCode, errBody) };
                }

                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                using (var doc = JsonDocument.Parse(json))
                {
                    var root = doc.RootElement;
                    title = GetJsonString(root, "name") ?? "";
                    overview = GetJsonString(root, "overview") ?? "";
                    string posterPath = GetJsonString(root, "poster_path") ?? "";
                    string backdropPath = GetJsonString(root, "backdrop_path") ?? "";

                    posterUrl = !string.IsNullOrWhiteSpace(posterPath) ? $"https://image.tmdb.org/t/p/w500{posterPath}" : "/images/poster_fallback.svg";
                    bannerUrl = !string.IsNullOrWhiteSpace(backdropPath) ? $"https://image.tmdb.org/t/p/original{backdropPath}" : posterUrl;
                    trailerUrl = ExtractYouTubeTrailerUrl(root);

                    if (DateOnly.TryParse(GetJsonString(root, "first_air_date"), out var d))
                    {
                        releaseDate = d;
                    }

                    string origLang = GetJsonString(root, "original_language") ?? "";
                    (country, language) = MapLanguage(origLang);
                    if (origLang == "th" && targetContentTypeId == 2) targetContentTypeId = 3;

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

                var existing = await _db.Contents.FirstOrDefaultAsync(c =>
                    !c.IsDeleted &&
                    c.ContentTypeId != 1 &&
                    (c.TmdbId == tmdbId || c.Title.ToLower() == title.ToLower()), cancellationToken);

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
                await _db.SaveChangesAsync(cancellationToken);

                var allGenres = await _db.Genres.Where(g => !g.IsDeleted).ToListAsync(cancellationToken);
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

                // For dramas that are not hosted locally on the server:
                // Only add the trailer if available, do NOT generate fake episodes. Full episodes are on Telegram.
                var finalTrailerUrl = trailerUrl ?? GetFallbackYoutubeUrl(title) ?? "https://www.youtube.com/watch?v=4b7_4sN9Uvg";

                var episode = new Episode
                {
                    ContentId = newContent.ContentId,
                    EpisodeNumber = 1,
                    Title = "Official Trailer",
                    Description = $"Official Trailer for {title}. Watch all full episodes on our Telegram channel (@WEAREALLTRYINGHEREMM).",
                    ThumbnailUrl = bannerUrl,
                    VideoUrl = finalTrailerUrl,
                    Duration = 3,
                    ReleaseDate = releaseDate,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                _db.Episodes.Add(episode);
                await _db.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("[DotNetProject.WebApi] ImportTmdbDramaAsync => Drama imported with trailer. ContentId={ContentId}", newContent.ContentId);
                return new TmdbImportResult
                {
                    IsSuccess = true,
                    Message = $"Successfully imported \"{title}\" (Official Trailer only, full episodes available on Telegram).",
                    ContentId = newContent.ContentId,
                    EpisodeCount = 1
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DotNetProject.WebApi] ImportTmdbDramaAsync => Error importing drama.");
                return new TmdbImportResult
                {
                    IsSuccess = false,
                    Message = $"Error importing drama: {ex.Message}"
                };
            }
        }

        private async Task<TmdbLookupResponse<List<TmdbSearchResultDto>>> SearchAsync(string query, string mediaType)
        {
            var results = new List<TmdbSearchResultDto>();
            if (string.IsNullOrWhiteSpace(query))
                return new TmdbLookupResponse<List<TmdbSearchResultDto>>(true, results, null);

            string? tmdbError = null;
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

                var posterUrl = !string.IsNullOrWhiteSpace(posterPath) ? $"https://image.tmdb.org/t/p/w500{posterPath}" : "/images/poster_fallback.svg";
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

        private static string? ExtractYouTubeTrailerUrl(JsonElement root)
        {
            if (root.TryGetProperty("videos", out var videosProp) &&
                videosProp.TryGetProperty("results", out var resultsArr) &&
                resultsArr.ValueKind == JsonValueKind.Array)
            {
                string? firstTrailerKey = null;
                string? firstTeaserKey = null;
                string? anyKey = null;

                foreach (var v in resultsArr.EnumerateArray())
                {
                    var site = GetJsonString(v, "site");
                    if (!string.Equals(site, "YouTube", StringComparison.OrdinalIgnoreCase)) continue;

                    var key = GetJsonString(v, "key");
                    if (string.IsNullOrWhiteSpace(key)) continue;

                    var type = GetJsonString(v, "type");
                    bool isOfficial = false;
                    if (v.TryGetProperty("official", out var offProp))
                    {
                        isOfficial = offProp.GetBoolean();
                    }

                    if (string.Equals(type, "Trailer", StringComparison.OrdinalIgnoreCase))
                    {
                        if (isOfficial)
                        {
                            return $"https://www.youtube.com/watch?v={key}";
                        }
                        firstTrailerKey ??= key;
                    }
                    else if (string.Equals(type, "Teaser", StringComparison.OrdinalIgnoreCase))
                    {
                        firstTeaserKey ??= key;
                    }
                    else
                    {
                        anyKey ??= key;
                    }
                }

                var selectedKey = firstTrailerKey ?? firstTeaserKey ?? anyKey;
                if (!string.IsNullOrWhiteSpace(selectedKey))
                {
                    return $"https://www.youtube.com/watch?v={selectedKey}";
                }
            }
            return null;
        }

        private static string? GetFallbackYoutubeUrl(string seriesTitle)
        {
            if (string.IsNullOrWhiteSpace(seriesTitle)) return null;

            if (seriesTitle.Contains("Your Name", StringComparison.OrdinalIgnoreCase))
                return "https://www.youtube.com/watch?v=xU47nhruN-Q";
            if (seriesTitle.Contains("Spirited", StringComparison.OrdinalIgnoreCase))
                return "https://www.youtube.com/watch?v=ByXuk9QqQkk";
            if (seriesTitle.Contains("Busan", StringComparison.OrdinalIgnoreCase))
                return "https://www.youtube.com/watch?v=pyWuHv2-Abk";
            if (seriesTitle.Contains("Parasite", StringComparison.OrdinalIgnoreCase))
                return "https://www.youtube.com/watch?v=5xH0RwpEbSU";
            if (seriesTitle.Contains("The Childe", StringComparison.OrdinalIgnoreCase))
                return "https://www.youtube.com/watch?v=SRJgKBFwZyw";
            if (seriesTitle.Contains("Bad Genius", StringComparison.OrdinalIgnoreCase))
                return "https://www.youtube.com/watch?v=CLdhN4oMxCQ";
            if (seriesTitle.Contains("Along with the Gods", StringComparison.OrdinalIgnoreCase))
                return "https://www.youtube.com/watch?v=Y3xcJor-hyQ";
            if (seriesTitle.Contains("Decibel", StringComparison.OrdinalIgnoreCase))
                return "https://www.youtube.com/watch?v=UmgVsK6cQuc";
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
            if (seriesTitle.Contains("Itaewon", StringComparison.OrdinalIgnoreCase))
                return "https://www.youtube.com/watch?v=NN7zE_aI7B4";
            if (seriesTitle.Contains("True Beauty", StringComparison.OrdinalIgnoreCase))
                return "https://www.youtube.com/watch?v=2Tz8YmQ-N0s";
            if (seriesTitle.Contains("Twenty-Five", StringComparison.OrdinalIgnoreCase))
                return "https://www.youtube.com/watch?v=gYp4cKumTwU";
            if (seriesTitle.Contains("Business", StringComparison.OrdinalIgnoreCase))
                return "https://www.youtube.com/watch?v=M-PHcxPJUS8";
            if (seriesTitle.Contains("SOTUS", StringComparison.OrdinalIgnoreCase))
                return "https://www.youtube.com/watch?v=t9-X2wXm6u0";
            if (seriesTitle.Contains("Sunset", StringComparison.OrdinalIgnoreCase))
                return "https://www.youtube.com/watch?v=zJg4hZ5Fw7A";
            if (seriesTitle.Contains("President", StringComparison.OrdinalIgnoreCase))
                return "https://www.youtube.com/watch?v=3gVq6vO8VzE";
            if (seriesTitle.Contains("Moonlight", StringComparison.OrdinalIgnoreCase))
                return "https://www.youtube.com/watch?v=9gR9vM8ZqT8";
            if (seriesTitle.Contains("Not Me", StringComparison.OrdinalIgnoreCase))
                return "https://www.youtube.com/watch?v=1F_U3mN8rJ0";

            return null;
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
