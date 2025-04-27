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

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetUserPosts()
        {
            if (!User.Identity.IsAuthenticated)
            {
                Console.WriteLine("❌ User is NOT authenticated!");
                return Unauthorized();
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                Console.WriteLine("❌ User ID not found in claims.");
                return Unauthorized();
            }

            Console.WriteLine($"✅ Authenticated User ID: {userId}");

            var userPosts = _context.UserPosts
                .Where(p => p.UserId == userId)
                .Include(p => p.User)
                .Include(p => p.PostLikes)
                .Include(p => p.UserComments)
                .ThenInclude(c => c.User)
                .ToList();

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
                MediaPath = string.Join(",", filePaths),
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
                return NotFound();
            }

            if (post.UserId != userId)
            {
                return Unauthorized();
            }

            var postLikes = _context.PostLikes.Where(pl => pl.PostId == id);
            _context.PostLikes.RemoveRange(postLikes);

            var postComments = _context.UserComments.Where(pc => pc.PostId == id);
            _context.UserComments.RemoveRange(postComments);

            _context.UserPosts.Remove(post);
            await _context.SaveChangesAsync();

            return RedirectToAction("GetUserPosts");
        }

        [HttpPost]
        public async Task<IActionResult> LikePost(int postId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Json(new { success = false });
            }

            var existingLike = await _context.PostLikes.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
            bool isLiked = false;

            if (existingLike != null)
            {
                _context.PostLikes.Remove(existingLike);
            }
            else
            {
                var like = new PostLike { PostId = postId, UserId = userId };
                _context.PostLikes.Add(like);
                isLiked = true;
            }

            await _context.SaveChangesAsync();
            int likeCount = await _context.PostLikes.CountAsync(l => l.PostId == postId);

            return Json(new { success = true, likeCount, isLiked });
        }



        [HttpPost]
        public async Task<IActionResult> CommentPost(int postId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false });
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Json(new { success = false });
            }

            var comment = new UserComment
            {
                PostId = postId,
                UserId = userId,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserComments.Add(comment);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(userId);
            string userName = user?.Name ?? "Anonymous";

            return Json(new { success = true, userName, content });
        }

    }
}
