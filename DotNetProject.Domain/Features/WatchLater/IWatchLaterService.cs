using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNetProject.Domain.Features.WatchLater
{
    public interface IWatchLaterService
    {
        Task<List<WatchLaterItem>> GetWatchLaterByUserIdAsync(int userId);
        Task<bool> AddWatchLaterAsync(int userId, WatchLaterItem item);
        Task<bool> RemoveWatchLaterAsync(int userId, long contentId, bool isContent);
        Task<bool> IsWatchLaterAsync(int userId, long contentId, bool isContent);
    }
}
