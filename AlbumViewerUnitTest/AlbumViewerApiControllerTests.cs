using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AlbumViewerAspNetCore;
using AlbumViewerBusiness;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;

namespace AlbumViewerUnitTest
{
    [TestFixture]
    public class AlbumViewerApiControllerTests
    {
        private SqliteConnection _connection;
        private AlbumViewerContext _albumViewerContext;
        private IServiceProvider _serviceProvider;
        private IArtistRepository _artistRepository;
        private IAlbumRepository _albumRepository;
        private IConfiguration _configuration;
        private ILogger<AlbumViewerApiController> _logger;
        private IHostingEnvironment _hostingEnvironment;
        private AlbumViewerApiController _sut;

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

            _serviceProvider = Substitute.For<IServiceProvider>();
            _artistRepository = Substitute.For<IArtistRepository>();
            _albumRepository = Substitute.For<IAlbumRepository>();
            _configuration = Substitute.For<IConfiguration>();
            _logger = Substitute.For<ILogger<AlbumViewerApiController>>();
            _hostingEnvironment = Substitute.For<IHostingEnvironment>();

            _sut = new AlbumViewerApiController(_albumViewerContext, _serviceProvider, _artistRepository,
                _albumRepository, _configuration, _logger, _hostingEnvironment);
        }

        [TearDown]
        public void TearDown()
        {
            _connection.Close();
        }

        [Test]
        public void SaveAlbumShouldThrowExceptionForInvalidUser()
        {
            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var exception = Assert.ThrowsAsync<ApiException>(async () => await _sut.SaveAlbum(new Album()));

            Assert.AreEqual(exception.Message, "You have to be logged in to modify data");
            Assert.AreEqual(exception.StatusCode, 401);
        }



        [Test]
        public void SaveAlbumShouldThrowExceptionForInvalidModel()
        {
            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, "User")
                    }, "AuthenticationType"))
                }
            };

            var exception = Assert.ThrowsAsync<ApiException>(async () => await _sut.SaveAlbum(new Album()));

            Assert.AreEqual(exception.StatusCode, 500);
        }

        [Test]
        public async Task SaveAlbumShouldSaveValidAlbum()
        {
            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, "User")
                    }, "AuthenticationType"))
                }
            };

            var validAlbum = new Album
            {
                AmazonUrl = "AmazonUrl",
                Artist = new Artist { AmazonUrl = "AmazonUrl", ArtistName = "ArtistName", Description = "ArtistDescription", Id = 1, ImageUrl = "ImageUrl" },
                ArtistId = 1,
                Description = "AlbumDescription",
                Id = 1,
                ImageUrl = "ImageUrl",
                SpotifyUrl = "SpotifyUrl",
                Title = "Title",
                Tracks = new List<Track> { new Track { AlbumId = 1, Bytes = 1, Id = 1, Length = "5:11", SongName = "SongName", UnitPrice = 0 } }

            };

            _albumRepository.SaveAlbum(validAlbum).Returns(validAlbum);
            _albumRepository.Validate(validAlbum).Returns(true);

            var createdAlbum = await _sut.SaveAlbum(validAlbum);

            Assert.AreEqual(createdAlbum.Id, validAlbum.Id);
            Assert.AreEqual(createdAlbum.Title, validAlbum.Title);
        }

        [Test]
        public void SaveAlbumShouldNotSaveAlbum()
        {
            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, "User")
                    }, "AuthenticationType"))
                }
            };

            var validAlbum = new Album
            {
                AmazonUrl = "AmazonUrl",
                Artist = new Artist { AmazonUrl = "AmazonUrl", ArtistName = "ArtistName", Description = "ArtistDescription", Id = 1, ImageUrl = "ImageUrl" },
                ArtistId = 1,
                Description = "AlbumDescription",
                Id = 1,
                ImageUrl = "ImageUrl",
                SpotifyUrl = "SpotifyUrl",
                Title = "Title",
                Tracks = new List<Track> { new Track { AlbumId = 1, Bytes = 1, Id = 1, Length = "5:11", SongName = "SongName", UnitPrice = 0 } }

            };

            _albumRepository.SaveAlbum(validAlbum).ReturnsNull();
            _albumRepository.Validate(validAlbum).Returns(true);
            _albumRepository.ErrorMessage = "Error";
            

            var exception = Assert.ThrowsAsync<ApiException>(async () => await _sut.SaveAlbum(validAlbum));
            Assert.AreEqual(exception.Message, "Error");
        }

        [Test]
        public void DeleteAlbumShouldNotAuthorizeAlbumDeletion()
        {
            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var exception = Assert.ThrowsAsync<ApiException>(async () => await _sut.DeleteAlbum(1));

            Assert.AreEqual(exception.Message, "You have to be logged in to modify data");
            Assert.AreEqual(exception.StatusCode, 401);
        }

        [Test]
        public async Task DeleteAlbumShouldAuthorizeAlbumDeletion()
        {
            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, "User")
                    }, "AuthenticationType"))
                }
            };

            _albumRepository.DeleteAlbum(1).Returns(true);
            var didDelete = await _sut.DeleteAlbum(1);

            Assert.IsTrue(didDelete);
        }


        [Test]
        public async Task DeleteAlbumByNameShouldAuthorizeAlbumDeletion()
        {
            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, "User")
                    }, "AuthenticationType"))
                }
            };

            _albumViewerContext.Albums.Add(new Album { Id = 1, Title ="Delete"});
            _albumViewerContext.Albums.Add(new Album { Id = 2, Title = "Keep"});
            _albumViewerContext.Albums.Add(new Album { Id = 3, Title = "Delete" });
            _albumViewerContext.SaveChanges();

            await _sut.DeleteAlbumByName("Delete");

            await _albumRepository.Received(1).DeleteAlbum(1);
            await _albumRepository.DidNotReceive().DeleteAlbum(2);
            await _albumRepository.Received(1).DeleteAlbum(3);
        }

        [Test]
        public void DeleteAlbumShouldThrowForInvalidUser()
        {
            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var exception = Assert.ThrowsAsync<ApiException>(async () => await _sut.DeleteAlbumByName("Name"));

            Assert.AreEqual(exception.StatusCode, 401);
        }

        [Test]
        public async Task GetAlbumsShouldCallAlbumRepositoryForAllAlbums()
        {
            await _sut.GetAlbums(1);

            await _albumRepository.Received().GetAllAlbums(1);
        }

        [Test]
        public async Task GetAlbumShouldLoadAlbumFromAlbumRepository()
        {
            await _sut.GetAlbum(1);

            await _albumRepository.Received().Load(1);
        }
    }
}
