using System;

namespace DotNetProject.Database.AppDbModels;

public partial class UserWatchLater
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public long ContentId { get; set; }
    public bool IsContent { get; set; } = true;
    public string Title { get; set; } = null!;
    public string? PosterUrl { get; set; }
    public string? Category { get; set; }
    public string? Description { get; set; }
    public int TotalEpisodes { get; set; } = 1;
    public int CurrentEpisode { get; set; } = 1;
    public string Status { get; set; } = "Want to Watch";
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
}
