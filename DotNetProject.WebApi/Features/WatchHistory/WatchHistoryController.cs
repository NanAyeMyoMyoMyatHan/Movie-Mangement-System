using DotNetProject.Domain.Features.WatchHistory;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DotNetProject.WebApi.Features.WatchHistory
{
    [Route("api/[controller]")]
    [ApiController]
    public class WatchHistoryController : ControllerBase
    {
        private readonly IWatchHistoryService _watchHistoryService;

        public WatchHistoryController(IWatchHistoryService watchHistoryService)
        {
            _watchHistoryService = watchHistoryService;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetHistory(int userId)
        {
            var res = await _watchHistoryService.GetHistoryByUserIdAsync(userId);
            return Ok(res);
        }

        [HttpGet("item")]
        public async Task<IActionResult> GetItem([FromQuery] int userId, [FromQuery] long contentId, [FromQuery] bool isContent = true, [FromQuery] int episodeNumber = 1)
        {
            var res = await _watchHistoryService.GetItemAsync(userId, contentId, isContent, episodeNumber);
            return Ok(res);
        }

        [HttpPost("user/{userId}")]
        public async Task<IActionResult> SaveProgress(int userId, [FromBody] WatchHistoryItem item)
        {
            var res = await _watchHistoryService.SaveProgressAsync(userId, item);
            return Ok(new { isSuccess = res });
        }

        [HttpDelete("user/{userId}")]
        public async Task<IActionResult> RemoveHistory(int userId, [FromQuery] long contentId, [FromQuery] bool isContent = true)
        {
            var res = await _watchHistoryService.RemoveHistoryAsync(userId, contentId, isContent);
            return Ok(new { isSuccess = res });
        }

        [HttpDelete("user/{userId}/clear")]
        public async Task<IActionResult> ClearHistory(int userId)
        {
            var res = await _watchHistoryService.ClearHistoryAsync(userId);
            return Ok(new { isSuccess = res });
        }
    }
}
