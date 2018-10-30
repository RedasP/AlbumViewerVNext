using AlbumViewerBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;


// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace AlbumViewerAspNetCore
{
    [ServiceFilter(typeof(ApiExceptionFilter))]
    [EnableCors("CorsPolicy")]
    public class AlbumViewerApiController : Controller
    {
        AlbumViewerContext context;

        IAlbumRepository AlbumRepo;

        public AlbumViewerApiController(
            AlbumViewerContext ctx,
            IServiceProvider svcProvider,
            IArtistRepository artistRepo,
            IAlbumRepository albumRepo,
            IConfiguration config,
            ILogger<AlbumViewerApiController> logger,
            IHostingEnvironment env)
        {
            context = ctx;

            AlbumRepo = albumRepo;
        }

        [HttpGet]
        [Route("api/albums")]
        public async Task<IEnumerable<Album>> GetAlbums(int page = -1, int pageSize = 15)
        {
            //var repo = new AlbumRepository(context);
            return await AlbumRepo.GetAllAlbums(page, pageSize);
        }

        [HttpGet("api/album/{id:int}")]
        public async Task<Album> GetAlbum(int id)
        {
            return await AlbumRepo.Load(id);
        }

        [HttpPost("api/album")]
        public async Task<Album> SaveAlbum([FromBody] Album postedAlbum)
        {
            //throw new ApiException("Lemmy says: NO!");

            if (!HttpContext.User.Identity.IsAuthenticated)
                throw new ApiException("You have to be logged in to modify data", 401);

            if (!AlbumRepo.Validate(postedAlbum))
                throw new ApiException(AlbumRepo.ErrorMessage, 500, AlbumRepo.ValidationErrors);

            // this doesn't work for updating the child entities properly
            //if(!await AlbumRepo.SaveAsync(postedAlbum))
            //    throw new ApiException(AlbumRepo.ErrorMessage, 500);

            var album = await AlbumRepo.SaveAlbum(postedAlbum);
            if (album == null)
                throw new ApiException(AlbumRepo.ErrorMessage, 500);

            return album;
        }

        [HttpDelete("api/album/{id:int}")]
        public async Task<bool> DeleteAlbum(int id)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
                throw new ApiException("You have to be logged in to modify data", 401);

            return await AlbumRepo.DeleteAlbum(id);
        }


        [HttpGet]
        public async Task<string> DeleteAlbumByName(string name)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
                throw new ApiException("You have to be logged in to modify data", 401);

            var pks =
                await context.Albums.Where(alb => alb.Title == name).Select(alb => alb.Id).ToAsyncEnumerable().ToList();

            StringBuilder sb = new StringBuilder();
            foreach (int pk in pks)
            {
                bool result = await AlbumRepo.DeleteAlbum(pk);
                if (!result)
                    sb.AppendLine(AlbumRepo.ErrorMessage);
            }

            return sb.ToString();
        }

    }
}

