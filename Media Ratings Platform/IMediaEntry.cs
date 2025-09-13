using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media_Ratings_Platform
{
    /*
     A media entry:
        - represents either a movie, series, or game
        - consists of title, description, media type, release year, genre(s), and age restriction
        - is created by a user and can only be edited or deleted by its creator
        - includes a list of ratings and a calculated average score
        - can be marked as favorite by other users
    */

    public enum Genres
    {
        Action,
        Adventure,
        Comedy = 2,
        Drama,
        Horror,
        SciFi,
        Fantasy,
        Thriller,
        Documentary,
        Romance
    }
       
    public enum MediaType
    {
        Movie,
        Series,
        Game
    }

    public interface IMediaEntry
    {
        string Title { get; set; }
        string Description { get; set; }
        MediaType MediaType { get; }
        int ReleaseYear { get; set; }
        List<Genres> Genres { get; set; }
        int AgeRestriction { get; set; }
    }
}
