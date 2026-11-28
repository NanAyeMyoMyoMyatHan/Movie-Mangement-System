using DotNetProject.Database.AppDbModels;
using DotNetProject.Domain.Features.WatchHistory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetProject.WebApi.Features.WatchHistory
{
    public class WatchHistoryService : IWatchHistoryService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<WatchHistoryService> _logger;

        public WatchHistoryService(AppDbContext db, ILogger<WatchHistoryService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<List<WatchHistoryItem>> GetHistoryByUserIdAsync(int userId)
        {
            _logger.LogInformation("[DotNetProject.WebApi] GetHistoryByUserIdAsync => Fetching watch history. UserId={UserId}", userId);
            if (userId <= 0) return new List<WatchHistoryItem>();

            var dbRows = await _db.UserContinueWatchings
                .Where(w => w.UserId == userId && !w.IsDeleted)
                .OrderByDescending(w => w.LastWatchedAt)
                .ToListAsync();

            return dbRows.Select(r =>
            {
                int progressPercent = 0;
                if (r.DurationSeconds > 0 && r.ProgressSeconds > 0)
                {
                    progressPercent = (int)Math.Min(100, Math.Round((double)r.ProgressSeconds / r.DurationSeconds * 100));
                }

                return new WatchHistoryItem
                {
                    ContentId = r.ContentId,
                    IsContent = r.IsContent,
                    Title = r.Title,
                    PosterUrl = r.PosterUrl,
                    EpisodeNumber = r.EpisodeNumber,
                    TotalEpisodes = r.TotalEpisodes,
                    ProgressSeconds = r.ProgressSeconds,
                    DurationSeconds = r.DurationSeconds,
                    ProgressPercentage = progressPercent,
                    LastWatchedAt = r.LastWatchedAt
                };
            }).ToList();
        }

        public async Task<WatchHistoryItem?> GetItemAsync(int userId, long contentId, bool isContent, int episodeNumber = 1)
        {
            if (userId <= 0) return null;

            var r = await _db.UserContinueWatchings
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ContentId == contentId && w.IsContent == isContent && w.EpisodeNumber == episodeNumber && !w.IsDeleted);

            if (r == null) return null;

            int progressPercent = 0;
            if (r.DurationSeconds > 0 && r.ProgressSeconds > 0)
            {
                progressPercent = (int)Math.Min(100, Math.Round((double)r.ProgressSeconds / r.DurationSeconds * 100));
            }

            return new WatchHistoryItem
            {
                ContentId = r.ContentId,
                IsContent = r.IsContent,
                Title = r.Title,
                PosterUrl = r.PosterUrl,
                EpisodeNumber = r.EpisodeNumber,
                TotalEpisodes = r.TotalEpisodes,
                ProgressSeconds = r.ProgressSeconds,
                DurationSeconds = r.DurationSeconds,
                ProgressPercentage = progressPercent,
                LastWatchedAt = r.LastWatchedAt
            };
        }

        public async Task<bool> SaveProgressAsync(int userId, WatchHistoryItem item)
        {
            _logger.LogInformation("[DotNetProject.WebApi] SaveProgressAsync => Saving progress. UserId={UserId}, ContentId={ContentId}, ProgressSec={ProgressSec}", userId, item.ContentId, item.ProgressSeconds);
            if (userId <= 0) return false;

            var existing = await _db.UserContinueWatchings
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ContentId == item.ContentId && w.IsContent == item.IsContent && w.EpisodeNumber == item.EpisodeNumber);

            if (existing != null)
            {
                existing.Title = !string.IsNullOrWhiteSpace(item.Title) ? item.Title : existing.Title;
                existing.PosterUrl = item.PosterUrl ?? existing.PosterUrl;
                existing.Category = item.Description ?? existing.Category;
                existing.EpisodeNumber = item.EpisodeNumber;
                existing.TotalEpisodes = item.TotalEpisodes > 0 ? item.TotalEpisodes : existing.TotalEpisodes;
                existing.ProgressSeconds = item.ProgressSeconds;
                existing.DurationSeconds = item.DurationSeconds;
                existing.LastWatchedAt = DateTime.UtcNow;
                existing.IsDeleted = false;
            }
            else
            {
                _db.UserContinueWatchings.Add(new UserContinueWatching
                {
                    UserId = userId,
                    ContentId = item.ContentId,
                    IsContent = item.IsContent,
                    Title = item.Title ?? string.Empty,
                    PosterUrl = item.PosterUrl,
                    Category = item.Description,
                    EpisodeNumber = item.EpisodeNumber,
                    TotalEpisodes = item.TotalEpisodes > 0 ? item.TotalEpisodes : 1,
                    ProgressSeconds = item.ProgressSeconds,
                    DurationSeconds = item.DurationSeconds,
                    LastWatchedAt = DateTime.UtcNow,
                    IsDeleted = false
                });
            }

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveHistoryAsync(int userId, long contentId, bool isContent)
        {
            _logger.LogInformation("[DotNetProject.WebApi] RemoveHistoryAsync => Removing history. UserId={UserId}, ContentId={ContentId}", userId, contentId);
            if (userId <= 0) return false;

            var records = await _db.UserContinueWatchings
                .Where(w => w.UserId == userId && w.ContentId == contentId && w.IsContent == isContent && !w.IsDeleted)
                .ToListAsync();

            if (records.Any())
            {
                foreach (var r in records) r.IsDeleted = true;
                await _db.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> ClearHistoryAsync(int userId)
        {
            _logger.LogInformation("[DotNetProject.WebApi] ClearHistoryAsync => Clearing all history for UserId={UserId}", userId);
            if (userId <= 0) return false;

            var records = await _db.UserContinueWatchings
                .Where(w => w.UserId == userId && !w.IsDeleted)
                .ToListAsync();

            if (records.Any())
            {
                foreach (var r in records) r.IsDeleted = true;
                await _db.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}
