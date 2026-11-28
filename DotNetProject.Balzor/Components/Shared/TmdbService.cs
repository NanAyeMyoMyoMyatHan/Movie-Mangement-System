using DotNetProject.Domain.Features.Tmdb;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetProject.Balzor.Components.Shared
{
    public class TmdbService
    {
        private readonly HttpClientService _api;

        public TmdbService(HttpClientService api)
        {
            _api = api;
        }

        public async Task<TmdbLookupResponse<List<TmdbSearchResultDto>>> SearchMovieSuggestionsAsync(
            string query,
            int maxResults = 6,
            CancellationToken cancellationToken = default)
        {
            var res = await _api.SearchTmdbMoviesAsync(query, maxResults);
            return res ?? new TmdbLookupResponse<List<TmdbSearchResultDto>>(true, new List<TmdbSearchResultDto>(), null);
        }

        public async Task<TmdbLookupResponse<List<TmdbSearchResultDto>>> SearchDramaSuggestionsAsync(
            string query,
            int maxResults = 6,
            CancellationToken cancellationToken = default)
        {
            var res = await _api.SearchTmdbDramasAsync(query, maxResults);
            return res ?? new TmdbLookupResponse<List<TmdbSearchResultDto>>(true, new List<TmdbSearchResultDto>(), null);
        }

        public async Task<TmdbLookupResponse<TmdbMovieDetailsDto>> GetMovieDetailsAsync(
            int tmdbId,
            CancellationToken cancellationToken = default)
        {
            var res = await _api.GetTmdbMovieAsync(tmdbId);
            return res ?? new TmdbLookupResponse<TmdbMovieDetailsDto>(false, null, "Failed to retrieve movie details.");
        }

        public Task<TmdbLookupResponse<TmdbMovieDetailsDto>> GetMovieByIdAsync(
            int tmdbId,
            CancellationToken cancellationToken = default) =>
            GetMovieDetailsAsync(tmdbId, cancellationToken);

        public async Task<TmdbLookupResponse<TmdbMovieDetailsDto>> GetDramaDetailsAsync(
            int tmdbId,
            CancellationToken cancellationToken = default)
        {
            var res = await _api.GetTmdbDramaAsync(tmdbId);
            return res ?? new TmdbLookupResponse<TmdbMovieDetailsDto>(false, null, "Failed to retrieve drama details.");
        }

        public async Task<TmdbLookupResponse<List<TmdbSearchResultDto>>> SearchAsync(string query, string mediaType = "tv")
        {
            if (mediaType == "movie")
            {
                var res = await _api.SearchTmdbMoviesAsync(query, 20);
                return res ?? new TmdbLookupResponse<List<TmdbSearchResultDto>>(true, new List<TmdbSearchResultDto>(), null);
            }
            else
            {
                var res = await _api.SearchTmdbDramasAsync(query, 20);
                return res ?? new TmdbLookupResponse<List<TmdbSearchResultDto>>(true, new List<TmdbSearchResultDto>(), null);
            }
        }

        public async Task<TmdbImportResult> ImportMovieAsync(int tmdbId)
        {
            var res = await _api.ImportTmdbMovieAsync(tmdbId);
            return res ?? new TmdbImportResult { IsSuccess = false, Message = "Failed to import movie from TMDB." };
        }

        public async Task<TmdbImportResult> ImportDramaAsync(int tmdbId, int targetContentTypeId = 2)
        {
            string? cat = targetContentTypeId == 3 ? "Thai" : "Korean";
            var res = await _api.ImportTmdbDramaAsync(tmdbId, cat);
            return res ?? new TmdbImportResult { IsSuccess = false, Message = "Failed to import drama from TMDB." };
        }

        public Task<TmdbImportResult> ImportTmdbMovieAsync(int tmdbId, string? customCategory = null, CancellationToken cancellationToken = default) =>
            ImportMovieAsync(tmdbId);

        public Task<TmdbImportResult> ImportTmdbDramaAsync(int tmdbId, string? customCategory = null, CancellationToken cancellationToken = default) =>
            ImportDramaAsync(tmdbId, customCategory?.Contains("Thai", System.StringComparison.OrdinalIgnoreCase) == true ? 3 : 2);
    }
}
