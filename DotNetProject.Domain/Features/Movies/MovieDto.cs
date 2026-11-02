namespace DotNetProject.Domain.Features.Movies
{
    // -- Existing models (unchanged) --------------------------------------------
    public class MovieModel
    {
        public long ContentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? PosterUrl { get; set; }
        public string? BannerUrl { get; set; }
        public DateOnly? ReleaseDate { get; set; }
        public string? Country { get; set; }
        public string? Language { get; set; }
        public int ContentTypeId { get; set; } 
        public string Status { get; set; } = string.Empty;
    }

    // -- New: Result-pattern request / response records -------------------------

    public sealed record GetMoviePagedListRequest
    {
        public int PageNo { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
    }

    public sealed record GetMovieDetailRequest
    {
        public int Id { get; set; }
    }

    public sealed record CreateMovieRequest
    {
        public string? Title { get; set; }
        public string? Genre { get; set; }
        public int? ReleaseYear { get; set; }
        public string? Description { get; set; }
        public string? PosterUrl { get; set; }
        public int? TmdbId { get; set; }
        public bool IsManual { get; set; }
    }

    public sealed record UpdateMovieRequest
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Genre { get; set; }
        public int? ReleaseYear { get; set; }
        public string? Description { get; set; }
        public string? PosterUrl { get; set; }
        public int? TmdbId { get; set; }
        public bool IsManual { get; set; }
    }

    public sealed record DeleteMovieRequest
    {
        public int Id { get; set; }
    }

    // -- Response records -------------------------------------------------------

    public sealed record MovieListItemResponse(
        long Id,
        string Title,
        int? ReleaseYear,
        string? PosterUrl);

    public sealed record MovieDetailResponse(
        int Id,
        string Title,
        string? Genre,
        int? ReleaseYear,
        string? Description,
        string? PosterUrl,
        int? TmdbId,
        bool IsManual,
        DateTimeOffset CreatedAt);

    public sealed record GetMoviePagedListResponse(
        IReadOnlyList<MovieListItemResponse> Items,
        int TotalCount,
        int PageNo,
        int PageSize);

    public sealed record GetMovieDetailResponse(MovieDetailResponse Movie);
    public sealed record CreateMovieResponse(MovieDetailResponse Movie);
    public sealed record UpdateMovieResponse(MovieDetailResponse Movie);
    public sealed record DeleteMovieResponse(int Id, bool IsDeleted);
}
