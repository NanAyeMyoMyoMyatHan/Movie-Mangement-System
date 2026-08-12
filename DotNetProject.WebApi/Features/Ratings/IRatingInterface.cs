namespace DotNetProject.WebApi.Features.Ratings
{
    public interface IRatingInterface
    {
        Task<List<RatingModel>> GetRatingAsync(int userId);
        Task<RatingCreateResponseModel> CreateRatingAsync(RatingCreateRequestModel model);
        Task<RatingUpdateResponseModel> UpdateRatingAsync(RatingUpdateRequestModel model);
        Task<RatingDeleteResponseModel> DeleteRaingAsync(RatingDeleteRequestModel model);
    }
}
