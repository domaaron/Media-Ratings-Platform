using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media_Ratings_Platform.models
{
    public class Movie : MediaEntry
    {
        public override MediaType MediaType => MediaType.Movie;
        public Movie(string title, string description, int releaseYear, List<Genres> genres, int ageRestriction)
            : base(Guid.NewGuid(), title, description, releaseYear, genres, ageRestriction) { }
    }
}
