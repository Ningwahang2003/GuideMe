using GuideMe.Models;
using GuideMe.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace GuideMe.Controllers
{
    public class AdminController : Controller
    {
        private readonly GuideMeContext _context;
        private readonly IDataProtector _protector;
        private readonly IWebHostEnvironment _env;

        public AdminController(GuideMeContext context, DataSecurity key, IDataProtectionProvider _provider, IWebHostEnvironment env)
        {
            _context = context;
            _protector = _provider.CreateProtector(key.key);
            _env = env;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ViewUser()
        {
            var users = _context.Users.Where(user => user.Role != "Admin").ToList();
            foreach (var user in users)
            {
                try
                {
                    user.Password = _protector.Unprotect(user.Password);
                }
                catch
                {
                    user.Password = "Invalid Encrypted Password";
                }
            }

            return View(users);
        }

        public IActionResult UpdateUser(UserEdit l)
        {
            if (l.UserFile != null)
            {
                string filename = "UserFile" + Guid.NewGuid() + Path.GetExtension(l.UserFile.FileName);
                string filepath = Path.Combine(_env.WebRootPath, "UserFile", filename);
                using (FileStream stream = new FileStream(filepath, FileMode.Create))
                {
                    l.UserFile.CopyTo(stream);
                }
                l.UserImage = filename;
            }

            User e = new()
            {
                UserId = l.UserId,
                Name = l.Name,
                Email = l.Email,
                Password = l.Password,
                Role = l.Role,
                UserImage = l.UserImage
            };

            _context.Update(e);
            _context.SaveChanges();

            var user = _context.Users.FirstOrDefault(p => p.UserId.Equals(Convert.ToInt16(User.Identity!.Name)));
            ViewBag.image = string.IsNullOrEmpty(user?.UserImage) ? "default-profile.png" : user.UserImage;

            TempData["SuccessMessage"] = "Profile has been successfully updated.";

            return RedirectToAction("Index", "Home");
        }


    }
}
