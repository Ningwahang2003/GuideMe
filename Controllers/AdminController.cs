using GuideMe.Models;
using GuideMe.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
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

            var existingUser = _context.Users.FirstOrDefault(u => u.UserId == l.UserId);
            if (existingUser == null)
            {
                TempData["ErrorMessage"] = "User not found!";
                return RedirectToAction("Index", "Home");
            }

            // Retain the existing password if none is provided
            string updatedPassword = string.IsNullOrEmpty(l.Password) ? existingUser.Password : l.Password;

            // Retain existing role if not provided
            string updatedRole = string.IsNullOrEmpty(l.Role) ? existingUser.Role : l.Role;

            existingUser.Name = l.Name;
            existingUser.Email = l.Email;
            existingUser.Password = updatedPassword; 
            existingUser.Role = updatedRole;
            existingUser.UserImage = l.UserImage ?? existingUser.UserImage;

            _context.Update(existingUser);
            _context.SaveChanges();

            var user = _context.Users.FirstOrDefault(p => p.UserId.Equals(Convert.ToInt16(User.Identity!.Name)));
            ViewBag.image = string.IsNullOrEmpty(user?.UserImage) ? "default-profile.png" : user.UserImage;

            TempData["SuccessMessage"] = "Profile has been successfully updated.";

            return RedirectToAction("Index", "Home");
        }


        /*Urban Treasures*/
        [HttpGet]
        public IActionResult ManageForm()
        {
            var proviences = _context.Provinces.ToList();
            return View(proviences);
        }

        [HttpPost]
        public IActionResult AddProvince(Province p)
        {
            if (ModelState.IsValid)
            {
                if (p.ProvinceImages != null && p.ProvinceImages.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "ProvinceImages");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string fileName = "ProvinceImage" + Guid.NewGuid() + Path.GetExtension(p.ProvinceImages.FileName);
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        p.ProvinceImages.CopyTo(stream);
                    }

                    p.ProvinceImage = "/ProvinceImages/" + fileName; 
                }

                Province pd = new()
                {
                    ProvinceName = p.ProvinceName,
                    ProvinceImage = p.ProvinceImage,
                    ProvinceDescription = p.ProvinceDescription
                };

                _context.Provinces.Add(pd);
                _context.SaveChanges();
                return RedirectToAction("ManageForm");
            }
            return View();
        }


        [HttpPost]
        public IActionResult DeleteProvince(int id)
        {
            try
            {
                var province = _context.Provinces
                    .Include(p => p.UrbanTreasures)
                    .FirstOrDefault(p => p.ProvinceId == id);

                if (province == null)
                {
                    return NotFound();
                }

                // Delete urban treasures
                _context.UrbanTreasures.RemoveRange(province.UrbanTreasures);

                // Finally delete the province
                _context.Provinces.Remove(province);
                _context.SaveChanges();

                return RedirectToAction("ManageForm");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Unable to delete province. Please ensure it has no related data.");
                return RedirectToAction("ManageForm");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUrbanTreasure(int id)
        {
            var urbanTreasure = await _context.UrbanTreasures.Include(u => u.User).FirstOrDefaultAsync(u => u.UrbanTreasureId == id);

            if (urbanTreasure != null)
            {
                if (!string.IsNullOrEmpty(urbanTreasure.Image))
                {
                    var imageNames = urbanTreasure.Image.Split(',');
                    foreach (var imageName in imageNames)
                    {
                        var imagePath = Path.Combine(_env.WebRootPath, "UrbanImage", imageName);
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }
                }

                var notification = new Notification
                {
                    UserId = urbanTreasure.UserId,
                    Message = $"Your Urban Treasure post '{urbanTreasure.Title}' has been removed by admin.",
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                    NotificationType = "Warning"
                };

                _context.Notifications.Add(notification);
                _context.UrbanTreasures.Remove(urbanTreasure);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Urban Treasure post deleted and user notified.";
            }

            return RedirectToAction("ProvinceContribution");
        }


        /*EventHandling*/
        [HttpGet]
        public IActionResult ManageEventRequest()
        {
            var requests = _context.Events.Where(e => e.IsApproved == false).ToList();
            return View(requests);
        }

        [HttpPost]
        public IActionResult ApproveEventRequest(int eventId)
        {
            var request = _context.Events.Find(eventId);
            if (request != null)
            {
                request.IsApproved = true;
                _context.SaveChanges();

                var notification = new Notification
                {
                    UserId = request.UserId,
                    Message = $"Your event request for '{request.EventTitle}' has been approved.",
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                    NotificationType = "Approved"
                };

                _context.Notifications.Add(notification);
                _context.SaveChanges();
            }
            return RedirectToAction("ManageEventRequest");
        }

        [HttpPost]
        public IActionResult RejectEventRequest(int eventId)
        {
            var request = _context.Events.Find(eventId);
            if (request != null)
            {
                _context.Events.Remove(request);
                _context.SaveChanges();

                var notification = new Notification
                {
                    UserId = request.UserId,
                    Message = $"Your event request for '{request.EventTitle}' has been rejected.",
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                    NotificationType = "Rejected"
                };

                _context.Notifications.Add(notification);
                _context.SaveChanges();
            }
            return RedirectToAction("ManageEventRequest");
        }


        /*Weekly Contest*/
        [HttpGet]
        public IActionResult ViewWeeklyContest()
        {
            var weeklycontest = _context.WeeklyContests.ToList();
            return View(weeklycontest);
        }

        [HttpGet]
        public IActionResult AddContest()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddContest(WeeklyContest contest)
        {
            var userId = User.FindFirst(ClaimTypes.Name)?.Value;
            if (userId != null)
            {
                contest.UserId = int.Parse(userId);
                _context.WeeklyContests.Add(contest);
                await _context.SaveChangesAsync();
                ModelState.Clear();
                TempData["SuccessMessage"] = "Contest added successfully!";
                return RedirectToAction("ViewWeeklyContest");
            }
            return View(contest);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteContest(int id)
        {
            try
            {
                var contest = await _context.WeeklyContests.Include(c => c.ContestEntries).ThenInclude(ce => ce.UserVotes).FirstOrDefaultAsync(c => c.ContestId == id);

                if (contest != null)
                {
                    foreach (var entry in contest.ContestEntries)
                    {
                        _context.UserVotes.RemoveRange(entry.UserVotes);
                    }

                    _context.ContestEntries.RemoveRange(contest.ContestEntries);

                    _context.WeeklyContests.Remove(contest);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Contest deleted successfully";
                }
                return RedirectToAction(nameof(ViewWeeklyContest));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Unable to delete contest. Please ensure it has no active participants.";
                return RedirectToAction(nameof(ViewWeeklyContest));
            }
        }



        [HttpPost]
        public async Task<IActionResult> DeleteUserPost(int postId)
        {
            var post = await _context.UserPosts
                .Include(p => p.User)
                .Include(p => p.PostLikes)
                .Include(p => p.UserComments)
                .FirstOrDefaultAsync(p => p.PostId == postId);

            if (post != null)
            {
                // Removing related likes
                _context.PostLikes.RemoveRange(post.PostLikes);

                // Removing related comments
                _context.UserComments.RemoveRange(post.UserComments);

                // Creating notification for the user
                var notification = new Notification
                {
                    UserId = post.UserId,
                    Message = $"Your post has been removed by admin due to violence concerns.",
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                    NotificationType = "Warning"
                };

                _context.Notifications.Add(notification);

                _context.UserPosts.Remove(post);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Post deleted and user notified.";
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
