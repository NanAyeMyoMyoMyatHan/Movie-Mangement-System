using DotNetProject.Database.AppDbModels;
using DotNetProject.Domain.Features.Movies;
using Common;
using Microsoft.EntityFrameworkCore;

namespace DotNetProject.WebApi.Features.Movies
{
    public class MovieServices : IMoviesInterface
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public MovieServices(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // -- Legacy implementations (unchanged) --------------------------------

        public async Task<List<MovieModel>> GetAllMoviesAsync()
        {
            var movies = await _context.Contents.Where(m => !m.IsDeleted && m.ContentTypeId == 1).ToListAsync();
            return movies.Select(x => new MovieModel
            {
                ContentId = x.ContentId,
                Title = x.Title,
                Description = x.Description,
                PosterUrl = x.PosterUrl,
                BannerUrl = x.BannerUrl,
                ReleaseDate = x.ReleaseDate,
                Country = x.Country,
                Language = x.Language,
                ContentTypeId = x.ContentTypeId,
                Status = x.Status
            }).ToList();
        }

        public async Task<MovieModel?> GetMovieByIdAsync(long id)
        {
            var movie = await _context.Contents.FindAsync(id);
            if (movie == null || movie.IsDeleted)
                return null;

            return new MovieModel
            {
                ContentId = movie.ContentId,
                Title = movie.Title,
                Description = movie.Description,
                PosterUrl = movie.PosterUrl,
                BannerUrl = movie.BannerUrl,
                ReleaseDate = movie.ReleaseDate,
                Country = movie.Country,
                Language = movie.Language,
                ContentTypeId = movie.ContentTypeId,
                Status = movie.Status
            };
        }
 
       
        // -- New: Result-pattern + pagination ----------------------------------

        public async Task<Result<GetMoviePagedListResponse>> GetPagedListAsync(
            GetMoviePagedListRequest request,
            CancellationToken cancellationToken)
        {
            var pageNo = request.PageNo < 1 ? 1 : request.PageNo;
            var pageSize = request.PageSize < 1 ? 10 : request.PageSize > 100 ? 100 : request.PageSize;

            var query = _context.Contents
                .AsNoTracking()
                .Where(m => !m.IsDeleted && m.ContentTypeId == 1);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var search = request.SearchTerm.Trim().ToLower();
                query = query.Where(m =>
                    m.Title.ToLower().Contains(search) ||
                    (m.Title != null && m.Title.ToLower().Contains(search)));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(m => m.Title)
                .ThenBy(m => m.ContentId)
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new MovieListItemResponse(
                    m.ContentId,
                    m.Title,
                    
                    m.ReleaseDate.HasValue ? m.ReleaseDate.Value.Year : (int?)null,
                    m.PosterUrl))
                .ToListAsync(cancellationToken);

            return Result<GetMoviePagedListResponse>.Success(
                new GetMoviePagedListResponse(items, totalCount, pageNo, pageSize));
        }

        public async Task<Result<GetMovieDetailResponse>> GetDetailAsync(
            GetMovieDetailRequest request,
            CancellationToken cancellationToken)
        {
            if (request.Id <= 0)
                return Result<GetMovieDetailResponse>.ValidationError("Invalid movie ID.");

            var movie = await _context.Movies
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == request.Id && !m.IsDeleted, cancellationToken);

            if (movie == null)
                return Result<GetMovieDetailResponse>.NotFoundError($"Movie with ID {request.Id} was not found.");

            return Result<GetMovieDetailResponse>.Success(
                new GetMovieDetailResponse(ToDetailResponse(movie)));
        }

        public async Task<Result<CreateMovieResponse>> CreateAsync(
            CreateMovieRequest request,
            CancellationToken cancellationToken)
        {
            var title = request.Title?.Trim();
            if (string.IsNullOrEmpty(title))
                return Result<CreateMovieResponse>.ValidationError("Title is required.");

            if (title.Length > 255)
                return Result<CreateMovieResponse>.ValidationError("Title must not exceed 255 characters.");

            var duplicate = await _context.Movies
                .AnyAsync(m => m.Title.ToLower() == title.ToLower() && !m.IsDeleted, cancellationToken);

            if (duplicate)
                return Result<CreateMovieResponse>.DuplicateRecordError($"A movie with the title '{title}' already exists.");

            var movie = new Movie
            {
                Title = title,
                Genre = request.Genre?.Trim(),
                Year = request.ReleaseYear,
                Description = request.Description?.Trim(),
                PosterUrl = request.PosterUrl?.Trim(),
                TmdbId = request.TmdbId,
                IsManual = request.IsManual,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<CreateMovieResponse>.Success(
                new CreateMovieResponse(ToDetailResponse(movie)));
        }

        public async Task<Result<UpdateMovieResponse>> UpdateAsync(
            UpdateMovieRequest request,
            CancellationToken cancellationToken)
        {
            if (request.Id <= 0)
                return Result<UpdateMovieResponse>.ValidationError("Invalid movie ID.");

            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.Id == request.Id && !m.IsDeleted, cancellationToken);

            if (movie == null)
                return Result<UpdateMovieResponse>.NotFoundError($"Movie with ID {request.Id} was not found.");

            var title = request.Title?.Trim();
            if (string.IsNullOrEmpty(title))
                return Result<UpdateMovieResponse>.ValidationError("Title is required.");

            if (title.Length > 255)
                return Result<UpdateMovieResponse>.ValidationError("Title must not exceed 255 characters.");

            var duplicate = await _context.Movies
                .AnyAsync(m => m.Title.ToLower() == title.ToLower() && !m.IsDeleted && m.Id != request.Id,
                    cancellationToken);

            if (duplicate)
                return Result<UpdateMovieResponse>.DuplicateRecordError($"A movie with the title '{title}' already exists.");

            movie.Title = title;
            movie.Genre = request.Genre?.Trim();
            movie.Year = request.ReleaseYear;
            movie.Description = request.Description?.Trim();
            movie.PosterUrl = request.PosterUrl?.Trim();
            movie.TmdbId = request.TmdbId;
            movie.IsManual = request.IsManual;

            await _context.SaveChangesAsync(cancellationToken);

            return Result<UpdateMovieResponse>.Success(
                new UpdateMovieResponse(ToDetailResponse(movie)));
        }

        public async Task<Result<DeleteMovieResponse>> DeleteAsync(
            DeleteMovieRequest request,
            CancellationToken cancellationToken)
        {
            if (request.Id <= 0)
                return Result<DeleteMovieResponse>.ValidationError("Invalid movie ID.");

            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

            if (movie == null)
                return Result<DeleteMovieResponse>.NotFoundError($"Movie with ID {request.Id} was not found.");

            if (movie.IsDeleted)
                return Result<DeleteMovieResponse>.InvalidDataError("Movie is already deleted.");

            movie.IsDeleted = true;
            movie.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return Result<DeleteMovieResponse>.Success(new DeleteMovieResponse(request.Id, true));
        }

        // -- Private helper -----------------------------------------------------

        private static MovieDetailResponse ToDetailResponse(Movie m) =>
            new MovieDetailResponse(
                m.Id,
                m.Title,
                m.Genre,
                m.Year,
                m.Description,
                m.PosterUrl,
                m.TmdbId,
                m.IsManual,
                new DateTimeOffset(m.CreatedAt, TimeSpan.Zero));
    }
}
