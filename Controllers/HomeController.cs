using System.Diagnostics;
using GuideMe.Models;
using GuideMe.Security;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace GuideMe.Controllers
{
    public class HomeController : Controller
    {
        private readonly GuideMeContext _context;
        private readonly IDataProtector _protector;
        private readonly IWebHostEnvironment _env;

        public HomeController(GuideMeContext context, DataSecurity key, IDataProtectionProvider _provider, IWebHostEnvironment env)
        {

            _protector = _provider.CreateProtector(key.key);
            _context = context;
            _env = env;
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.Name)?.Value;

                if (int.TryParse(userIdClaim, out int userId))
                {
                    var user = _context.Users.Find(userId); 
                    ViewBag.LastLogin = user?.LastLogin; 
                }
            }
            return View();
        }

        [HttpGet]
        public IActionResult Register() 
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(UserEdit e) 
        {
            if (!ModelState.IsValid)
            {
                return View(e);
            }
            try
            {
                var user = _context.Users.Where(a=>a.Email ==  e.Email).FirstOrDefault();

                if (user == null)
                {
                    if (e.UserFile != null)
                    {
                        string fileName = "UserFile" + Guid.NewGuid() + Path.GetExtension(e.UserFile.FileName);
                        string filePath = Path.Combine(_env.WebRootPath, "UserFile", fileName);
                        using (FileStream stream = new FileStream(filePath, FileMode.Create))
                        {
                            e.UserFile.CopyTo(stream);
                        }
                        e.UserImage = fileName;
                    }

                    User u = new()
                    {
                        UserId = e.UserId,
                        Email = e.Email,
                        Password = _protector.Protect(e.Password),
                        Name = e.Name,
                        Role = e.Role,
                        LastLogin = e.LastLogin,
                        UserImage = e.UserImage
                    };

                    _context.Users.Add(u);
                    _context.SaveChanges();
                    return RedirectToAction("LogIn","Authentication");
                }
                else
                {
                    ModelState.AddModelError("", "Email already exist, Plz try another Email !!");
                }
            }
            catch
            {
                ModelState.AddModelError("", "Registration Failed !!");
                return View(e);
            }
            return View(e);
        }

        [Authorize]
        public IActionResult ProfilePhoto()
        {

            var userId = Convert.ToInt16(User.Identity!.Name);

            var user = _context.Users.FirstOrDefault(p => p.UserId == userId);

            var profileImagePath = string.IsNullOrEmpty(user?.UserImage)
                ? "/UserFile/default-profile.png"
                : $"/UserFile/{user.UserImage}";

            ViewBag.image = profileImagePath;

            return PartialView("_Profile");
        }




        [Authorize]
        [HttpGet]
        public IActionResult ProfileUpdate()
        {
            var update = _context.Users.Where(x => x.UserId.Equals(Convert.ToInt16(User.Identity!.Name))).FirstOrDefault();

            UserEdit edit = new()
            {
                UserId = update.UserId,
                Name = update.Name,
                Email = update.Email,
                Password = update.Password,
                Role = update.Role,
                UserImage = update.UserImage
            };
            return View(edit);
        }

        [Authorize]
        [HttpPost]

        public IActionResult ProfileUpdate(UserEdit e)
        {
            if (e.UserFile != null)
            {
                string filename = "UserFile" + Guid.NewGuid() + Path.GetExtension(e.UserFile.FileName);
                string filepath = Path.Combine(_env.WebRootPath, "UserFile", filename);
                using (FileStream stream = new FileStream(filepath, FileMode.Create))
                {
                    e.UserFile.CopyTo(stream);
                }
                e.UserImage = filename;
            }

            User l = new()
            {
                UserId = e.UserId,
                Name = e.Name,
                Email = e.Email,
                Password = e.Password,
                Role = e.Role,
                UserImage = e.UserImage
            };

            _context.Update(l);
            _context.SaveChanges();

            var user = _context.Users.FirstOrDefault(p => p.UserId.Equals(Convert.ToInt16(User.Identity!.Name)));
            ViewBag.image = string.IsNullOrEmpty(user?.UserImage) ? "default-profile.png" : user.UserImage;

            TempData["SuccessMessage"] = "Your profile has been successfully updated.";

            return RedirectToAction("Index", "Home");
        }

    }   
}
