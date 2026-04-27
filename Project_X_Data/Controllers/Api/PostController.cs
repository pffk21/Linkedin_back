using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Project_X_Data.Data;
using Project_X_Data.Data;
using Project_X_Data.Data.Entities;
using Project_X_Data.Models.LogInOut;
using Project_X_Data.Models.Post;
using Project_X_Data.Models.Rest;
using Project_X_Data.Services.Auth;
using Project_X_Data.Services.Kdf;
using Project_X_Data.Services.MailKit;
using Project_X_Data.Services.Storage;
using System.Security.Claims;
using System.Text.Json;

namespace Project_X_Data.Controllers.Api
{
    [Route("api/post")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly DataAccessor _dataAccessor;
        private readonly IKdfService _kdfService;
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;
        private readonly IStorageService _storageService;
        private readonly DataContext _dataContext;


        public PostController(
            DataAccessor dataAccessor,
            DataContext dataContext,
            IConfiguration configuration,
            IKdfService kdfService,
            IAuthService authService,
            IStorageService storageService)
        {
            _dataAccessor = dataAccessor;
            _kdfService = kdfService;
            _authService = authService;
            _configuration = configuration;
            _storageService = storageService;
            _dataContext = dataContext;
        }

        [HttpPost]
        public IActionResult Post([FromForm] PostViewModel model)
        {
            RestResponse response = new();

            try
            {
                if (string.IsNullOrEmpty(model.Description) && model.ImageFile == null)
                {
                    response.Status = RestStatus.Status400;
                    response.Data = "Post cannot be empty";
                    return BadRequest(response);
                }

                string? imageUrl = null;
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    imageUrl = _storageService.Save(model.ImageFile);
                }

                Post post = new Post
                {
                    Id = Guid.NewGuid(),
                    UserId = model.UserId,
                    Description = model.Description,
                    ImageUrl = imageUrl,
                    CreatedAt = DateTime.UtcNow
                };

                _dataContext.Posts.Add(post);
                _dataContext.SaveChanges();

                response.Status = RestStatus.Status200;
                response.Data = post;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Status = RestStatus.Status500;
                response.Data = $"Internal error: {ex.Message}";
                return StatusCode(500, response);
            }
        }

        [HttpGet]
        public IActionResult GetPosts()
        {
            RestResponse response = new();
            try
            {
                var posts = _dataContext.Posts
                    .Include(p => p.User)
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => new {
                        p.Id,
                        p.Description,
                        p.ImageUrl,
                        p.CreatedAt,
                        AuthorName = p.User != null ? p.User.Name : "Deleted User",
                        AuthorAvatar = p.User != null ? p.User.AvatarPhoto : null,
                        AuthorRole = p.User != null ? p.User.AboitSection : "Member",
                        LikesCount = p.PostReactions != null ? p.PostReactions.Count : 0
                    })
                    .ToList();

                response.Status = RestStatus.Status200;
                response.Data = posts;

                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine("---------------- ERROR START ----------------");
                Console.WriteLine($"Message: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                Console.WriteLine("---------------- ERROR END ------------------");

                response.Status = RestStatus.Status500;
                response.Data = "Error loading posts: " + ex.Message;
                return StatusCode(500, response);
            }
        }
    }
}
