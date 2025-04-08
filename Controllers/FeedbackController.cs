using GuideMe.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GuideMe.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly GuideMeContext _context;

        public FeedbackController(GuideMeContext context)
        {
            _context = context;
        }

        [Authorize]
        public IActionResult Index()
        {
            var userId = Convert.ToInt32(User.Identity.Name);
            var feedbacks = _context.Feedbacks.Where(f => f.UserId == userId).OrderByDescending(f => f.CreatedAt).ToList();
            return View(feedbacks);
        }


        [Authorize]
        [HttpGet]
        public IActionResult Submit()
        {
            return View(new Feedback());
        }

        [Authorize]
        [HttpPost]
        public IActionResult Submit(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                TempData["Error"] = "Feedback cannot be empty";
                return RedirectToAction("Index", "Home");
            }

            var userId = Convert.ToInt32(User.Identity.Name);
            var feedback = new Feedback
            {
                UserId = userId,
                Content = content,
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            _context.Feedbacks.Add(feedback);
            _context.SaveChanges();

            TempData["Success"] = "Thank you for your feedback!";
            return RedirectToAction("Index", "Home");
        }


        [Authorize(Roles = "Admin")]
        public IActionResult ManageFeedback()
        {
            var feedbacks = _context.Feedbacks.Include(f => f.User).OrderByDescending(f => f.CreatedAt).ToList();
            return View(feedbacks);
        }
    }
}
