using System;
using System.Collections.Generic;

namespace DotNetProject.Domain.Features.Tmdb
{
    public class TmdbSearchResultDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string OriginalTitle { get; set; } = string.Empty;
        public string Overview { get; set; } = string.Empty;
        public string PosterUrl { get; set; } = string.Empty;
        public string BannerUrl { get; set; } = string.Empty;
        public string ReleaseDate { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public double VoteAverage { get; set; }
        public string MediaType { get; set; } = "tv"; // "tv" or "movie"
        public bool IsFromTmdb { get; set; } = true;
        public bool AlreadyInDatabase { get; set; } = false;
        public long ExistingContentId { get; set; } = 0;

        public int? ReleaseYear =>
            DateOnly.TryParse(ReleaseDate, out var d) ? d.Year
            : ReleaseDate.Length >= 4 && int.TryParse(ReleaseDate[..4], out var y) ? y
            : null;
    }

    public class TmdbMovieDetailsDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string OriginalTitle { get; set; } = string.Empty;
        public string? Genre { get; set; }
        public List<string> Genres { get; set; } = new();
        public int? ReleaseYear { get; set; }
        public DateOnly? ReleaseDate { get; set; }
        public string? Description { get; set; }
        public string? PosterUrl { get; set; }
        public string? BannerUrl { get; set; }
        public string Country { get; set; } = "United States";
        public string Language { get; set; } = "English";
        public int? Runtime { get; set; }
        public double VoteAverage { get; set; }
        public string? TrailerUrl { get; set; }
    }

    public sealed record TmdbLookupResponse<T>(bool IsSuccess, T? Data, string? ErrorMessage);

    public class TmdbImportResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public long ContentId { get; set; }
        public int EpisodeCount { get; set; }
    }

    public class TmdbImportRequest
    {
        public int TmdbId { get; set; }
        public string? CustomCategory { get; set; }
    }
}
