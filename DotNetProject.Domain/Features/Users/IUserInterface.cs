namespace DotNetProject.Domain.Features.Users
{
    public interface IUserInterface
    {
        Task<UserModel> GetUserByIdAsync(int id);
        Task<List<UserModel>> GetAllUsersAsync();
        Task<UserCreateResponseModel> CreateUserAsync(UserCreateRequestModel model);
        Task<UserUpdateResponseModel> UpdateUserAsync(UserUpdateRequestModel model);
        Task<UserLoginResponseModel> LoginAsync(UserLoginRequestModel model);
        Task<UserProfileDto> GetProfileAsync(int id);
        Task<UserProfileUpdateResponseModel> UpdateProfileAsync(UserProfileUpdateRequestModel model);
        Task<ChangePasswordResponseModel> ChangePasswordAsync(ChangePasswordRequestModel model);
        Task<string> UpdateAvatarAsync(int userId, string avatarUrl);
    }
}
