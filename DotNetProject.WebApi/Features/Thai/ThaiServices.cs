using DotNetProject.Domain.Features.Thai;
using Common;
using DotNetProject.Database.AppDbModels;
using Microsoft.EntityFrameworkCore;
namespace DotNetProject.WebApi.Features.Thai
{
    public class ThaiServices : IThaiInterface
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ThaiServices(AppDbContext context, IWebHostEnvironment? environment = null)
        {
            _environment = environment;
            _context = context;
        }

        public async Task<Result<GetThaiPagedListResponse>> GetPagedListAsync(GetThaiPagedListRequest request, CancellationToken cancellationToken)
        {
            var pageNo = request.PageNo < 1 ? 1 : request.PageNo;
            var pageSize = request.PageSize < 1 ? 10 : request.PageSize > 100 ? 100 : request.PageSize;

            var query = _context.Contents.AsNoTracking().Where(c => c.IsDeleted == false && c.ContentTypeId == 3);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.Trim().ToLower();
                query = query.Where(c => c.Title.Contains(searchTerm));
            }
            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(c => c.Title)
                .ThenBy(c => c.ContentId)
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new ThaiListItemResponse(
                    c.ContentId,
                    c.Title,
                    c.ReleaseDate.HasValue ? c.ReleaseDate.Value.Year : (int?)null,
                    c.PosterUrl
                ))
                    .ToListAsync(cancellationToken);

            return Result<GetThaiPagedListResponse>.Success(new GetThaiPagedListResponse(items, totalCount, pageNo, pageSize));
        }
        public async Task<Result<ThaiDetailResponse>> GetDetailAsync(GetThaiDetailRequest request, CancellationToken cancellationToken)
        {
            var content = await _context.Contents.AsNoTracking()
                .Where(c => c.ContentId == request.Id && c.IsDeleted == false && c.ContentTypeId == 3)
                .Select(c => new ThaiDetailResponse(
                    c.ContentId,
                    c.Title,
                    c.ReleaseDate.HasValue ? c.ReleaseDate.Value.Year : (int?)null,
                    c.Description,
                    c.PosterUrl,
                    c.CreatedAt
                ))
                .FirstOrDefaultAsync(cancellationToken);
            if (content == null)
            {
                return Result<ThaiDetailResponse>.NotFoundError("Content not found.");
            }
            return Result<ThaiDetailResponse>.Success(content);
        }
        public async Task<Result<CreateThaiResponse>> CreateAsync(CreateThaiReques request, CancellationToken cancellationToken)
        {
            var content = new Content
            {
                Title = request.Title,
                Description = request.Description,
                PosterUrl = request.PosterUrl,
                ReleaseDate = request.ReleaseYear.HasValue ? new DateOnly(request.ReleaseYear.Value, 1, 1) : null,
                ContentTypeId = 3,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Contents.Add(content);
            await _context.SaveChangesAsync(cancellationToken);
            var response = new ThaiDetailResponse(
                content.ContentId,
                content.Title,
                content.ReleaseDate.HasValue ? content.ReleaseDate.Value.Year : (int?)null,
                content.Description,
                content.PosterUrl,
                content.CreatedAt
            );
            return Result<CreateThaiResponse>.Success(new CreateThaiResponse(response));
        }
        public async Task<Result<UpdateThaiResponse>> UpdateAsync(UpdateThaiRequest request, CancellationToken cancellationToken)
        {
            var content = await _context.Contents.FirstOrDefaultAsync(c => c.ContentId == request.Id && c.IsDeleted == false && c.ContentTypeId == 3, cancellationToken);
            if (content == null)
            {
                return Result<UpdateThaiResponse>.NotFoundError("Content not found.");
            }
            content.Title = request.Title ?? content.Title;
            content.Description = request.Description ?? content.Description;
            content.PosterUrl = request.PosterUrl ?? content.PosterUrl;
            content.ReleaseDate = request.ReleaseYear.HasValue ? new DateOnly(request.ReleaseYear.Value, 1, 1) : content.ReleaseDate;
            await _context.SaveChangesAsync(cancellationToken);
            var response = new ThaiDetailResponse(
                content.ContentId,
                content.Title,
                content.ReleaseDate.HasValue ? content.ReleaseDate.Value.Year : (int?)null,
                content.Description,
                content.PosterUrl,
                content.CreatedAt
            );
            return Result<UpdateThaiResponse>.Success(new UpdateThaiResponse(response));
        }
        public async Task<Result<DeleteThaiResponse>> DeleteAsync(DeleteThaiRequest request, CancellationToken cancellationToken)
        {
            var content = await _context.Contents.FirstOrDefaultAsync(c => c.ContentId == request.Id && c.IsDeleted == false && c.ContentTypeId == 3, cancellationToken);
            if (content == null)
            {
                return Result<DeleteThaiResponse>.NotFoundError("Content not found.");
            }
            content.IsDeleted = true;
            await _context.SaveChangesAsync(cancellationToken);
            return Result<DeleteThaiResponse>.Success(new DeleteThaiResponse(content.ContentId, true));
        }
    }
}

