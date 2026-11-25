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

            var content = await _context.Contents
                .AsNoTracking()
                .Include(c => c.ContentGenres)
                    .ThenInclude(cg => cg.Genre)
                .FirstOrDefaultAsync(c => c.ContentId == request.Id && c.ContentTypeId == 1 && !c.IsDeleted, cancellationToken);

            if (content != null)
            {
                return Result<GetMovieDetailResponse>.Success(
                    new GetMovieDetailResponse(ToDetailResponse(content)));
            }

            var legacyMovie = await _context.Movies
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == request.Id && !m.IsDeleted, cancellationToken);

            if (legacyMovie == null)
                return Result<GetMovieDetailResponse>.NotFoundError($"Movie with ID {request.Id} was not found.");

            return Result<GetMovieDetailResponse>.Success(
                new GetMovieDetailResponse(ToDetailResponse(legacyMovie)));
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

            var duplicate = await _context.Contents
                .AnyAsync(c => c.Title.ToLower() == title.ToLower() && c.ContentTypeId == 1 && !c.IsDeleted, cancellationToken);

            if (duplicate)
                return Result<CreateMovieResponse>.DuplicateRecordError($"A movie with the title '{title}' already exists.");

            var content = new Content
            {
                ContentTypeId = 1,
                Title = title,
                Description = request.Description?.Trim(),
                PosterUrl = string.IsNullOrWhiteSpace(request.PosterUrl) ? null : request.PosterUrl.Trim(),
                ReleaseDate = ToReleaseDate(request.ReleaseYear),
                Status = "Completed",
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Contents.Add(content);
            await _context.SaveChangesAsync(cancellationToken);

            await SyncGenresAsync(content, request.Genre, cancellationToken);

            _context.Episodes.Add(new Episode
            {
                ContentId = content.ContentId,
                EpisodeNumber = 1,
                Title = "Full Movie",
                Description = content.Description,
                ThumbnailUrl = content.PosterUrl,
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
            await _context.SaveChangesAsync(cancellationToken);

            return Result<CreateMovieResponse>.Success(
                new CreateMovieResponse(ToDetailResponse(content)));
        }

        public async Task<Result<UpdateMovieResponse>> UpdateAsync(
            UpdateMovieRequest request,
            CancellationToken cancellationToken)
        {
            if (request.Id <= 0)
                return Result<UpdateMovieResponse>.ValidationError("Invalid movie ID.");

            var title = request.Title?.Trim();
            if (string.IsNullOrEmpty(title))
                return Result<UpdateMovieResponse>.ValidationError("Title is required.");

            if (title.Length > 255)
                return Result<UpdateMovieResponse>.ValidationError("Title must not exceed 255 characters.");

            var content = await _context.Contents
                .Include(c => c.ContentGenres)
                    .ThenInclude(cg => cg.Genre)
                .FirstOrDefaultAsync(c => c.ContentId == request.Id && c.ContentTypeId == 1 && !c.IsDeleted, cancellationToken);

            if (content == null)
                return Result<UpdateMovieResponse>.NotFoundError($"Movie with ID {request.Id} was not found.");

            var duplicate = await _context.Contents
                .AnyAsync(c => c.Title.ToLower() == title.ToLower() && c.ContentTypeId == 1 && !c.IsDeleted && c.ContentId != request.Id,
                    cancellationToken);

            if (duplicate)
                return Result<UpdateMovieResponse>.DuplicateRecordError($"A movie with the title '{title}' already exists.");

            content.Title = title;
            content.Description = request.Description?.Trim();
            content.PosterUrl = string.IsNullOrWhiteSpace(request.PosterUrl) ? null : request.PosterUrl.Trim();
            content.ReleaseDate = ToReleaseDate(request.ReleaseYear);
            content.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            await SyncGenresAsync(content, request.Genre, cancellationToken);

            return Result<UpdateMovieResponse>.Success(
                new UpdateMovieResponse(ToDetailResponse(content)));
        }

        public async Task<Result<DeleteMovieResponse>> DeleteAsync(
            DeleteMovieRequest request,
            CancellationToken cancellationToken)
        {
            if (request.Id <= 0)
                return Result<DeleteMovieResponse>.ValidationError("Invalid movie ID.");

            var content = await _context.Contents
                .FirstOrDefaultAsync(c => c.ContentId == request.Id && c.ContentTypeId == 1, cancellationToken);

            if (content == null)
                return Result<DeleteMovieResponse>.NotFoundError($"Movie with ID {request.Id} was not found.");

            if (content.IsDeleted)
                return Result<DeleteMovieResponse>.InvalidDataError("Movie is already deleted.");

            content.IsDeleted = true;
            content.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return Result<DeleteMovieResponse>.Success(new DeleteMovieResponse(request.Id, true));
        }

        // -- Private helpers ----------------------------------------------------

        private static DateOnly? ToReleaseDate(int? year) =>
            year is >= 1888 and <= 2100 ? new DateOnly(year.Value, 1, 1) : null;

        private async Task SyncGenresAsync(Content content, string? rawGenres, CancellationToken cancellationToken)
        {
            var requestedNames = (rawGenres ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var existingLinks = await _context.ContentGenres
                .Where(cg => cg.ContentId == content.ContentId)
                .ToListAsync(cancellationToken);

            if (existingLinks.Count > 0)
                _context.ContentGenres.RemoveRange(existingLinks);

            content.ContentGenres.Clear();
            if (requestedNames.Count == 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
                return;
            }

            var allGenres = await _context.Genres
                .Where(g => !g.IsDeleted)
                .ToListAsync(cancellationToken);

            foreach (var name in requestedNames)
            {
                var genre = allGenres.FirstOrDefault(g => g.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (genre == null)
                {
                    genre = new Genre { Name = name, CreatedAt = DateTime.UtcNow, IsDeleted = false };
                    _context.Genres.Add(genre);
                    await _context.SaveChangesAsync(cancellationToken);
                    allGenres.Add(genre);
                }

                var link = new ContentGenre
                {
                    ContentId = content.ContentId,
                    GenreId = genre.GenreId,
                    Genre = genre,
                    IsDeleted = false
                };
                _context.ContentGenres.Add(link);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        private static MovieDetailResponse ToDetailResponse(Content c)
        {
            var genres = c.ContentGenres
                .Where(cg => !cg.IsDeleted && cg.Genre != null && !cg.Genre.IsDeleted)
                .Select(cg => cg.Genre.Name)
                .ToList();

            return new MovieDetailResponse(
                (int)c.ContentId,
                c.Title,
                genres.Count > 0 ? string.Join(", ", genres) : null,
                c.ReleaseDate?.Year,
                c.Description,
                c.PosterUrl,
                null,
                true,
                new DateTimeOffset(c.CreatedAt, TimeSpan.Zero));
        }

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
