using System;
using System.Collections.Generic;

namespace DotNetProject.Database.AppDbModels;

public partial class Genre
{
    public int GenreId { get; set; }

    public string Name { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<ContentGenre> ContentGenres { get; set; } = new List<ContentGenre>();
}
