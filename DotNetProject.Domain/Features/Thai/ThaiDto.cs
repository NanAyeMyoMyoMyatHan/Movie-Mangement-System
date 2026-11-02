using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetProject.Domain.Features.Thai
{
    public sealed record GetThaiDetailRequest
    {
        public int Id { get; set; }
    }
    public sealed record GetThaiPagedListRequest
    {
        public int PageNo { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
    }
    public sealed record CreateThaiReques
    {
        public string? Title { get; set; }
        public string? Genre { get; set; }
        public int? ReleaseYear { get; set; }
        public string? Description { get; set; }
        public string? PosterUrl { get; set; }
        public int? TmdbId { get; set; }
        public bool IsManual { get; set; }
    }
    public sealed record UpdateThaiRequest
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
    public sealed record DeleteThaiRequest
    {
        public long Id { get; set; }
    }
    public sealed record ThaiListItemResponse(
        long Id,
        string Title,
        int? ReleaseYear,
        string? PosterUrl);
    public sealed record ThaiDetailResponse(
        long Id,
        string Title,
        int? ReleaseYear,
        string? Description,
        string? PosterUrl,
        DateTime CreatedAt);
    public sealed record GetThaiPagedListResponse(
        List<ThaiListItemResponse> Items,
        int TotalCount,
        int PageNo,
        int PageSize);
    public sealed record GetThaiDetailResponse(ThaiDetailResponse Thai);
    public sealed record CreateThaiResponse(ThaiDetailResponse Thai);
    public sealed record UpdateThaiResponse(ThaiDetailResponse Thai);
    public sealed record DeleteThaiResponse(long Id, bool IsDeleted);
}
