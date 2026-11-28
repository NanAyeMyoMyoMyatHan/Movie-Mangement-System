using DotNetProject.Domain.Features.Tmdb;
using DotNetProject.WebApi.Features.Shared;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetProject.WebApi.Features.Tmdb
{
    [Route("api/[controller]")]
    [ApiController]
    public class TmdbController : ControllerBase
    {
        private readonly ITmdbService _tmdbService;

        public TmdbController(ITmdbService tmdbService)
        {
            _tmdbService = tmdbService;
        }

        [HttpGet("search/movies")]
        public async Task<IActionResult> SearchMovies([FromQuery] string query, [FromQuery] int maxResults = 6, CancellationToken cancellationToken = default)
        {
            var res = await _tmdbService.SearchMovieSuggestionsAsync(query, maxResults, cancellationToken);
            return Ok(res);
        }

        [HttpGet("search/dramas")]
        public async Task<IActionResult> SearchDramas([FromQuery] string query, [FromQuery] int maxResults = 6, CancellationToken cancellationToken = default)
        {
            var res = await _tmdbService.SearchDramaSuggestionsAsync(query, maxResults, cancellationToken);
            return Ok(res);
        }

        [HttpGet("movies/{tmdbId}")]
        public async Task<IActionResult> GetMovie(int tmdbId, CancellationToken cancellationToken = default)
        {
            var res = await _tmdbService.GetMovieDetailsAsync(tmdbId, cancellationToken);
            return Ok(res);
        }

        [HttpGet("dramas/{tmdbId}")]
        public async Task<IActionResult> GetDrama(int tmdbId, CancellationToken cancellationToken = default)
        {
            var res = await _tmdbService.GetDramaDetailsAsync(tmdbId, cancellationToken);
            return Ok(res);
        }

        [HttpPost("import/movie/{tmdbId}")]
        public async Task<IActionResult> ImportMovie(int tmdbId, [FromQuery] string? customCategory = null, CancellationToken cancellationToken = default)
        {
            var res = await _tmdbService.ImportTmdbMovieAsync(tmdbId, customCategory, cancellationToken);
            return Ok(res);
        }

        [HttpPost("import/drama/{tmdbId}")]
        public async Task<IActionResult> ImportDrama(int tmdbId, [FromQuery] string? customCategory = null, CancellationToken cancellationToken = default)
        {
            var res = await _tmdbService.ImportTmdbDramaAsync(tmdbId, customCategory, cancellationToken);
            return Ok(res);
        }
    }
}
