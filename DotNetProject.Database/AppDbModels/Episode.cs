using System;
using System.Collections.Generic;

namespace DotNetProject.Database.AppDbModels;

public partial class Episode
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

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Content Content { get; set; } = null!;
}
