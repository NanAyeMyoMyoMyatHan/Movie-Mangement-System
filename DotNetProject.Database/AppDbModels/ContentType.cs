using System;
using System.Collections.Generic;

namespace DotNetProject.Database.AppDbModels;

public partial class ContentType
{
    public int ContentTypeId { get; set; }

    public string Name { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<Content> Contents { get; set; } = new List<Content>();
}
