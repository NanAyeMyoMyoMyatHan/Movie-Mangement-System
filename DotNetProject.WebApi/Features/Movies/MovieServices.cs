using DotNetProject.Database.AppDbModels;
using Microsoft.EntityFrameworkCore;

namespace DotNetProject.WebApi.Features.Movies
{
    public class MovieServices:IMoviesInterface
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        public MovieServices(AppDbContext context, IWebHostEnvironment environment = null)
        {
            _context = context;
            _environment = environment;
        }
        public async Task<List<MovieModel>> GetAllMoviesAsync()
        {
            var movies= await _context.Movies.ToListAsync();
            return movies.Select(x => new MovieModel
            {
                Id = x.Id,
                Title = x.Title,
                Genre = x.Genre,
                ReleaseYear = x.Year,
                Description = x.Description,
                PosterUrl = x.PosterUrl,
                TmdbId = x.TmdbId
            }).ToList();
        }
       public async Task<MovieModel> GetMovieByIdAsync(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return null;
            }
            return new MovieModel
            {
                Id = movie.Id,
                Title = movie.Title,
                Genre = movie.Genre,
                ReleaseYear = movie.Year,
                Description = movie.Description,
                PosterUrl = movie.PosterUrl,
                TmdbId = movie.TmdbId
            };
        }
        public async Task<List<MovieModel>> GetMoviesByGenreAsync(string genre)
        {
            var movies = await _context.Movies.Where(x => x.Genre == genre).ToListAsync();
            return movies.Select(x => new MovieModel
            {
                Id = x.Id,
                Title = x.Title,
                Genre = x.Genre,
                ReleaseYear = x.Year,
                Description = x.Description,
                PosterUrl = x.PosterUrl,
                TmdbId = x.TmdbId
            }).ToList();
        }
        public async Task<MovieCreateResponseModel> CreateMoviesAsync(MovieCreateRequestModel model)
        {
            var movie = new Movie
            {
                Title = model.Title,
                Genre = model.Genre,
                Year = model.ReleaseYear,
                Description = model.Description,
                PosterUrl = model.PosterUrl,
                TmdbId = model.TmdbId,
                IsManual = model.IsManual,
                CreatedAt = DateTime.UtcNow
            };
            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();
            return new MovieCreateResponseModel
            {
                IsSuccess = "true",
                Message = "Movie created successfully"
            };
        }
        public async Task<MovieDeleteResponseModel> DeleteMovieAsync(MovieDeleteRequestModel model)
        {
            var movie = await _context.Movies.FindAsync(model.Id);
            if (movie == null)
            {
                return new MovieDeleteResponseModel
                {
                    IsSuccess = "false",
                    Message = "Movie not found"
                };
            }
            movie.IsDeleted = true;
            movie.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return new MovieDeleteResponseModel
            {
                IsSuccess = "true",
                Message = "Movie deleted successfully"
            };
        }
        public async Task<MovieUpdateResponseModel> UpdateMovieAsync(MovieUpdateRequestModel model)
        {
            var movie = await _context.Movies.FindAsync(model.Id);
            if(movie == null)
            {
                return new MovieUpdateResponseModel
                {
                    IsSuccess = "false",
                    Message = "Movie not found"
                };
            }
            movie.Title = model.Title;
            movie.Genre= model.Genre;
            movie.Year = model.ReleaseYear;
            movie.Description = model.Description;
            movie.PosterUrl = model.PosterUrl;
            movie.TmdbId = model.TmdbId;
            movie.IsManual = model.IsManual;
            await _context.SaveChangesAsync();
            return new MovieUpdateResponseModel
            {
                IsSuccess = "true",
                Message = "Update Successful"
            };
        }
    }
}
