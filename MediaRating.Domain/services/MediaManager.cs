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

            existing.Title = mediaDto.Title;
            existing.Description = mediaDto.Description;
            existing.ReleaseYear = mediaDto.ReleaseYear;
            existing.AgeRestriction = mediaDto.AgeRestriction;

            existing.Genres = mediaDto.Genres.Select(g => g.ToLower() switch
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

            return _repository.UpdateMediaEntry(existing);
        }

        public int CountMediaEntries()
        {
            return _repository.GetAllMedia().Count;
        }
    }
}
