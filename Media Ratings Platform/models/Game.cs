using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media_Ratings_Platform.models
{
    public class Game : MediaEntry
    {
        public override MediaType MediaType => MediaType.Game;
        public Game(string title, string description, int releaseYear, List<Genres> genres, int ageRestriction)
            : base(Guid.NewGuid(), title, description, releaseYear, genres, ageRestriction) { }
    }
}
