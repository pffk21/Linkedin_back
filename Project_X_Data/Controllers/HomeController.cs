

using MailKit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Project_X_Data.Data;
using Project_X_Data.Data.Entities;
using Project_X_Data.Models;
using Project_X_Data.Models.LogInOut;
using Project_X_Data.Services.Kdf;
using Project_X_Data.Services.MailKit;
using Project_X_Data.Services.Storage;
using System.Diagnostics;
using System.Security.Claims;

namespace Project_X_Data.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IKdfService _kdfService;
        private readonly DataContext _dataContext;
        private readonly DataAccessor _dataAccessor;
        private readonly IEmailSender _emailSender;
        public readonly IMemoryCache _memoryCache;
        public readonly IStorageService _storageService;



        public HomeController(ILogger<HomeController> logger, IKdfService kdfService, IStorageService storageService, DataContext dataContext, DataAccessor dataAccessor, IMemoryCache memoryCache, IEmailSender emailSender)
        {
            _logger = logger;
            _kdfService = kdfService;
            _storageService = storageService;
            _dataContext = dataContext;
            _dataAccessor = dataAccessor;
            _memoryCache = memoryCache;
            _emailSender = emailSender;
        }


        public IActionResult Index() { 
            return View();
        }

        //[HttpGet]
        //public IActionResult Gmail()
        //{
        //    return View();
        //}

        //[HttpPost]
        //public IActionResult Gmail(GmailFormModel model)
        //{
        //    _emailSender.SendEmail("gerpaul588@gmail.com", model.Subject, model.Message);
        //    ViewBag.Message = "Email sent successfully!";
        //    return View();
        //}

        [HttpGet]
        public IActionResult Verification()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Verification(VerificationModel model)
        {
            if (!_memoryCache.TryGetValue<RegistrationSave>(model.Email, out var registrationData))
                return BadRequest(new { message = "Verification failed" });

            if (registrationData.Code != model.Code)
                return BadRequest(new { message = "Invalid code" });

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
            return Ok(new { Status = "Registration successful" });
        }

        [HttpGet]
        public IActionResult Registration()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Registration(RegistrationViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string avatarFileName = null;
            if (model.ProfileImageUrl != null)
            {
                avatarFileName = _storageService.Save(model.ProfileImageUrl);
            }
            if (_dataAccessor.GetUserByEmail(model.Email) != null)
            {
                ModelState.AddModelError("Email", "Email already exists");
                return View(model);
            }

            var code = new Random().Next(10000, 99999).ToString();

            var registrationData = new RegistrationSave
            {
                FullName = model.FirstName + " " + model.LastName,
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
            ViewBag.Message = "Verification code sent!";
            return RedirectToAction("Verification", "Home");
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!_dataAccessor.LoginCheck(model.Email, model.Password)) 
            { 
                return BadRequest(new { message = "Invalid email or password" });
            }
            

            return Ok(new { Status = "Login status successful" });
        }


        //2744FC45FF2F7CACD2EB
        //public IActionResult Index()
        //{
        //    // 2744FC45FF2F7CACD2EB
        //    // ViewData["dk"] = _kdfService.Dk("Admin", "4506C7");

        //    HomeIndexViewModel model = new()
        //    {
        //        //ProductGroups = _dataContext
        //        //    .ProductGroups
        //        //    .Where(g => g.DeletedAt == null)
        //        //    .AsEnumerable()
        //        ProductGroups = _dataAccessor.GetProductGroups()
        //    };

        //    return View(model);
        //}


        //public IActionResult Category(String id)
        //{
        //    HomeCategoryViewModel model = new()
        //    {
        //        ProductGroup = _dataContext
        //            .ProductGroups
        //            .Include(g => g.Products)
        //            .AsNoTracking()
        //            .FirstOrDefault(g => g.Slug == id && g.DeletedAt == null)
        //    };
        //    return View(model);
        //}

        //public IActionResult Admin()
        //{
        //    bool isAdmin = HttpContext.User.Claims
        //                .FirstOrDefault(c => c.Type == ClaimTypes.Role)
        //                ?.Value == "Admin";
        //    if (isAdmin)
        //    {
        //        HomeAdminViewModel model = new()
        //        {
        //            ProductGroups = _dataContext
        //                .ProductGroups
        //                .Where(g => g.DeletedAt == null)
        //                .AsEnumerable()
        //        };
        //        return View(model);
        //    }
        //    else
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //}

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
