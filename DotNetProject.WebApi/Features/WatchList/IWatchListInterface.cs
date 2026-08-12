namespace DotNetProject.WebApi.Features.WatchList
{
    public interface IWatchListInterface
    {
        
        Task<WatchListModel> GetWatchListByUserIdAsync(int userId);
        Task<WatchListModel?> GetWatchListByIdAsync(int id);
        Task<WatchListCreateResponseModel> CreateWatchListAsync(int userId,WatchListCreateRequestModel model);
        Task<WatchListUpdateResponseModel> UpdateWatchListAsync(int id, WatchListUpdateRequestModel model);
        Task<WatchListDeleteResponseModel> DeleteWatchListAsync(int id);
    }
}
