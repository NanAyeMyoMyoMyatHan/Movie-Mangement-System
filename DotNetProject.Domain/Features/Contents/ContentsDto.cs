using System;
using System.Collections.Generic;

namespace DotNetProject.Domain.Features.Contents
{
    // --- Legacy DTOs (preserved) ---
    public class ContentsDto
    {
        public long ContentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? PosterUrl { get; set; }
        public string? BannerUrl { get; set; }
        public DateOnly? ReleaseDate { get; set; }
        public string? Country { get; set; }
        public string? Language { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class ContentDetailDto
    {
        public long ContentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? PosterUrl { get; set; }
        public string? BannerUrl { get; set; }
        public DateOnly? ReleaseDate { get; set; }
        public string? Country { get; set; }
        public string? Language { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<string> Genres { get; set; } = new();
        public List<EpisodeDto> Episodes { get; set; } = new();
    }

    public class EpisodeDto
    {
        public long EpisodeId { get; set; }
        public long ContentId { get; set; }
        public int EpisodeNumber { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? VideoUrl { get; set; }
        public int? Duration { get; set; }
        public DateOnly? ReleaseDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    // --- New: Result-pattern Request / Response records ---

    public sealed record GetContentPagedListRequest
    {
        public int PageNo { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public int? ContentTypeId { get; set; }
    }

    public sealed record GetContentDetailRequest
    {
        public long ContentId { get; set; }
    }

    public sealed record CreateContentRequest
    {
        public int ContentTypeId { get; set; } = 1;
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? PosterUrl { get; set; }
        public string? BannerUrl { get; set; }
        public DateOnly? ReleaseDate { get; set; }
        public string? Country { get; set; }
        public string? Language { get; set; }
        public string Status { get; set; } = "Active";
    }

    public sealed record UpdateContentRequest
    {
        public long ContentId { get; set; }
        public int ContentTypeId { get; set; } = 1;
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? PosterUrl { get; set; }
        public string? BannerUrl { get; set; }
        public DateOnly? ReleaseDate { get; set; }
        public string? Country { get; set; }
        public string? Language { get; set; }
        public string Status { get; set; } = "Active";
    }

    public sealed record DeleteContentRequest
    {
        public long ContentId { get; set; }
    }

    // Response records

    public sealed record ContentListItemResponse(
        long ContentId,
        string Title,
        string? Description,
        string? PosterUrl,
        string? BannerUrl,
        DateOnly? ReleaseDate,
        string? Country,
        string? Language,
        string ContentType,
        string Status);

    public sealed record ContentDetailResponse(
        long ContentId,
        int ContentTypeId,
        string Title,
        string? Description,
        string? PosterUrl,
        string? BannerUrl,
        DateOnly? ReleaseDate,
        string? Country,
        string? Language,
        string ContentType,
        string Status,
        DateTime CreatedAt,
        DateTime? UpdatedAt);

    public sealed record GetContentPagedListResponse(
        IReadOnlyList<ContentListItemResponse> Items,
        int TotalCount,
        int PageNo,
        int PageSize);

    public sealed record GetContentDetailResponse(ContentDetailResponse Content);
    public sealed record CreateContentResponse(ContentDetailResponse Content);
    public sealed record UpdateContentResponse(ContentDetailResponse Content);
    public sealed record DeleteContentResponse(long ContentId, bool IsDeleted);
}
