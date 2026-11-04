using System;

namespace DotNetProject.Database.AppDbModels;

public partial class UserFavorite
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public long ContentId { get; set; }
    public bool IsContent { get; set; } = true;
    public string Title { get; set; } = null!;
    public string? PosterUrl { get; set; }
    public string? Category { get; set; }
    public string? Description { get; set; }
    public string? Year { get; set; }
    public double Rating { get; set; } = 9.0;
    public int TotalEpisodes { get; set; } = 1;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
}
