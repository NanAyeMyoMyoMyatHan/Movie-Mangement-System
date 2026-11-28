using DotNetProject.Database.AppDbModels;
using DotNetProject.Domain.Features.WatchLater;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetProject.WebApi.Features.WatchLater
{
    public class WatchLaterService : IWatchLaterService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<WatchLaterService> _logger;

        public WatchLaterService(AppDbContext db, ILogger<WatchLaterService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<List<WatchLaterItem>> GetWatchLaterByUserIdAsync(int userId)
        {
            _logger.LogInformation("[DotNetProject.WebApi] GetWatchLaterByUserIdAsync => Fetching watch later list. UserId={UserId}", userId);
            if (userId <= 0) return new List<WatchLaterItem>();

            var dbRows = await _db.UserWatchLaters
                .Where(w => w.UserId == userId && !w.IsDeleted)
                .OrderByDescending(w => w.AddedAt)
                .ToListAsync();

            return dbRows.Select(r => new WatchLaterItem
            {
                Id = r.ContentId,
                IsContent = r.IsContent,
                Title = r.Title,
                PosterUrl = r.PosterUrl,
                Category = r.Category,
                Description = r.Description,
                TotalEpisodes = r.TotalEpisodes,
                CurrentEpisode = r.CurrentEpisode,
                Status = r.Status,
                AddedAt = r.AddedAt
            }).ToList();
        }

        public async Task<bool> AddWatchLaterAsync(int userId, WatchLaterItem item)
        {
            _logger.LogInformation("[DotNetProject.WebApi] AddWatchLaterAsync => Adding watch later. UserId={UserId}, ContentId={ContentId}", userId, item.Id);
            if (userId <= 0) return false;

            var existing = await _db.UserWatchLaters
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ContentId == item.Id && w.IsContent == item.IsContent);

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

            _db.UserWatchLaters.Add(new UserWatchLater
            {
                UserId = userId,
                ContentId = item.Id,
                IsContent = item.IsContent,
                Title = item.Title ?? string.Empty,
                PosterUrl = item.PosterUrl,
                Category = item.Category,
                Description = item.Description,
                TotalEpisodes = item.TotalEpisodes,
                CurrentEpisode = item.CurrentEpisode,
                Status = item.Status,
                AddedAt = DateTime.UtcNow,
                IsDeleted = false
            });

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveWatchLaterAsync(int userId, long contentId, bool isContent)
        {
            _logger.LogInformation("[DotNetProject.WebApi] RemoveWatchLaterAsync => Removing watch later. UserId={UserId}, ContentId={ContentId}", userId, contentId);
            if (userId <= 0) return false;

            var existing = await _db.UserWatchLaters
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ContentId == contentId && w.IsContent == isContent && !w.IsDeleted);

            if (existing != null)
            {
                existing.IsDeleted = true;
                await _db.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> IsWatchLaterAsync(int userId, long contentId, bool isContent)
        {
            if (userId <= 0) return false;
            return await _db.UserWatchLaters
                .AnyAsync(w => w.UserId == userId && w.ContentId == contentId && w.IsContent == isContent && !w.IsDeleted);
        }
    }
}
