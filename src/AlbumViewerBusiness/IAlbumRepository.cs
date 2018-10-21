using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Westwind.BusinessObjects;

namespace AlbumViewerBusiness
{
    public interface IAlbumRepository : IEntityFrameworkRepository<AlbumViewerContext, Album>
    {
        Task<bool> DeleteAlbum(int id, IDbContextTransaction tx = null);
        Task<List<Album>> GetAllAlbums(int page = 0, int pageSize = 15);
        Task<Album> SaveAlbum(Album postedAlbum);
    }
}