using DotNetProject.Database.AppDbModels;
using DotNetProject.Domain.Features.Favorites;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetProject.WebApi.Features.Favorites
{
    public class FavoriteService : IFavoriteService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<FavoriteService> _logger;

        public FavoriteService(AppDbContext db, ILogger<FavoriteService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<List<FavoriteItem>> GetFavoritesByUserIdAsync(int userId)
        {
            _logger.LogInformation("[DotNetProject.WebApi] GetFavoritesByUserIdAsync => Fetching favorites for UserId={UserId}", userId);
            if (userId <= 0) return new List<FavoriteItem>();

            var dbRows = await _db.UserFavorites
                .Where(f => f.UserId == userId && !f.IsDeleted)
                .OrderByDescending(f => f.AddedAt)
                .ToListAsync();

            return dbRows.Select(r => new FavoriteItem
            {
                Id = r.ContentId,
                IsContent = r.IsContent,
                Title = r.Title,
                PosterUrl = r.PosterUrl,
                Category = r.Category,
                Description = r.Description,
                Year = r.Year ?? "2024",
                Rating = r.Rating,
                TotalEpisodes = r.TotalEpisodes,
                AddedAt = r.AddedAt
            }).ToList();
        }

        public async Task<bool> AddFavoriteAsync(int userId, FavoriteItem item)
        {
            _logger.LogInformation("[DotNetProject.WebApi] AddFavoriteAsync => Adding favorite. UserId={UserId}, ContentId={ContentId}", userId, item.Id);
            if (userId <= 0) return false;

            var existing = await _db.UserFavorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ContentId == item.Id && f.IsContent == item.IsContent);

            if (existing != null)
            {
                if (existing.IsDeleted)
                {
                    existing.IsDeleted = false;
                    existing.AddedAt = DateTime.UtcNow;
                    await _db.SaveChangesAsync();
                }
                return true;
            }

            _db.UserFavorites.Add(new UserFavorite
            {
                UserId = userId,
                ContentId = item.Id,
                IsContent = item.IsContent,
                Title = item.Title ?? string.Empty,
                PosterUrl = item.PosterUrl,
                Category = item.Category,
                Description = item.Description,
                Year = item.Year,
                Rating = item.Rating,
                TotalEpisodes = item.TotalEpisodes,
                AddedAt = DateTime.UtcNow,
                IsDeleted = false
            });

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveFavoriteAsync(int userId, long contentId, bool isContent)
        {
            _logger.LogInformation("[DotNetProject.WebApi] RemoveFavoriteAsync => Removing favorite. UserId={UserId}, ContentId={ContentId}", userId, contentId);
            if (userId <= 0) return false;

            var existing = await _db.UserFavorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ContentId == contentId && f.IsContent == isContent && !f.IsDeleted);

            if (existing != null)
            {
                existing.IsDeleted = true;
                await _db.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> IsFavoriteAsync(int userId, long contentId, bool isContent)
        {
            if (userId <= 0) return false;
            return await _db.UserFavorites
                .AnyAsync(f => f.UserId == userId && f.ContentId == contentId && f.IsContent == isContent && !f.IsDeleted);
        }
    }
}
