namespace DotNetProject.Domain.Features.Movies
{
    public interface IMoviesInterface
    {
        Task<List<MovieModel>> GetAllMoviesAsync();
        Task<MovieModel> GetMovieByIdAsync(int id);
        Task<List<MovieModel>> GetMoviesByGenreAsync(string genre);
        Task<MovieCreateResponseModel> CreateMoviesAsync(MovieCreateRequestModel model);
        Task<MovieDeleteResponseModel> DeleteMovieAsync(MovieDeleteRequestModel model);
        Task<MovieUpdateResponseModel> UpdateMovieAsync(MovieUpdateRequestModel model);
    }
}
