using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media_Ratings_Platform.DTOs
{
    public class CreateMediaDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ReleaseYear { get; set; }
        public List<string> Genres { get; set; } = new();
        public string MediaType { get; set; } = string.Empty;
        public int AgeRestriction { get; set; }
    }
}
