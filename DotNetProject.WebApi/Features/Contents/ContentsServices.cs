using DotNetProject.Database.AppDbModels;
using DotNetProject.Domain.Features.Contents;
using Common;
using Microsoft.EntityFrameworkCore;

namespace DotNetProject.WebApi.Features.Contents
{
    public class ContentsServices : IContent
    {
        private readonly IWebHostEnvironment? _environment;
        private readonly AppDbContext _db;

        public ContentsServices(AppDbContext db, IWebHostEnvironment? environment = null)
        {
            _db = db;
            _environment = environment;
        }

        // --- Legacy methods ---

        public async Task<Result<ContentDetailDto>> GetContentDetailAsync(long contentId)
        {
            var content = await _db.Contents
                .Include(x => x.ContentType)
                .Include(x => x.ContentGenres)
                .ThenInclude(x => x.Genre)
                .Include(x => x.Episodes)
                .FirstOrDefaultAsync(x => x.ContentId == contentId);

            if (content == null)
            {
                return Result<ContentDetailDto>.NotFoundError($"Content with id {contentId} was not found.");
            }

            var result = new ContentDetailDto
            {
                ContentId = content.ContentId,
                Title = content.Title,
                Description = content.Description,
                PosterUrl = content.PosterUrl,
                BannerUrl = content.BannerUrl,
                ReleaseDate = content.ReleaseDate,
                Country = content.Country,
                Language = content.Language,
                ContentType = content.ContentType?.Name ?? string.Empty,
                Status = content.Status,
                Genres = content.ContentGenres
                    .Select(x => x.Genre.Name)
                    .ToList(),
                Episodes = content.Episodes.Where(x => !x.IsDeleted).OrderBy(x => x.EpisodeNumber)
                    .Select(x => new EpisodeDto
                    {
                        EpisodeId = x.EpisodeId,
                        ContentId = x.ContentId,
                        EpisodeNumber = x.EpisodeNumber,
                        Title = x.Title,
                        Description = x.Description,
                        ThumbnailUrl = x.ThumbnailUrl,
                        VideoUrl = x.VideoUrl,
                        Duration = x.Duration,
                        ReleaseDate = x.ReleaseDate,
                        Status = x.Status
                    })
                    .ToList()
            };
            return Result<ContentDetailDto>.Success(result);
        }

        public async Task<Result<List<ContentsDto>>> GetMoviesAsync()
        {
            var contents = await _db.Contents
                .Include(x => x.ContentType)
                .Where(x => x.ContentType.Name == "Movie" && !x.IsDeleted)
                .ToListAsync();

            var result = contents.Select(x => new ContentsDto
            {
                ContentId = x.ContentId,
                Title = x.Title,
                Description = x.Description,
                PosterUrl = x.PosterUrl,
                BannerUrl = x.BannerUrl,
                ReleaseDate = x.ReleaseDate,
                Country = x.Country,
                Language = x.Language,
                ContentType = x.ContentType.Name,
                Status = x.Status
            }).ToList();

            return Result<List<ContentsDto>>.Success(result);
        }

        public async Task<Result<List<ContentsDto>>> GetDramasAsync()
        {
            var contents = await _db.Contents
                .Include(x => x.ContentType)
                .Where(x => x.ContentType.Name == "Drama" && !x.IsDeleted)
                .ToListAsync();

            var result = contents.Select(x => new ContentsDto
            {
                ContentId = x.ContentId,
                Title = x.Title,
                Description = x.Description,
                PosterUrl = x.PosterUrl,
                BannerUrl = x.BannerUrl,
                ReleaseDate = x.ReleaseDate,
                Country = x.Country,
                Language = x.Language,
                ContentType = x.ContentType.Name,
                Status = x.Status
            }).ToList();

            return Result<List<ContentsDto>>.Success(result);
        }

