using DotNetProject.Domain.Features.Movies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DotNetProject.WebApi.Features.Movies
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovieController : ControllerBase
    {
        private readonly IMoviesInterface _movieService;
        private readonly IWebHostEnvironment _environment;
        public MovieController(IMoviesInterface movieService, IWebHostEnvironment environment)
        {
            _movieService = movieService;
            _environment = environment;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllMovies()
        {
            var movies = await _movieService.GetAllMoviesAsync();
            return Ok(movies);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMovieById(int id)
        {
            var movie = await _movieService.GetMovieByIdAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return Ok(movie);
        }
        [HttpGet("genre/{genre}")]
        public async Task<IActionResult> GetMoviesByGenre(string genre)
        {
            var movies = await _movieService.GetMoviesByGenreAsync(genre);
            return Ok(movies);
        }
        [HttpPost]
        public async Task<IActionResult> CreateMovie([FromBody]MovieCreateRequestModel model)
        {
            var movie = await _movieService.CreateMoviesAsync(model);
            return Ok(movie);
        }
        [HttpPut]
        public async Task<IActionResult> UpdateMovie([FromBody]MovieUpdateRequestModel model)
        {
            var movie = await _movieService.UpdateMovieAsync(model);
            return Ok(movie);
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteMovie([FromBody]MovieDeleteRequestModel model)
        {
            var movie = await _movieService.DeleteMovieAsync(model);
            return Ok(movie);
        }
    }
}
