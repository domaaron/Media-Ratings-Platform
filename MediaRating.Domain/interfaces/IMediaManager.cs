using Media_Ratings_Platform.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatings.Domain.interfaces
{
    public interface IMediaManager
    {
        void AddMediaEntry(IMediaEntry mediaEntry);
        bool RemoveMediaEntry(int mediaEntryId);
        bool UpdateMediaEntry(int oldMediaEntryId, UpdateMediaDto mediaDto);
        int CountMediaEntries();
        IReadOnlyCollection<IMediaEntry> GetAllMediaEntries();
    }
}
