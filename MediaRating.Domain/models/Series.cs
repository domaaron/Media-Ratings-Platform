using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatings.Domain
{
    public class Series : MediaEntry
    {
        public override MediaType MediaType => MediaType.Series;
        public Series(string title, string description, int releaseYear, List<Genres> genres, int ageRestriction)
            : base(Guid.NewGuid(), title, description, releaseYear, genres, ageRestriction) { }
    }
}
