using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatings.Domain
{
    public class Movie : MediaEntry
    {
        public Movie(int createdBy, string title, string description, int releaseYear, List<Genres> genres, int ageRestriction)
        : base(createdBy, title, description, releaseYear, genres, ageRestriction, MediaType.Movie) { }
    }
}
