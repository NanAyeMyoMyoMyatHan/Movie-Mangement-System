using DotNetProject.Database.AppDbModels;
using DotNetProject.Domain.Features.WatchList;
using Microsoft.EntityFrameworkCore;

namespace DotNetProject.WebApi.Features.WatchList
{
    public class WatchListServices : IWatchListInterface
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public WatchListServices(IWebHostEnvironment env, AppDbContext db)
        {
            _env = env;
            _db = db;
        }

        public async Task<WatchListModel> GetWatchListByUserIdAsync(int userId)
        {
            var watchList = await _db.WatchlistItems
                .FirstOrDefaultAsync(w => w.UserId == userId && !w.IsDeleted);
            if (watchList == null)
            {
                throw new Exception("Watch list not found for the given user ID.");
            }
            return new WatchListModel
            {
                Id = watchList.Id,
                UserId = watchList.UserId,
                MovieId = watchList.MovieId,
                Status = (WatchStatus)watchList.Status,
                CurrentEpisode = watchList.CurrentEpisode,
                TotalEpisodes = watchList.TotalEpisodes
            };
        }
        public async Task<WatchListModel?> GetWatchListByIdAsync(int id)
        {
            var watchList = await _db.WatchlistItems.FindAsync(id);
            if (watchList == null || watchList.IsDeleted)
            {
                return null;
            }
            return new WatchListModel
            {
                Id = watchList.Id,
                UserId = watchList.UserId,
                MovieId = watchList.MovieId,
                Status = (WatchStatus)watchList.Status,
                CurrentEpisode = watchList.CurrentEpisode,
                TotalEpisodes = watchList.TotalEpisodes
            };
        }
        public async Task<WatchListCreateResponseModel> CreateWatchListAsync(int userId, WatchListCreateRequestModel model)
        {
            var watchList = new WatchlistItem
            {
                UserId = userId,
                MovieId = model.MovieId,
                Status = (int)model.Status,
                CurrentEpisode = model.CurrentEpisode,
                TotalEpisodes = model.TotalEpisodes
            };
            _db.WatchlistItems.Add(watchList);
            await _db.SaveChangesAsync();
            return new WatchListCreateResponseModel
            {
                IsSuccess = true,
                Message = "Watch list created successfully."
            };
        }
        public async Task<WatchListUpdateResponseModel> UpdateWatchListAsync(int id, WatchListUpdateRequestModel model)
        {
            var watchList = await _db.WatchlistItems.FindAsync(id);
            if (watchList == null)
            {
                return new WatchListUpdateResponseModel
                {
                    IsSuccess = false,
                    Message = "Watch list not found."
                };
            }
            watchList.Status = (int)model.Status;
            watchList.CurrentEpisode = model.CurrentEpisode;
            watchList.TotalEpisodes = model.TotalEpisodes;
            await _db.SaveChangesAsync();
            return new WatchListUpdateResponseModel
            {
                IsSuccess = true,
                Message = "Watch list updated successfully."
            };
        }
        public async Task<WatchListDeleteResponseModel> DeleteWatchListAsync(int id)
        {
            var watchList = await _db.WatchlistItems.FindAsync(id);
            if (watchList == null)
            {
                return new WatchListDeleteResponseModel
                {
                    IsSuccess = false,
                    Message = "Watch list not found."
                };
            }
            watchList.IsDeleted = true;
            watchList.DeletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return new WatchListDeleteResponseModel
            {
                IsSuccess = true,
                Message = "Watch list deleted successfully."
            };
        }
    }
}
