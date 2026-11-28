using DotNetProject.Domain.Features.WatchLater;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DotNetProject.WebApi.Features.WatchLater
{
    [Route("api/[controller]")]
    [ApiController]
    public class WatchLaterController : ControllerBase
    {
        private readonly IWatchLaterService _watchLaterService;

        public WatchLaterController(IWatchLaterService watchLaterService)
        {
            _watchLaterService = watchLaterService;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetWatchLater(int userId)
        {
            var res = await _watchLaterService.GetWatchLaterByUserIdAsync(userId);
            return Ok(res);
        }

        [HttpPost("user/{userId}")]
        public async Task<IActionResult> AddWatchLater(int userId, [FromBody] WatchLaterItem item)
        {
            var res = await _watchLaterService.AddWatchLaterAsync(userId, item);
            return Ok(new { isSuccess = res });
        }

        [HttpDelete("user/{userId}")]
        public async Task<IActionResult> RemoveWatchLater(int userId, [FromQuery] long contentId, [FromQuery] bool isContent = true)
        {
            var res = await _watchLaterService.RemoveWatchLaterAsync(userId, contentId, isContent);
            return Ok(new { isSuccess = res });
        }

        [HttpGet("check")]
        public async Task<IActionResult> IsWatchLater([FromQuery] int userId, [FromQuery] long contentId, [FromQuery] bool isContent = true)
        {
            var res = await _watchLaterService.IsWatchLaterAsync(userId, contentId, isContent);
            return Ok(new { isWatchLater = res });
        }
    }
}
