using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GuideMe.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace GuideMe.Controllers
{
    [Authorize]
    public class UserContentController : Controller
    {
        private readonly GuideMeContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UserContentController(GuideMeContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            var posts = _context.UserPosts.Include(p => p.User).ToList();
            return View(posts);
        }

        [HttpGet]
        public IActionResult GetUserPosts()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var userPosts = _context.UserPosts.Where(p => p.UserId == userId).ToList();
            return View(userPosts);
        }

        [HttpGet]
        public IActionResult CreatePost()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> CreatePost(List<IFormFile> mediaFiles, string caption)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            if (mediaFiles == null || mediaFiles.Count == 0)
            {
                ModelState.AddModelError("", "At least one media file is required.");
                return View();
            }

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "usercontent");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            List<string> filePaths = new List<string>();

            foreach (var mediaFile in mediaFiles)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(mediaFile.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await mediaFile.CopyToAsync(fileStream);
                }

                filePaths.Add("/usercontent/" + fileName);
            }

            var post = new UserPost
            {
                UserId = userId,
                MediaPath = string.Join(",", filePaths),  // Store as comma-separated
                Caption = caption,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserPosts.Add(post);
            await _context.SaveChangesAsync();

            return RedirectToAction("GetUserPosts");
        }


        [HttpPost]
        public async Task<IActionResult> DeletePost(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var post = await _context.UserPosts.FindAsync(id);
            if (post == null)
            {
                ModelState.AddModelError("", "Post not found.");
                return View("GetUserPosts");
            }

            if (post.UserId != userId)
            {
                ModelState.AddModelError("", "You are not authorized to delete this post.");
                return View("GetUserPosts");
            }

            _context.UserPosts.Remove(post);
            await _context.SaveChangesAsync();
            return RedirectToAction("GetUserPosts");
        }

    }
}
