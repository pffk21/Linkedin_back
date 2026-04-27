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
using Project_X_Data.Models.Rest;
using Project_X_Data.Services.Auth;
using Project_X_Data.Services.Kdf;
using Project_X_Data.Services.MailKit;
using Project_X_Data.Services.Storage;
using System.Security.Claims;
using System.Text.Json;

namespace Project_X_Data.Controllers.Api
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DataAccessor _dataAccessor;
        private readonly IKdfService _kdfService;
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;
        private readonly IStorageService _storageService;
        private readonly IMemoryCache _memoryCache;
        private readonly DataContext _dataContext;
        private readonly IEmailSender _emailSender;

        public UserController(
            DataAccessor dataAccessor,
            DataContext dataContext,
            IEmailSender emailSender,
            IConfiguration configuration,
            IKdfService kdfService,
            IAuthService authService,
            IStorageService storageService,
            IMemoryCache memoryCache)
        {
            _dataAccessor = dataAccessor;
            _kdfService = kdfService;
            _authService = authService;
            _configuration = configuration;
            _storageService = storageService;
            _memoryCache = memoryCache;
            _dataContext = dataContext;
            _emailSender = emailSender;
        }
        [HttpGet("jwt")]
        public IActionResult AuthenticateJwt()
        {
            try
            {
                var (login, password) = GetBasicCredentials();

                var userAccess = _dataAccessor.Authenticate(login, password)
                    ?? throw new Exception("Credentials rejected");

                var header = new { alg = "HS256", typ = "JWT" };
                var headerJson = JsonSerializer.Serialize(header);
                var header64 = Base64UrlTextEncoder.Encode(
                    System.Text.Encoding.UTF8.GetBytes(headerJson)
                );

                var payload = new
                {
                    iss = "Project_X",
                    sub = userAccess.UserId,
                    aud = userAccess.RoleId,
                    exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds(),
                    iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    name = userAccess.User.Name,
                    email = userAccess.User.Email,

                };
                var payloadJson = JsonSerializer.Serialize(payload);
                var payload64 = Base64UrlTextEncoder.Encode(
                    System.Text.Encoding.UTF8.GetBytes(payloadJson)
                );

                string? secret = _configuration["Jwt:Secret"];
                if (string.IsNullOrEmpty(secret))
                    throw new KeyNotFoundException("Missing Jwt.Secret in configuration");

                string tokenBody = $"{header64}.{payload64}";
                string signature = Base64UrlTextEncoder.Encode(
                    System.Security.Cryptography.HMACSHA256.HashData(
                        System.Text.Encoding.UTF8.GetBytes(secret),
                        System.Text.Encoding.UTF8.GetBytes(tokenBody)
                    )
                );

                string token = $"{tokenBody}.{signature}";

                return Ok(new
                {
                    token = token,
                    user = new
                    {
                        id = userAccess.UserId,
                        name = userAccess.User.Name,
                        email = userAccess.User.Email,
                        role = userAccess.RoleId
                    }
                });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
        }
        private (string, string) GetBasicCredentials()
        {
            string? header = HttpContext.Request.Headers.Authorization;
            if (header == null)
                throw new Exception("Authorization Header Required");

            const string prefix = "Basic ";
            if (!header.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                throw new Exception("Invalid Authorization Scheme");

            string credentials = header[prefix.Length..];
            string userPass;
            try
            {
                userPass = System.Text.Encoding.UTF8.GetString(
                    Convert.FromBase64String(credentials));
            }
            catch
            {
                throw new Exception("Invalid Base64 credentials");
            }

            string[] parts = userPass.Split(':', 2);
            if (parts.Length != 2)
                throw new Exception("Credentials must be in format login:password");

            return (parts[0], parts[1]);
        }

        [HttpPost("verification")]
        public IActionResult Verification(VerificationModel model)
        {
            RestResponse response = new();

            if (!_memoryCache.TryGetValue<RegistrationSave>(model.Email, out var registrationData))
            {
                response.Status = RestStatus.Status400;
                response.Data = "Verification failed";
                return BadRequest(response);
            }

            if (registrationData.Code != model.Code)
            {
                response.Status = RestStatus.Status400;
                response.Data = "Invalid code";
                return BadRequest(response);
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = registrationData.FullName,
                Email = registrationData.Email,
                AboitSection = registrationData.WorkingPlace,
                Location = registrationData.Location,
                AvatarPhoto = registrationData.ProfileImageUrl,
                CreatedAt = DateTime.Now,
                DateOfBirth = registrationData.BirthDate
            };

            _dataContext.Users.Add(user);
            _dataContext.SaveChanges();

            string salt = _kdfService.GenerateSalt();
            var access = new UserAccess
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Login = registrationData.Email,
                Salt = salt,
                Dk = _kdfService.Dk(registrationData.Password, salt),
                RoleId = "User"
            };

            _dataContext.UserAccesses.Add(access);
            _dataContext.SaveChanges();

            _memoryCache.Remove(model.Email);

            response.Status = RestStatus.Status200;
            response.Data = "Registration successful";
            return Ok(response);
        }
        [HttpPost("entrance")]
        public IActionResult Login(LoginViewModel model)
        {
            RestResponse response = new();

            if (!_dataAccessor.LoginCheck(model.Email, model.Password))
            {
                response.Status = RestStatus.Status401;
                response.Data = "Login failed";
                return Unauthorized(response);
            }

            response.Status = RestStatus.Status200;
            response.Data = "Login successful";
            return Ok(response);
        }

        [HttpPost("registration")]
        public IActionResult Registration(RegistrationViewModel model)
        {
            RestResponse response = new();

            if (!ModelState.IsValid)
            {
                response.Status = RestStatus.Status400;
                response.Data = "Invalid registration data";
                return BadRequest(response);
            }

            string? avatarFileName = null;
            if (model.ProfileImageUrl != null)
                avatarFileName = _storageService.Save(model.ProfileImageUrl);

            if (_dataAccessor.GetUserByEmail(model.Email) != null)
            {
                response.Status = RestStatus.Status400;
                response.Data = "Email already exists";
                return BadRequest(response);
            }

            var code = new Random().Next(10000, 99999).ToString();

            var registrationData = new RegistrationSave
            {
                FullName = $"{model.FirstName} {model.LastName}",
                Email = model.Email,
                WorkingPlace = model.WorkingPlace,
                Location = model.Location,
                BirthDate = model.BirthDate,
                ProfileImageUrl = avatarFileName,
                Password = model.Password,
                Code = code
            };

            _dataAccessor.SetRole();
            _memoryCache.Set(registrationData.Email, registrationData, TimeSpan.FromMinutes(3));

            _emailSender.SendEmail(
                model.Email,
                "Verification Code",
                $"Your verification code is: {code}"
            );

            response.Status = RestStatus.Status200;
            response.Data = "Check your email";
            return Ok(response);
        }

        [HttpGet("info")]
        public ActionResult<RestResponse> Information()
        {
            try
            {
                var emailClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(emailClaim))
                {

                    return Unauthorized();
                }


                var user = _dataAccessor.GetUserByEmail(emailClaim);
                if (user == null)
                {
                    return NotFound(new RestResponse
                    {
                        Status = RestStatus.Status401,
                        Data = "Пользователя нету с таким имейлом " + emailClaim
                    });

                }
                User user1 = new()
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    AboitSection = user.AboitSection,
                    Location = user.Location,
                    AvatarPhoto = $"/Storage/Item/{user.AvatarPhoto}",
                    CreatedAt = user.CreatedAt,
                    DateOfBirth = user.DateOfBirth
                };

                RestResponse response = new()
                {
                    Status = RestStatus.Status200,
                    Data = user1
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server Error: {ex}"); 

                return StatusCode(500, new RestResponse
                {
                    Status = RestStatus.Status404,
                    Data = "Внутренняя ошибка сервера. Проверьте лог сервера."
                });
            }
            //var user = _dataAccessor.GetUserByEmail(email);
            
        }

        [HttpPost("admin")]
        public IActionResult SignUpAdmin()
        {
            RestResponse response = new()
            {
                Status = RestStatus.Status200,
                Data = "SignUpAdmin Works"
            };
            return Ok(response);
        }
    }
}

