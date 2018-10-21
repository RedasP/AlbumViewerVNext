using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Westwind.BusinessObjects;

namespace AlbumViewerBusiness
{
    public interface IArtistRepository : IEntityFrameworkRepository<AlbumViewerContext, Artist>
    {
        Task<List<ArtistLookupItem>> ArtistLookup(string search = null);
        Task<bool> DeleteArtist(int id);
        Task<List<Album>> GetAlbumsForArtist(int artistId);
        Task<List<ArtistWithAlbumCount>> GetAllArtists();
    }
}