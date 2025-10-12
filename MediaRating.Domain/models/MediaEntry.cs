

namespace MediaRatings.Domain
{
    public abstract class MediaEntry : IMediaEntry
    {
        public int MediaId { get; private set; }
        public int CreatedBy { get; private set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public MediaType MediaType { get; protected set; }
        public int ReleaseYear { get; set; }
        public List<Genres> Genres { get; set; } = new List<Genres>();
        public int AgeRestriction { get; set; }
        public List<UserRating> Ratings { get; private set; } = new List<UserRating>();
        public HashSet<int> FavoritedBy { get; private set; } = new HashSet<int>();

        protected MediaEntry(int createdBy, string title, string description, int releaseYear, List<Genres> genres, int ageRestriction, MediaType mediaType)
        {
            CreatedBy = createdBy;
            Title = title;
            Description = description;
            ReleaseYear = releaseYear;
            Genres = genres;
            AgeRestriction = ageRestriction;
            MediaType = mediaType;
        }

        public void AddRating(UserRating rating)
        {
            Ratings.Add(rating);
        }

        public void AddFavorite(int userId)
        {
            FavoritedBy.Add(userId);
        }

        public void RemoveFavorite(int userId)
        {
            FavoritedBy.Remove(userId);
        }

        public void RemoveRating(UserRating rating)
        {
            Ratings.Remove(rating);
        }

        public double AverageRating()
        {
            if (Ratings.Count == 0)
            {
                return 0.0;
            }
            
            return Ratings.Average(r => r.StarValue);
        }
    }
}
