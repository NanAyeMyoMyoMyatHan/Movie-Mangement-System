using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNetProject.Domain.Features.WatchHistory
{
    public interface IWatchHistoryService
    {
        Task<List<WatchHistoryItem>> GetHistoryByUserIdAsync(int userId);
        Task<WatchHistoryItem?> GetItemAsync(int userId, long contentId, bool isContent, int episodeNumber = 1);
        Task<bool> SaveProgressAsync(int userId, WatchHistoryItem item);
        Task<bool> RemoveHistoryAsync(int userId, long contentId, bool isContent);
        Task<bool> ClearHistoryAsync(int userId);
    }
}
