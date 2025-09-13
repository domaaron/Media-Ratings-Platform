using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media_Ratings_Platform.models
{
    public class Movie : IMediaEntry
    {
        private string _title;
        private string _description;
        private string _mediaType;
        private DateTime _releaseYear;
        private Genres _genres;
        private int _ageRestriction;

        public string Title
        {
            get => _title;
            set => _title = value;
        }

        public string Description
        {
            get => _description; 
            set => _description = value;
        }

        public string MediaType
        {
            get => _mediaType;
            set => _mediaType = value;
        }

        public DateTime ReleaseYear
        {
            get => _releaseYear;
            set => _releaseYear = value;
        }

        public Genres Genres
        {
            get => _genres;
            set => _genres = value;
        }

        public int AgeRestriction
        {
            get => _ageRestriction;
            set => _ageRestriction = value;
        }
    }
}
