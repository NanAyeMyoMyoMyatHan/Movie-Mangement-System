using DotNetProject.Domain.Features.Contents;
using DotNetProject.WebApi.Features.Shared;
using Microsoft.AspNetCore.Mvc;
using Common;

namespace DotNetProject.WebApi.Features.Contents
{
    [Route("api/[controller]")]
    public class ContentsController : BaseController
    {
        private readonly IContent _contentInterface;
        private readonly FileStorageService _fileStorage;

        public ContentsController(IContent contentInterface, FileStorageService fileStorage)
        {
            _contentInterface = contentInterface;
            _fileStorage = fileStorage;
        }

        // --- Legacy endpoints ---

        [HttpGet("{contentId}")]
        public async Task<IActionResult> GetContent(long contentId)
        {
            var result = await _contentInterface.GetContentDetailAsync(contentId);
            return ToActionResult(result);
        }

        [HttpGet("movie")]
        public async Task<IActionResult> GetMovies()
        {
            var result = await _contentInterface.GetMoviesAsync();
            return ToActionResult(result);
        }

        [HttpGet("drama")]
        public async Task<IActionResult> GetDramas()
        {
            var result = await _contentInterface.GetDramasAsync();
            return ToActionResult(result);
        }

        [HttpGet("search/{keyword}")]
        public async Task<IActionResult> SearchContent(string keyword)
        {
            var result = await _contentInterface.SearchAsync(keyword);
            return ToActionResult(result);
        }

        // --- New: Result-pattern CRUD & Pagination endpoints ---

        [HttpPost("paged-list")]
        public async Task<IActionResult> PagedList(
            [FromBody] GetContentPagedListRequest request,
            CancellationToken cancellationToken) =>
            ToActionResult(await _contentInterface.GetPagedListAsync(request, cancellationToken));

        [HttpPost("detail")]
        public async Task<IActionResult> Detail(
            [FromBody] GetContentDetailRequest request,
            CancellationToken cancellationToken) =>
            ToActionResult(await _contentInterface.GetDetailAsync(request, cancellationToken));

        /// <summary>
        /// Create a new content record.
        /// Send as multipart/form-data.
        /// Optional files: PosterFile (image), BannerFile (image).
        /// </summary>
        [HttpPost("create")]
        [RequestSizeLimit(600L * 1024 * 1024)]
        public async Task<IActionResult> Create(
            [FromForm] CreateContentRequest request,
            IFormFile? PosterFile,
            IFormFile? BannerFile,
            CancellationToken cancellationToken)
        {
            try
            {
                // Save uploaded files and override the URL fields with local filenames
                if (PosterFile is { Length: > 0 })
                    request = request with { PosterUrl = await _fileStorage.SaveImageAsync(PosterFile, cancellationToken) };

                if (BannerFile is { Length: > 0 })
                    request = request with { BannerUrl = await _fileStorage.SaveImageAsync(BannerFile, cancellationToken) };

                return ToActionResult(await _contentInterface.CreateAsync(request, cancellationToken));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing content record.
        /// Send as multipart/form-data.
        /// Optional files: PosterFile (image), BannerFile (image).
        /// If a new file is supplied the old file on disk is deleted automatically.
        /// </summary>
        [HttpPost("update")]
        [RequestSizeLimit(600L * 1024 * 1024)]
        public async Task<IActionResult> Update(
            [FromForm] UpdateContentRequest request,
            IFormFile? PosterFile,
            IFormFile? BannerFile,
            CancellationToken cancellationToken)
        {
            try
            {
                // Fetch old filenames so we can delete them after a successful replacement
                string? oldPoster = null;
                string? oldBanner = null;

                if (PosterFile is { Length: > 0 } || BannerFile is { Length: > 0 })
                {
                    var detailResult = await _contentInterface.GetDetailAsync(
                        new GetContentDetailRequest { ContentId = request.ContentId }, cancellationToken);

                    if (detailResult.IsSuccess)
                    {
                        oldPoster = detailResult.Data?.Content.PosterUrl;
                        oldBanner = detailResult.Data?.Content.BannerUrl;
                    }
                }

                // Save new files
                if (PosterFile is { Length: > 0 })
                    request = request with { PosterUrl = await _fileStorage.SaveImageAsync(PosterFile, cancellationToken) };

                if (BannerFile is { Length: > 0 })
                    request = request with { BannerUrl = await _fileStorage.SaveImageAsync(BannerFile, cancellationToken) };

                var result = await _contentInterface.UpdateAsync(request, cancellationToken);

                // Delete old local files only after DB save succeeded
                if (result.IsSuccess)
                {
                    if (PosterFile is { Length: > 0 } && FileStorageService.IsLocalFile(oldPoster))
                        _fileStorage.DeleteImage(oldPoster);

                    if (BannerFile is { Length: > 0 } && FileStorageService.IsLocalFile(oldBanner))
                        _fileStorage.DeleteImage(oldBanner);
                }

                return ToActionResult(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("delete")]
        public async Task<IActionResult> Delete(
            [FromBody] DeleteContentRequest request,
            CancellationToken cancellationToken) =>
            ToActionResult(await _contentInterface.DeleteAsync(request, cancellationToken));
    }
}
