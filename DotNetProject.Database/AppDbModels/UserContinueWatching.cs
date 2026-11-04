using System;

namespace DotNetProject.Database.AppDbModels;

public partial class UserContinueWatching
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public long ContentId { get; set; }

    public bool IsContent { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? PosterUrl { get; set; }

    public string? Category { get; set; }

    public int EpisodeNumber { get; set; } = 1;

    public int TotalEpisodes { get; set; } = 1;

    public int ProgressSeconds { get; set; }

    public int DurationSeconds { get; set; }

    public DateTime LastWatchedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; }
}
