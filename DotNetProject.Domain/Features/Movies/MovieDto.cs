namespace DotNetProject.Domain.Features.Movies
{
    public class MovieModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Genre { get; set; }
        public int? ReleaseYear { get; set; }
        public string Description { get; set; }
        public string PosterUrl { get; set; }
        public int? TmdbId { get; set; }
        public bool IsManual { get; set; }
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }

    public class MovieCreateRequestModel
    {
        public string Title { get; set; }
        public string Genre { get; set; }
        public int? ReleaseYear { get; set; }
        public string Description { get; set; }
        public string PosterUrl { get; set; }
        public int? TmdbId { get; set; }
        public bool IsManual { get; set; }
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    }

    public class MovieCreateResponseModel
    {
        public string IsSuccess { get; set; }
        public string Message { get; set; }
    }

    public class MovieDeleteRequestModel
    {
        public int Id { get; set; }
    }

    public class MovieDeleteResponseModel
    {
        public string IsSuccess { get; set; }
        public string Message { get; set; }
    }

    public class MovieUpdateRequestModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Genre { get; set; }
        public int? ReleaseYear { get; set; }
        public string Description { get; set; }
        public string PosterUrl { get; set; }
        public int? TmdbId { get; set; }
        public bool IsManual { get; set; }
    }

    public class MovieUpdateResponseModel
    {
        public string IsSuccess { get; set; }
        public string Message { get; set; }
    }
}
