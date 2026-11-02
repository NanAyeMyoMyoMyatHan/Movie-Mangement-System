using Common;

namespace DotNetProject.Domain.Features.Movies
{
    public interface IMoviesInterface
    {
        // -- Legacy methods (unchanged) -----------------------------------------
        Task<List<MovieModel>> GetAllMoviesAsync();
        Task<MovieModel?> GetMovieByIdAsync(long id);
       

        // -- New: Result-pattern + pagination ----------------------------------
        Task<Result<GetMoviePagedListResponse>> GetPagedListAsync(
            GetMoviePagedListRequest request, CancellationToken cancellationToken);

        Task<Result<GetMovieDetailResponse>> GetDetailAsync(
            GetMovieDetailRequest request, CancellationToken cancellationToken);

        Task<Result<CreateMovieResponse>> CreateAsync(
            CreateMovieRequest request, CancellationToken cancellationToken);

        Task<Result<UpdateMovieResponse>> UpdateAsync(
            UpdateMovieRequest request, CancellationToken cancellationToken);

        Task<Result<DeleteMovieResponse>> DeleteAsync(
            DeleteMovieRequest request, CancellationToken cancellationToken);
    }
}
