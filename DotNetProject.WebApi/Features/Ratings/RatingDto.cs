namespace DotNetProject.WebApi.Features.Ratings
{
    public class RatingModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int MovieID { get; set; }
        public int Score { get; set; }
        public string Review { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
    public class RatingCreateRequestModel
    {
        public int UserId { get; set; }
        public int MovieID { get; set; }
        public int Score { get; set; }
        public string Review { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
    public class RatingCreateResponseModel
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
    public class RatingUpdateRequestModel
    {
        public int Id { get; set; }
        public int Score { get; set; }
        public string Review { get; set; }
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class RatingUpdateResponseModel
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
    public class RatingDeleteRequestModel
    {
        public int Id { get; set; }
    }

    public class RatingDeleteResponseModel
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}