        public async Task<Result<List<ContentsDto>>> GetByGenreAsync(int genreId)
        {
            var contents = await _db.Contents
                .Include(x => x.ContentType)
                .Where(x => !x.IsDeleted && x.ContentGenres.Any(cg => cg.GenreId == genreId))
                .ToListAsync();

            var result = contents.Select(x => new ContentsDto
            {
                ContentId = x.ContentId,
                Title = x.Title,
                Description = x.Description,
                PosterUrl = x.PosterUrl,
                BannerUrl = x.BannerUrl,
                ReleaseDate = x.ReleaseDate,
                Country = x.Country,
                Language = x.Language,
                ContentType = x.ContentType.Name,
                Status = x.Status
            }).ToList();

            return Result<List<ContentsDto>>.Success(result);
        }

        public async Task<Result<List<ContentsDto>>> SearchAsync(string keyword)
        {
            var contents = await _db.Contents
                .Include(x => x.ContentType)
                .Where(x => !x.IsDeleted && x.Title.Contains(keyword))
                .ToListAsync();

            var result = contents.Select(x => new ContentsDto
            {
                ContentId = x.ContentId,
                Title = x.Title,
                Description = x.Description,
                PosterUrl = x.PosterUrl,
                BannerUrl = x.BannerUrl,
                ReleaseDate = x.ReleaseDate,
                Country = x.Country,
                Language = x.Language,
                ContentType = x.ContentType.Name,
                Status = x.Status
            }).ToList();

            return Result<List<ContentsDto>>.Success(result);
        }

        // --- New: Result-pattern CRUD & Pagination ---

        public async Task<Result<GetContentPagedListResponse>> GetPagedListAsync(
            GetContentPagedListRequest request,
            CancellationToken cancellationToken)
        {
            var pageNo = request.PageNo < 1 ? 1 : request.PageNo;
            var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

            var query = _db.Contents
                .AsNoTracking()
                .Include(c => c.ContentType)
                .Where(c => !c.IsDeleted && c.ContentTypeId==2);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim().ToLower();
                query = query.Where(c => c.Title.ToLower().Contains(term));
            }

            if (request.ContentTypeId.HasValue && request.ContentTypeId.Value > 0)
            {
                query = query.Where(c => c.ContentTypeId == request.ContentTypeId.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new ContentListItemResponse(
                    c.ContentId,
                    c.Title,
                    c.Description,
                    c.PosterUrl,
                    c.BannerUrl,
                    c.ReleaseDate,
                    c.Country,
                    c.Language,
                    c.ContentType != null ? c.ContentType.Name : string.Empty,
                    c.Status))
                .ToListAsync(cancellationToken);

            return Result<GetContentPagedListResponse>.Success(
                new GetContentPagedListResponse(items, totalCount, pageNo, pageSize));
        }

        public async Task<Result<GetContentDetailResponse>> GetDetailAsync(
            GetContentDetailRequest request,
            CancellationToken cancellationToken)
        {
            if (request.ContentId <= 0)
                return Result<GetContentDetailResponse>.ValidationError("Invalid content ID.");

            var content = await _db.Contents
                .AsNoTracking()
                .Include(c => c.ContentType)
                .FirstOrDefaultAsync(c => c.ContentId == request.ContentId, cancellationToken);

            if (content == null || content.IsDeleted)
                return Result<GetContentDetailResponse>.NotFoundError($"Content with ID {request.ContentId} was not found.");

            return Result<GetContentDetailResponse>.Success(
                new GetContentDetailResponse(ToDetailResponse(content)));
        }

