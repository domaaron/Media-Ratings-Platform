using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media_Ratings_Platform
{
    internal interface IMediaEntry
    {
        string Title { get; set; }
        string Description { get; set; }
        string MediaType { get; set; }
        DateTime ReleaseYear { get; set; }
        string Genres { get; set; }
        int AgeRestriction { get; set; }
    }
}
