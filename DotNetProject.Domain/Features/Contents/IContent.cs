using Common;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetProject.Domain.Features.Contents
{
    public interface IContent
    {
        // Legacy methods
        Task<Result<ContentDetailDto>> GetContentDetailAsync(long contentId);
        Task<Result<List<ContentsDto>>> GetMoviesAsync();
        Task<Result<List<ContentsDto>>> GetDramasAsync();
        Task<Result<List<ContentsDto>>> GetByGenreAsync(int genreId);
        Task<Result<List<ContentsDto>>> SearchAsync(string keyword);

        // New: Result-pattern + pagination
        Task<Result<GetContentPagedListResponse>> GetPagedListAsync(
            GetContentPagedListRequest request, CancellationToken cancellationToken =default);

        Task<Result<GetContentDetailResponse>> GetDetailAsync(
            GetContentDetailRequest request, CancellationToken cancellationToken = default);

        Task<Result<CreateContentResponse>> CreateAsync(
            CreateContentRequest request, CancellationToken cancellationToken = default);

        Task<Result<UpdateContentResponse>> UpdateAsync(
            UpdateContentRequest request, CancellationToken cancellationToken = default);

        Task<Result<DeleteContentResponse>> DeleteAsync(
            DeleteContentRequest request, CancellationToken cancellationToken);
    }
}