        public async Task<Result<CreateContentResponse>> CreateAsync(
            CreateContentRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                return Result<CreateContentResponse>.ValidationError("Title is required.");

            var title = request.Title.Trim();

            var duplicate = await _db.Contents
                .AnyAsync(c => c.Title.ToLower() == title.ToLower() && !c.IsDeleted, cancellationToken);

            if (duplicate)
                return Result<CreateContentResponse>.DuplicateRecordError($"Content with title '{title}' already exists.");

            var content = new Content
            {
                ContentTypeId = request.ContentTypeId <= 0 ? 1 : request.ContentTypeId,
                Title = title,
                Description = request.Description?.Trim(),
                PosterUrl = request.PosterUrl?.Trim(),
                BannerUrl = request.BannerUrl?.Trim(),
                ReleaseDate = request.ReleaseDate,
                Country = request.Country?.Trim(),
                Language = request.Language?.Trim(),
                Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status.Trim(),
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _db.Contents.AddAsync(content, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            // Re-load ContentType for mapping
            await _db.Entry(content).Reference(c => c.ContentType).LoadAsync(cancellationToken);

            return Result<CreateContentResponse>.Success(
                new CreateContentResponse(ToDetailResponse(content)));
        }

        public async Task<Result<UpdateContentResponse>> UpdateAsync(
            UpdateContentRequest request,
            CancellationToken cancellationToken)
        {
            if (request.ContentId <= 0)
                return Result<UpdateContentResponse>.ValidationError("Invalid content ID.");

            if (string.IsNullOrWhiteSpace(request.Title))
                return Result<UpdateContentResponse>.ValidationError("Title is required.");

            var content = await _db.Contents
                .Include(c => c.ContentType)
                .FirstOrDefaultAsync(c => c.ContentId == request.ContentId, cancellationToken);

            if (content == null || content.IsDeleted)
                return Result<UpdateContentResponse>.NotFoundError($"Content with ID {request.ContentId} was not found.");

            var title = request.Title.Trim();

            var duplicate = await _db.Contents
                .AnyAsync(c => c.Title.ToLower() == title.ToLower() && !c.IsDeleted && c.ContentId != request.ContentId, cancellationToken);

            if (duplicate)
                return Result<UpdateContentResponse>.DuplicateRecordError($"Content with title '{title}' already exists.");

            content.ContentTypeId = request.ContentTypeId <= 0 ? content.ContentTypeId : request.ContentTypeId;
            content.Title = title;
            content.Description = request.Description?.Trim();
            content.PosterUrl = request.PosterUrl?.Trim();
            content.BannerUrl = request.BannerUrl?.Trim();
            content.ReleaseDate = request.ReleaseDate;
            content.Country = request.Country?.Trim();
            content.Language = request.Language?.Trim();
            content.Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status.Trim();
            content.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
            await _db.Entry(content).Reference(c => c.ContentType).LoadAsync(cancellationToken);

            return Result<UpdateContentResponse>.Success(
                new UpdateContentResponse(ToDetailResponse(content)));
        }

        public async Task<Result<DeleteContentResponse>> DeleteAsync(
            DeleteContentRequest request,
            CancellationToken cancellationToken)
        {
            if (request.ContentId <= 0)
                return Result<DeleteContentResponse>.ValidationError("Invalid content ID.");

            var content = await _db.Contents
                .FirstOrDefaultAsync(c => c.ContentId == request.ContentId, cancellationToken);

            if (content == null)
                return Result<DeleteContentResponse>.NotFoundError($"Content with ID {request.ContentId} was not found.");

            if (content.IsDeleted)
                return Result<DeleteContentResponse>.InvalidDataError("Content is already deleted.");

            content.IsDeleted = true;
            content.DeletedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);

            return Result<DeleteContentResponse>.Success(new DeleteContentResponse(request.ContentId, true));
        }

        private static ContentDetailResponse ToDetailResponse(Content c) =>
            new ContentDetailResponse(
                c.ContentId,
                c.ContentTypeId,
                c.Title,
                c.Description,
                c.PosterUrl,
                c.BannerUrl,
                c.ReleaseDate,
                c.Country,
                c.Language,
                c.ContentType != null ? c.ContentType.Name : string.Empty,
                c.Status,
                c.CreatedAt,
                c.UpdatedAt);
    }
}
