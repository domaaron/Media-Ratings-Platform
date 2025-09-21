

namespace MediaRatings.Domain
{
    public abstract class MediaEntry : IMediaEntry
    {
        public Guid CreatedBy { get; private set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public abstract MediaType MediaType { get; }
        public int ReleaseYear { get; set; }
        public List<Genres> Genres { get; set; } = new List<Genres>();
        public int AgeRestriction { get; set; }
        public List<UserRating> Ratings { get; private set; } = new List<UserRating>();
        public HashSet<Guid> FavoritedBy { get; private set; } = new HashSet<Guid>();

        protected MediaEntry(Guid createdBy, string title, string description, int releaseYear, List<Genres> genres, int ageRestriction)
        {
            CreatedBy = createdBy;
            Title = title;
            Description = description;
            ReleaseYear = releaseYear;
            Genres = genres;
            AgeRestriction = ageRestriction;
        }

        public void AddRating(UserRating rating)
        {
            Ratings.Add(rating);
        }

        public void AddFavorite(Guid userId)
        {
            FavoritedBy.Add(userId);
        }

        public void RemoveFavorite(Guid userId)
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
