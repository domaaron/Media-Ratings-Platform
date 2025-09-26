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
        void RemoveMediaEntry(IMediaEntry mediaEntry);
        void UpdateMediaEntry(IMediaEntry oldEntry, IMediaEntry newEntry);
        int CountMediaEntries();
        IReadOnlyCollection<IMediaEntry> GetAllMediaEntries();
    }
}
