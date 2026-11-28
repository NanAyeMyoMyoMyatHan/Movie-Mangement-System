using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNetProject.Domain.Features.Favorites
{
    public interface IFavoriteService
    {
        Task<List<FavoriteItem>> GetFavoritesByUserIdAsync(int userId);
        Task<bool> AddFavoriteAsync(int userId, FavoriteItem item);
        Task<bool> RemoveFavoriteAsync(int userId, long contentId, bool isContent);
        Task<bool> IsFavoriteAsync(int userId, long contentId, bool isContent);
    }
}
