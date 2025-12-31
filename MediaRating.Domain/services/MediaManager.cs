using Media_Ratings_Platform.DTOs;
using MediaRatings.Domain.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatings.Domain.services
{
    public class MediaManager : IMediaManager
    {
        private readonly IMediaRepository _repository;

        public MediaManager(IMediaRepository repository)
        {
            _repository = repository;
        }

        public IMediaEntry? GetMediaById(int mediaEntryId)
        {
            return _repository.GetMediaById(mediaEntryId);
        }

        public IReadOnlyCollection<IMediaEntry> GetAllMediaEntries()
        {
            return _repository.GetAllMedia();
        }

        public void AddMediaEntry(IMediaEntry mediaEntry)
        {
            _repository.CreateMediaEntry(mediaEntry);
        }

        public bool RemoveMediaEntry(int mediaEntryId)
        {
            return _repository.DeleteMediaEntry(mediaEntryId);
        }

        public bool UpdateMediaEntry(int mediaEntryId, UpdateMediaDto mediaDto)
        {
            var existing = _repository.GetMediaById(mediaEntryId) as MediaEntry;
            if (existing == null)
                return false;

            var updatedGenres = mediaDto.Genres.Select(g => g.ToLower() switch
            {
                "action" => Genres.Action,
                "thriller" => Genres.Thriller,
                "sci-fi" or "scifi" => Genres.SciFi,
                "animation" => Genres.Animation,
                "comedy" => Genres.Comedy,
                "drama" => Genres.Drama,
                "fantasy" => Genres.Fantasy,
                "adventure" => Genres.Adventure,
                _ => Genres.Unknown
            }).ToList();

            MediaEntry updated = mediaDto.MediaType?.ToLower() switch
            {
                "movie" => new Movie(existing.CreatedBy, mediaDto.Title, mediaDto.Description, mediaDto.ReleaseYear, updatedGenres, mediaDto.AgeRestriction, existing.MediaId),
                "series" => new Series(existing.CreatedBy, mediaDto.Title, mediaDto.Description, mediaDto.ReleaseYear, updatedGenres, mediaDto.AgeRestriction, existing.MediaId),
                "game" => new Game(existing.CreatedBy, mediaDto.Title, mediaDto.Description, mediaDto.ReleaseYear, updatedGenres, mediaDto.AgeRestriction, existing.MediaId),
                _ => existing
            };

            // transfer old ratings and favorites
            foreach (var rating in existing.Ratings)
                updated.AddRating(rating);

            foreach (var userId in existing.FavoritedBy)
                updated.AddFavorite(userId);

            return _repository.UpdateMediaEntry(updated);
        }


        public int CountMediaEntries()
        {
            return _repository.GetAllMedia().Count;
        }
    }
}
