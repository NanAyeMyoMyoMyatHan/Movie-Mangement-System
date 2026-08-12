using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DotNetProject.WebApi.Features.Users
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserInterface _userInterface;
        private readonly IWebHostEnvironment _env;

        public UserController(IWebHostEnvironment env, IUserInterface userInterface)
        {
            _env = env;
            _userInterface = userInterface;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users=await _userInterface.GetAllUsersAsync();
            return Ok(users);
        }
        [HttpGet("id")]
        public async Task<IActionResult>GetUsersById(int id)
        {
            var users = await _userInterface.GetUserByIdAsync(id);
            return Ok(users);
        }
        [HttpPost]
        public async Task<IActionResult>CreateUsers(UserCreateRequestModel model)
        {
            var users = await _userInterface.CreateUserAsync(model);
            return Ok(users);
        }
        [HttpPut]
        public async Task<IActionResult>UpdateUser(UserUpdateRequestModel model)
        {
            var users = await _userInterface.UpdateUserAsync(model);
            return Ok(users);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginRequestModel model)
        {
            var result = await _userInterface.LoginAsync(model);
            return Ok(result);
        }
    }
}
