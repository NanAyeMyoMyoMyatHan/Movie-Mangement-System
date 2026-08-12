using System;
using System.Collections.Generic;

namespace DotNetProject.Database.AppDbModels;

public partial class WatchlistItem
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int MovieId { get; set; }

    public int Status { get; set; }

    public int CurrentEpisode { get; set; }

    public int TotalEpisodes { get; set; }

    public DateTime AddedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Movie Movie { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
