using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using DotNetProject.Database.AppDbModels;

namespace DotNetProject.Domain.Features.Contents
{
    public class ContentGenre
    {
        public long ContentId { get; set; }

        public int GenreId { get; set; }

        public Content Content { get; set; } = null!;

        public Genre Genre { get; set; } = null!;
    }
}
