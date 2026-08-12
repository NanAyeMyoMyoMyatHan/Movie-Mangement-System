namespace DotNetProject.WebApi.Features.WatchList
{
    public class WatchListModel
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int MovieId { get; set; }

        public WatchStatus Status { get; set; }

        public int CurrentEpisode { get; set; }

        public int TotalEpisodes { get; set; }

        public DateTime AddedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }
    }

    public class WatchListCreateRequestModel
    {
        public int MovieId { get; set; }
        public WatchStatus Status { get; set; }
        public int CurrentEpisode { get; set; }
        public int TotalEpisodes { get; set; }
    }

    public class WatchListCreateResponseModel
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
    }
    public class WatchListUpdateRequestModel
    {
        
        public WatchStatus Status { get; set; }
        public int CurrentEpisode { get; set; }
        public int TotalEpisodes { get; set; }
    }

    public class WatchListUpdateResponseModel
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
    }
    public class WatchListDeleteRequestModel
    {
        public int MovieId { get; set; }
    }

    public class WatchListDeleteResponseModel
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public enum WatchStatus
    {
        WantToWatch = 1,
        Watching = 2,
        Completed = 3,
        Dropped = 4
    }
}
