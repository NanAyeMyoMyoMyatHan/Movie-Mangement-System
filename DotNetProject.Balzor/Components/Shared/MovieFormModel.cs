using DotNetProject.Domain.Features.Movies;

namespace DotNetProject.Balzor.Components.Shared
{
    /// <summary>
    /// Shared mutable form state for admin movie create and edit pages.
    /// </summary>
    public sealed class MovieFormModel
    {
        public string? Title { get; set; }
        public string? Genre { get; set; }
        public int? ReleaseYear { get; set; }
        public string? Description { get; set; }
        public string? PosterUrl { get; set; }
        public int? TmdbId { get; set; }
        public bool IsManual { get; set; }
        public UploadedFilePayload? PosterFile { get; set; }

        public CreateMovieRequest ToCreateRequest() => new()
        {
            Title = Title,
            Genre = Genre,
            ReleaseYear = ReleaseYear,
            Description = Description,
            PosterUrl = PosterUrl,
            TmdbId = TmdbId,
            IsManual = IsManual
        };

        public UpdateMovieRequest ToUpdateRequest(int id) => new()
        {
            Id = id,
            Title = Title,
            Genre = Genre,
            ReleaseYear = ReleaseYear,
            Description = Description,
            PosterUrl = PosterUrl,
            TmdbId = TmdbId,
            IsManual = IsManual
        };

        public static MovieFormModel FromDetail(MovieDetailResponse d) => new()
        {
            Title = d.Title,
            Genre = d.Genre,
            ReleaseYear = d.ReleaseYear,
            Description = d.Description,
            PosterUrl = d.PosterUrl,
            TmdbId = d.TmdbId,
            IsManual = d.IsManual
        };
    }
}
