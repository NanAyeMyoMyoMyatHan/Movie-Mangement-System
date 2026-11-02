using System;
using System.Collections.Generic;

namespace DotNetProject.Database.AppDbModels;

public partial class Content
{
    public long ContentId { get; set; }

    public int ContentTypeId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? PosterUrl { get; set; }

    public string? BannerUrl { get; set; }

    public DateOnly? ReleaseDate { get; set; }

    public string? Country { get; set; }

    public string? Language { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<ContentGenre> ContentGenres { get; set; } = new List<ContentGenre>();

    public virtual ContentType ContentType { get; set; } = null!;

    public virtual ICollection<Episode> Episodes { get; set; } = new List<Episode>();
}
