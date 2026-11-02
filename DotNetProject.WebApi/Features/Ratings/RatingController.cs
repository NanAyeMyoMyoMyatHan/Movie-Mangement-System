using DotNetProject.Domain.Features.Ratings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DotNetProject.WebApi.Features.Ratings
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatingController : ControllerBase
    {
        private readonly IRatingInterface _ratingInterface;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public RatingController(IWebHostEnvironment webHostEnvironment, IRatingInterface ratingInterface)
        {
            _webHostEnvironment = webHostEnvironment;
            _ratingInterface = ratingInterface;
        }
        [HttpGet("user/{userid}")]
        public async Task<IActionResult> GetRating (int userid)
        {
            var rating =await _ratingInterface.GetRatingAsync(userid);
            return Ok(rating);
        }
        [HttpPost]
        public async Task<IActionResult> CreateRating([FromBody]RatingCreateRequestModel model)
        {
            var rating = await _ratingInterface.CreateRatingAsync(model);
            return Ok(rating);
        }
        [HttpPut]
        public async Task<IActionResult> UpdateRating([FromBody]RatingUpdateRequestModel model)
        {
            var rating = await _ratingInterface.UpdateRatingAsync(model);
            return Ok(rating);
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteRating([FromBody] RatingDeleteRequestModel model)
        {
            var rating = await _ratingInterface.DeleteRaingAsync(model); 
            return Ok(rating);
        }
    }
}
