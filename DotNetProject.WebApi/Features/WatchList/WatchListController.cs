using DotNetProject.Domain.Features.WatchList;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DotNetProject.WebApi.Features.WatchList
{
    [Route("api/[controller]")]
    [ApiController]
    public class WatchListController : ControllerBase
    {
        private readonly IWatchListInterface _watchListInterface;
        private readonly IWebHostEnvironment _env;

        public WatchListController(IWatchListInterface watchListInterface, IWebHostEnvironment env = null)
        {
            _watchListInterface = watchListInterface;
            _env = env;
        }
        [HttpGet("UserId")]
        public async Task<IActionResult> GetAllWatchListByUserId(int userId)
        {
            var watchlist = await _watchListInterface.GetWatchListByUserIdAsync(userId);
            if(watchlist == null)
            {
                return NotFound();
            }
            return Ok(watchlist);
        }
        [HttpGet]
        public async Task<IActionResult> GetAllWatchListById(int id)
        {
            var watchlist = await _watchListInterface.GetWatchListByIdAsync(id);
            if (watchlist == null)
            {
                return NotFound();
            }
            return Ok(watchlist);
                
        }
        [HttpPost("userid")]
        public async Task<IActionResult> CreateWatchList(int userid,WatchListCreateRequestModel model)
        {
            var watchlist = await _watchListInterface.CreateWatchListAsync(userid,model);
            return Ok(watchlist);
        }
        [HttpPut("id")]
        public async Task<IActionResult>UpdateWatchList(int id,WatchListUpdateRequestModel model)
        {
            var watchlist = await _watchListInterface.UpdateWatchListAsync(id,model);
            return Ok(watchlist);
        }
        [HttpDelete("id")]
        public async Task<IActionResult>DeleteWatchlist(int id)
        {
            var watchlist = await _watchListInterface.DeleteWatchListAsync(id);
            return Ok(watchlist);
        }
    }
}
