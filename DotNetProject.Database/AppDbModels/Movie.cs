using System;
using System.Collections.Generic;

namespace DotNetProject.Database.AppDbModels;

public partial class Movie
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Genre { get; set; }

    public int? Year { get; set; }

    public string? Description { get; set; }

    public string? PosterUrl { get; set; }

    public int? TmdbId { get; set; }

    public bool IsManual { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual ICollection<WatchlistItem> WatchlistItems { get; set; } = new List<WatchlistItem>();
}
