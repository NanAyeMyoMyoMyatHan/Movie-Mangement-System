using DotNetProject.WebApi.Features.Shared;
using DotNetProject.Domain.Features.Movies;
using Common;
using Microsoft.AspNetCore.Mvc;

namespace DotNetProject.WebApi.Features.Movies
{
    [Route("api/[controller]")]
    public class MovieController : BaseController
    {
        private readonly IMoviesInterface _movieService;
        private readonly FileStorageService _fileStorage;

        public MovieController(IMoviesInterface movieService, FileStorageService fileStorage)
        {
            _movieService = movieService;
            _fileStorage = fileStorage;
        }

        // -- Legacy endpoints (unchanged) --------------------------------------

        [HttpGet]
        public async Task<IActionResult> GetAllMovies()
        {
            var movies = await _movieService.GetAllMoviesAsync();
            return Ok(movies);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMovieById(int id)
        {
            var movie = await _movieService.GetMovieByIdAsync(id);
            if (movie == null)
                return NotFound();
            return Ok(movie);
        }

        // -- New: Result-pattern + pagination endpoints -------------------------

        [HttpPost("paged-list")]
        public async Task<IActionResult> PagedList(
            [FromBody] GetMoviePagedListRequest request,
            CancellationToken cancellationToken) =>
            ToActionResult(await _movieService.GetPagedListAsync(request, cancellationToken));

        [HttpPost("detail")]
        public async Task<IActionResult> Detail(
            [FromBody] GetMovieDetailRequest request,
            CancellationToken cancellationToken) =>
            ToActionResult(await _movieService.GetDetailAsync(request, cancellationToken));

        /// <summary>
        /// Create a new movie record.
        /// Send as multipart/form-data.
        /// Optional file: PosterFile (image — jpg, jpeg, png, webp, gif, max 10 MB).
        /// </summary>
        [HttpPost("create")]
        [RequestSizeLimit(20L * 1024 * 1024)] // 20 MB (poster only)
        public async Task<IActionResult> Create(
            [FromForm] CreateMovieRequest request,
            IFormFile? PosterFile,
            CancellationToken cancellationToken)
        {
            try
            {
                if (PosterFile is { Length: > 0 })
                    request = request with { PosterUrl = await _fileStorage.SaveImageAsync(PosterFile, cancellationToken) };

                return ToActionResult(await _movieService.CreateAsync(request, cancellationToken));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing movie record.
        /// Send as multipart/form-data.
        /// Optional file: PosterFile (image). Old poster is deleted if a new one is uploaded.
        /// </summary>
        [HttpPost("update")]
        [RequestSizeLimit(20L * 1024 * 1024)] // 20 MB (poster only)
        public async Task<IActionResult> Update(
            [FromForm] UpdateMovieRequest request,
            IFormFile? PosterFile,
            CancellationToken cancellationToken)
        {
            try
            {
                string? oldPoster = null;

                if (PosterFile is { Length: > 0 })
                {
                    var detailResult = await _movieService.GetDetailAsync(
                        new GetMovieDetailRequest { Id = request.Id }, cancellationToken);

                    if (detailResult.IsSuccess)
                        oldPoster = detailResult.Data?.Movie.PosterUrl;

                    request = request with { PosterUrl = await _fileStorage.SaveImageAsync(PosterFile, cancellationToken) };
                }

                var result = await _movieService.UpdateAsync(request, cancellationToken);

                // Delete old file after successful DB save
                if (result.IsSuccess && PosterFile is { Length: > 0 } && FileStorageService.IsLocalFile(oldPoster))
                    _fileStorage.DeleteImage(oldPoster);

                return ToActionResult(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("delete")]
        public async Task<IActionResult> Delete(
            [FromBody] DeleteMovieRequest request,
            CancellationToken cancellationToken) =>
            ToActionResult(await _movieService.DeleteAsync(request, cancellationToken));
    }
}
