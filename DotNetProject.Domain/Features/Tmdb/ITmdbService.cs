using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetProject.Domain.Features.Tmdb
{
    public interface ITmdbService
    {
        Task<TmdbLookupResponse<List<TmdbSearchResultDto>>> SearchMovieSuggestionsAsync(
            string query,
            int maxResults = 6,
            CancellationToken cancellationToken = default);

        Task<TmdbLookupResponse<List<TmdbSearchResultDto>>> SearchDramaSuggestionsAsync(
            string query,
            int maxResults = 6,
            CancellationToken cancellationToken = default);

        Task<TmdbLookupResponse<TmdbMovieDetailsDto>> GetMovieDetailsAsync(
            int tmdbId,
            CancellationToken cancellationToken = default);

        Task<TmdbLookupResponse<TmdbMovieDetailsDto>> GetDramaDetailsAsync(
            int tmdbId,
            CancellationToken cancellationToken = default);

        Task<TmdbImportResult> ImportTmdbMovieAsync(
            int tmdbId,
            string? customCategory = null,
            CancellationToken cancellationToken = default);

        Task<TmdbImportResult> ImportTmdbDramaAsync(
            int tmdbId,
            string? customCategory = null,
            CancellationToken cancellationToken = default);
    }
}
