namespace DotNetProject.WebApi.Features.Users
{
    public interface IUserInterface
    {
        Task<UserModel> GetUserByIdAsync(int id);
        Task<List<UserModel>> GetAllUsersAsync();
        Task<UserCreateResponseModel> CreateUserAsync(UserCreateRequestModel model);
        Task<UserUpdateResponseModel> UpdateUserAsync(UserUpdateRequestModel model);
        Task<UserLoginResponseModel> LoginAsync(UserLoginRequestModel model);
    }
}
