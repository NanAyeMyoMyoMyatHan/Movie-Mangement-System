using DotNetProject.Domain.Features.Favorites;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DotNetProject.WebApi.Features.Favorites
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavoritesController : ControllerBase
    {
        private readonly IFavoriteService _favoriteService;

        public FavoritesController(IFavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetFavorites(int userId)
        {
            var res = await _favoriteService.GetFavoritesByUserIdAsync(userId);
            return Ok(res);
        }

        [HttpPost("user/{userId}")]
        public async Task<IActionResult> AddFavorite(int userId, [FromBody] FavoriteItem item)
        {
            var res = await _favoriteService.AddFavoriteAsync(userId, item);
            return Ok(new { isSuccess = res });
        }

        [HttpDelete("user/{userId}")]
        public async Task<IActionResult> RemoveFavorite(int userId, [FromQuery] long contentId, [FromQuery] bool isContent = true)
        {
            var res = await _favoriteService.RemoveFavoriteAsync(userId, contentId, isContent);
            return Ok(new { isSuccess = res });
        }

        [HttpGet("check")]
        public async Task<IActionResult> IsFavorite([FromQuery] int userId, [FromQuery] long contentId, [FromQuery] bool isContent = true)
        {
            var res = await _favoriteService.IsFavoriteAsync(userId, contentId, isContent);
            return Ok(new { isFavorite = res });
        }
    }
}
