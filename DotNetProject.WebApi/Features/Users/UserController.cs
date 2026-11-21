using DotNetProject.Domain.Features.Users;
using DotNetProject.WebApi.Features.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DotNetProject.WebApi.Features.Users
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserInterface _userInterface;
        private readonly FileStorageService _fileStorageService;
        private readonly IWebHostEnvironment _env;

        public UserController(IWebHostEnvironment env, IUserInterface userInterface, FileStorageService fileStorageService)
        {
            _env = env;
            _userInterface = userInterface;
            _fileStorageService = fileStorageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users=await _userInterface.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUsersById(int id)
        {
            var users = await _userInterface.GetUserByIdAsync(id);
            return Ok(users);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUsers([FromBody] UserCreateRequestModel model)
        {
            var users = await _userInterface.CreateUserAsync(model);
            return Ok(users);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromBody] UserUpdateRequestModel model)
        {
            var users = await _userInterface.UpdateUserAsync(model);
            return Ok(users);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequestModel model)
        {
            var result = await _userInterface.LoginAsync(model);
            return Ok(result);
        }

        [HttpGet("profile/{id}")]
        public async Task<IActionResult> GetProfile(int id)
        {
            try
            {
                var profile = await _userInterface.GetProfileAsync(id);
                return Ok(profile);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserProfileUpdateRequestModel model)
        {
            var result = await _userInterface.UpdateProfileAsync(model);
            return Ok(result);
        }

        [HttpPost("profile/avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile avatarFile, [FromQuery] int userId)
        {
            if (avatarFile == null || avatarFile.Length == 0)
            {
                return BadRequest(new { isSuccess = false, message = "No image file provided." });
            }

            try
            {
                var filename = await _fileStorageService.SaveImageAsync(avatarFile);
                var avatarUrl = $"/storage/images/{filename}";
                await _userInterface.UpdateAvatarAsync(userId, avatarUrl);

                return Ok(new
                {
                    isSuccess = true,
                    message = "Avatar uploaded successfully.",
                    avatarUrl = avatarUrl
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestModel model)
        {
            var result = await _userInterface.ChangePasswordAsync(model);
            return Ok(result);
        }
    }
}
