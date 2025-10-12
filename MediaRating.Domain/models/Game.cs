using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatings.Domain
{
    public class Game : MediaEntry
    {
        public Game(int createdBy, string title, string description, int releaseYear, List<Genres> genres, int ageRestriction)
        : base(createdBy, title, description, releaseYear, genres, ageRestriction, MediaType.Game) { }
    }
}
