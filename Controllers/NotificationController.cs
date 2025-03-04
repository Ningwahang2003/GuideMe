using GuideMe.Models;
using GuideMe.Security;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GuideMe.Controllers
{
    public class NotificationController : Controller
    {
        private readonly GuideMeContext _context;
        private readonly IDataProtector _protector;
        private readonly IWebHostEnvironment _env;

        public NotificationController(GuideMeContext context, DataSecurity key, IDataProtectionProvider _provider, IWebHostEnvironment env)
        {
            _context = context;
            _protector = _provider.CreateProtector(key.key);
            _env = env;
        }

        public IActionResult Index()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return RedirectToAction("Index", "Home");
            }

            var notifications = _context.Notifications.Where(n => n.UserId == userId).OrderByDescending(n => n.CreatedAt).ToList();
            ViewBag.Notifications = notifications;
            return View();
        }

        [HttpPost]
        public IActionResult MarkAsRead(int notificationId)
        {
            var notification = _context.Notifications.Find(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                _context.SaveChanges();
            }
            return RedirectToAction("Index","Notification");
        }
    }
}
