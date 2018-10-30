using AlbumViewerBusiness;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;
using System.IO;
using AlbumViewerAspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AlbumViewerNetCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArtistController : ControllerBase
    {
        AlbumViewerContext context;

        IArtistRepository ArtistRepo;
        IConfiguration Configuration;

        private IHostingEnvironment HostingEnv;

        public ArtistController(
            AlbumViewerContext ctx,
            IServiceProvider svcProvider,
            IArtistRepository artistRepo,
            IAlbumRepository albumRepo,
            IConfiguration config,
            ILogger<ArtistController> logger,
            IHostingEnvironment env)
        {
            context = ctx;
            Configuration = config;

            ArtistRepo = artistRepo;

            HostingEnv = env;
        }

        [HttpGet]
        [Route("api/artists")]
        public async Task<IEnumerable> GetArtists()
        {
            return await ArtistRepo.GetAllArtists();
        }

        [HttpGet("api/artist/{id:int}")]
        public async Task<object> Artist(int id)
        {
            var artist = await ArtistRepo.Load(id);

            if (artist == null)
                throw new ApiException("Invalid artist id.", 404);

            var albums = await ArtistRepo.GetAlbumsForArtist(id);

            return new ArtistResponse()
            {
                Artist = artist,
                Albums = albums
            };
        }

        [HttpPost("api/artist")]
        public async Task<ArtistResponse> SaveArtist([FromBody] Artist artist)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
                throw new ApiException("You have to be logged in to modify data", 401);

            if (!ArtistRepo.Validate(artist))
                throw new ApiException(ArtistRepo.ValidationErrors.ToString(), 500, ArtistRepo.ValidationErrors);

            if (!await ArtistRepo.SaveAsync(artist))
                throw new ApiException("Unable to save artist.");

            return new ArtistResponse()
            {
                Artist = artist,
                Albums = await ArtistRepo.GetAlbumsForArtist(artist.Id)
            };
        }

        [HttpGet("api/artistlookup")]
        public async Task<IEnumerable<object>> ArtistLookup(string search = null)
        {
            if (string.IsNullOrEmpty(search))
                return new List<object>();

            var repo = new ArtistRepository(context);
            var term = search.ToLower();
            return await repo.ArtistLookup(term);
        }


        [HttpDelete("api/artist/{id:int}")]
        public async Task<bool> DeleteArtist(int id)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
                throw new ApiException("You have to be logged in to modify data", 401);

            return await ArtistRepo.DeleteArtist(id);
        }


        #region admin
        [HttpGet]
        [Route("api/reloaddata")]
        public bool ReloadData()
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
                throw new ApiException("You have to be logged in to modify data", 401);

            string isSqLite = Configuration["data:useSqLite"];
            try
            {
                if (isSqLite != "true")
                {
                    context.Database.ExecuteSqlCommand(@"
drop table Tracks;
drop table Albums;
drop table Artists;
drop table Users;
");
                }
                else
                {
                    // this is not reliable for mutliple connections
                    context.Database.CloseConnection();

                    try
                    {
                        System.IO.File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "AlbumViewerData.sqlite"));
                    }
                    catch
                    {
                        throw new ApiException("Can't reset data. Existing database is busy.");
                    }
                }

            }
            catch { }


            AlbumViewerDataImporter.EnsureAlbumData(context,
                Path.Combine(HostingEnv.ContentRootPath,
                "albums.js"));

            return true;
        }


        #endregion
    }

    #region Custom Responses

    public class ArtistResponse
    {
        public Artist Artist { get; set; }

        public List<Album> Albums { get; set; }
    }

    #endregion
}