/* Відмінності АРІ та MVC контролерів
 * MVC:
 *  адресація за назвою дії (Action) - різні дії -- різні адреси
 *  GET  /Home/Index     --> HomeController.Index()
 *  POST /Home/Index     --> HomeController.Index()
 *  GET  /Home/Privacy   --> HomeController.Privacy()
 *  повернення - IActionResult частіше за все View
 *  
 * API:
 *  адресація за анотацією [Route("api/user")], різниця
 *  у методах запиту
 *  GET  api/user  ---> [HttpGet] Authenticate()
 *  POST api/user  ---> [HttpPost] SignUp()
 *  PUT  api/user  ---> 
 *  
 *  C   POST
 *  R   GET
 *  U   PUT(replace) PATCH(partially update)
 *  D   DELETE
 */
/* Авторизація. Схеми.
 * 0) Кукі (Cookie) - заголовки НТТР-пакету, які зберігаються у клієнта
 *      та автоматично включаються ним до всіх наступних запитів до сервера
 *      "+" простота використання
 *      "-" автоматизовано тільки у браузерах, в інших програмах це справа
 *           програміста. 
 *      "-" відкритість, легкість перехоплення даних
 *      
 * 1) Сесії (серверні): базуються на Кукі, проте всі дані зберігаються
 *     на сервері, у куках передається тільки ідентифікатор сесії
 *     "+" покращена безпека
 *     "-" велике навантаження на сховище сервера
 *     
 * 2) Токени (клієнтські): клієнт зберігає токен, який лише перевіряється
 *     сервером.
 *     "+" відмова від кукі та сесій
 *     "-" більше навантаження на роботу сервера
 *  2а) Токени-ідентифікатори
 *  2б) Токени з даними (JWT)
 */