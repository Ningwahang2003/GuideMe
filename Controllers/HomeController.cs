using System.Diagnostics;
using GuideMe.Models;
using GuideMe.Security;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System;
using Microsoft.EntityFrameworkCore;

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

        [HttpGet]
        public IActionResult Index()
        {
            var today = DateTime.UtcNow;

            var activeContest = _context.WeeklyContests.FirstOrDefault(c => c.Status == "Active");

            if (activeContest != null)
            {
                if (today.Date > activeContest.EndDate.Date)
                {
                    if (activeContest.WinnerUserId == null)
                    {
                        var winningEntry = _context.ContestEntries.Where(e => e.ContestId == activeContest.ContestId).OrderByDescending(e => e.VoteCount).FirstOrDefault();

                        if (winningEntry != null)
                        {
                            activeContest.WinnerUserId = winningEntry.UserId;

                        }

                    }

                    activeContest.Status = "Inactive";
                    _context.SaveChanges();

                    var nextContest = _context.WeeklyContests.Where(c => c.Status == "Inactive").OrderBy(c => c.ContestId).FirstOrDefault();

                    if (nextContest != null)
                    {
                        nextContest.Status = "Active";
                        nextContest.StartDate = today.Date;
                        nextContest.EndDate = today.Date.AddDays(6);
                        _context.SaveChanges();
                        activeContest = nextContest;
                    }
                }
            }
            else
            {
                activeContest = _context.WeeklyContests.Where(c => c.Status == "Inactive").OrderBy(c => c.StartDate).FirstOrDefault();

                if (activeContest != null)
                {
                    activeContest.Status = "Active";
                    activeContest.StartDate = today.Date;
                    activeContest.EndDate = today.Date.AddDays(6);
                    _context.SaveChanges();
                }
            }

            var winner = (activeContest != null && activeContest.WinnerUserId != null)
            ? _context.Users.Where(u => u.UserId == activeContest.WinnerUserId).FirstOrDefault()
            : null;

            List<ContestEntry> contestEntries = new List<ContestEntry>();

            if (activeContest != null)
            {
                contestEntries = _context.ContestEntries.Where(e => e.ContestId == activeContest.ContestId).ToList();
            }


            var homeViewModel = new HomeViewModel
            {
                UpcomingEvents = _context.Events.Where(e => e.EventStartDate >= today && e.IsAdded && !e.IsExpired).OrderBy(e => e.EventStartDate).Take(4).ToList(),

                UpcomingContests = activeContest != null ? new List<WeeklyContest> { activeContest } : new List<WeeklyContest>()
            };

            return View(homeViewModel);
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



        /*WeeklyContest*/
        [HttpGet]
        public IActionResult ViewContest(int contestId)
        {
            var contestEntries = _context.ContestEntries.Where(e => e.ContestId == contestId).Include(e => e.User).ToList();

            return View(contestEntries);
        }

        [HttpGet("participatecontest/{contestId}")]
        public IActionResult ParticipateContest(int contestId)
        {
            ContestEntry contestEntry = new ContestEntry { ContestId = contestId };
            return View(contestEntry);
        }

        [HttpPost("participatecontest/{contestId}")]
        public IActionResult ParticipateContest(ContestEntry ce, IEnumerable<IFormFile> ContestImages)
        {
            if (ContestImages == null || ContestImages.Count() == 0)
            {
                ModelState.AddModelError("", "Please upload at least one image.");
                return View("ParticipateContest", ce);
            }
            if (ContestImages.Count() > 5)
            {
                ModelState.AddModelError("", "You can only upload up to 5 images at a time.");
                return View("ParticipateContest", ce);
            }

            List<string> images = new List<string>();
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };

            // Loop through each uploaded image.
            foreach (var image in ContestImages)
            {
                if (image.Length > 0)
                {
                    string extension = Path.GetExtension(image.FileName).ToLower();
                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("", "Only image files (.jpg, .jpeg, .png) are allowed.");
                        return View("ParticipateContest", ce);
                    }

                    string filename = "ContestSubmission_" + Guid.NewGuid() + extension;
                    string filepath = Path.Combine(_env.WebRootPath, "ContestSubmissions", filename);
                    using (FileStream stream = new FileStream(filepath, FileMode.Create))
                    {
                        image.CopyTo(stream);
                    }
                    images.Add(filename);
                }
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                ModelState.AddModelError("", "Unable to determine the current user.");
                return View("ParticipateContest", ce);
            }

            if (string.IsNullOrEmpty(ce.Title) || string.IsNullOrEmpty(ce.Descriptions) || ce.ContestId == 0 || !images.Any())
            {
                ModelState.AddModelError("", "All fields are required.");
                return View("ParticipateContest", ce);
            }

            var contest = _context.WeeklyContests.FirstOrDefault(c => c.ContestId == ce.ContestId);
            if (contest == null)
            {
                ModelState.AddModelError("", "Invalid contest.");
                return View("ParticipateContest", ce);
            }

            var existingEntry = _context.ContestEntries.FirstOrDefault(e => e.ContestId == ce.ContestId && e.UserId == userId);
            if (existingEntry != null)
            {
                ModelState.AddModelError("", "You have already participated in this contest.");
                return View("ParticipateContest", ce);
            }

            ContestEntry entry = new ContestEntry
            {
                ContestId = ce.ContestId,
                UserId = userId,
                Submission = string.Join(",", images),
                VoteCount = 0,
                Title = ce.Title,
                Descriptions = ce.Descriptions
            };

            _context.ContestEntries.Add(entry);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Your contest submission has been successfully uploaded!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Vote(int contestEntryId)
        {
            var contestEntry = _context.ContestEntries.FirstOrDefault(e => e.ContestEntryId == contestEntryId);
            if (contestEntry == null)
            {
                TempData["ErrorMessage"] = "Contest entry not found.";
                return RedirectToAction("ViewContest", new { contestId = 0 });
            }

            int contestId = contestEntry.ContestId;
            var userIdClaim = User.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                TempData["ErrorMessage"] = "Unable to determine the current user.";
                return RedirectToAction("ViewContest", new { contestId = contestId });
            }

            var existingVoteInContest = _context.UserVotes.Any(v => v.UserId == userId && v.ContestEntry.ContestId == contestId);

            if (existingVoteInContest)
            {
                TempData["ErrorMessage"] = "You have already voted in this contest.";
                return RedirectToAction("ViewContest", new { contestId = contestId });
            }

            var userVote = new UserVote
            {
                UserId = userId,
                ContestEntryId = contestEntryId,
                VoteDate = DateTime.UtcNow
            };

            _context.UserVotes.Add(userVote);
            contestEntry.VoteCount += 1;

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Your vote has been recorded!";
            return RedirectToAction("ViewContest", new { contestId = contestId });
        }




    }
}
