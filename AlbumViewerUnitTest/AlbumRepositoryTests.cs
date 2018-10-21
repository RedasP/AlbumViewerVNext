using System.Threading.Tasks;
using AlbumViewerBusiness;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace AlbumViewerUnitTest
{
    [TestFixture]
    public class AlbumRepositoryTests
    {
        private SqliteConnection _connection;
        private AlbumViewerContext _albumViewerContext;

        [SetUp]
        public void SetUp()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<AlbumViewerContext>()
                .UseSqlite(_connection)
                .Options;

            _albumViewerContext = new AlbumViewerContext(options);

            _albumViewerContext.Database.EnsureCreated();
        }

        [TearDown]
        public void TearDown()
        {
            _connection.Close();
        }

        [TestCase(16, 2)]
        [TestCase(31, 3)]
        [TestCase(46, 4)]
        public async Task GetAllAlbumsShouldReturnOneAlbumWhen15AlbumsPerPage(int generatedAlbums, int page)
        {
            for (var i = 0; i < generatedAlbums; i++)
            {
                _albumViewerContext.Albums.Add(new Album { Title = $"Album{i}" });
            }
            _albumViewerContext.SaveChanges();

            var sut = new AlbumRepository(_albumViewerContext);
            var albums = await sut.GetAllAlbums(page);

            Assert.AreEqual(albums.Count, 1);
        }
    }
}
