using System;
using System.Collections.Generic;

namespace DotNetProject.Database.AppDbModels;

public partial class ContentGenre
{
    public long ContentId { get; set; }

    public int GenreId { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Content Content { get; set; } = null!;

    public virtual Genre Genre { get; set; } = null!;
}
