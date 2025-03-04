using GuideMe.Models;
using GuideMe.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace GuideMe.Controllers
{
    public class ContributionController : Controller
    {
        private readonly GuideMeContext _context;
        private readonly IDataProtector _protector;
        private readonly IWebHostEnvironment _env;

        public ContributionController(GuideMeContext context, DataSecurity key, IDataProtectionProvider _provider, IWebHostEnvironment env)
        {
            _context = context;
            _protector = _provider.CreateProtector(key.key);
            _env = env;
        }
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public IActionResult UrbanTreasures()
        {
            ViewBag.Provinces = _context.Provinces.ToList();
            return View();
        }

        [Authorize]
        [HttpPost]
        public IActionResult UrbanTreasures(UrbanTreasureEdit ue)
        {
            if (ue.UrbanImage != null && ue.UrbanImage.Count() > 5)
            {
                ModelState.AddModelError("", "You can only upload up to 5 images at a time.");
                ViewBag.Provinces = _context.Provinces.ToList();
                return View("UrbanTreasures", ue);
            }

            List<string> images = new List<string>();

            if (ue.UrbanImage != null)
            {
                foreach (var urbanImage in ue.UrbanImage)
                {
                    if (urbanImage.Length > 0)
                    {
                        string filename = "UrbanImage" + Guid.NewGuid() + Path.GetExtension(urbanImage.FileName);
                        string filepath = Path.Combine(_env.WebRootPath, "UrbanImage", filename);
                        using (FileStream stream = new FileStream(filepath, FileMode.Create))
                        {
                            urbanImage.CopyTo(stream);
                        }
                        images.Add(filename);
                    }
                }
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                ModelState.AddModelError("", "Unable to determine the current user.");
                ViewBag.Provinces = _context.Provinces.ToList();
                return View("UrbanTreasures", ue);
            }

            if (string.IsNullOrEmpty(ue.Description) || ue.ProvinceId == 0 || !images.Any())
            {
                ModelState.AddModelError("", "All fields are required.");
                ViewBag.Provinces = _context.Provinces.ToList();
                return View("UrbanTreasures", ue);
            }

            try
            {
                UrbanTreasure ut = new()
                {
                    UrbanTreasureId = ue.UrbanTreasureId,
                    Image = string.Join(",", images),
                    Description = ue.Description,
                    Title = ue.Title,
                    Location = ue.Location,
                    ProvinceId = ue.ProvinceId,
                    UserId = userId
                };

                _context.UrbanTreasures.Add(ut);
                _context.SaveChanges();
                return RedirectToAction("UrbanTreasures", "Contribution");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while saving the data. Please try again.");
                return View("UrbanTreasures", ue);
            }
        }

        public IActionResult ProvinceContribution(int id)
        {
            var province = _context.Provinces.Include(p => p.UrbanTreasures).ThenInclude(ut => ut.User).FirstOrDefault(p => p.ProvinceId == id);

            if (province == null)
            {
                return NotFound();
            }
            return View(province);
        }

        [HttpGet]
        public IActionResult ViewEvent(string location, DateTime? date)
        {
            var expiredEvents = _context.Events.Where(e => e.EventEndDate < DateTime.Now && e.IsExpired == false).ToList();
            if (expiredEvents.Any())
            {
                foreach (var e in expiredEvents)
                {
                    e.IsExpired = true;
                }
                _context.SaveChanges();
            }

            var events = _context.Events.Where(e => e.IsAdded == true && e.IsExpired == false).AsQueryable();

            if (!string.IsNullOrEmpty(location))
            {
                events = events.Where(e => e.EventLocation.Contains(location));
            }

            if (date.HasValue)
            {
                events = events.Where(e => e.EventStartDate <= date && e.EventEndDate >= date);
            }

            return View(events.ToList());
        }




        [Authorize]
        [HttpGet]
        public IActionResult RequestEvent()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                ModelState.AddModelError("", "User identification failed.");
                return View();
            }

            // Check if the user already has an approved event
            var existingEvent = _context.Events.FirstOrDefault(e => e.UserId == userId && e.IsApproved == true);
            if (existingEvent != null)
            {
                if ((bool)existingEvent.IsApproved)
                {
                    return RedirectToAction("AddEvent");
                }
                else
                {
                    ModelState.AddModelError("", "Your event request is still awaiting approval.");
                    return View();
                }
            }

            return View();
        }

        [Authorize]
        [HttpPost]
        public IActionResult RequestEvent(Event request)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                ModelState.AddModelError("", "User identification failed.");
                return View(request);
            }

            // Check if the user already submitted a request
            var existingEvent = _context.Events.FirstOrDefault(e => e.UserId == userId && e.IsApproved == false);
            if (existingEvent != null)
            {
                ModelState.AddModelError("", "Your event request is still awaiting approval.");
                return View(request);
            }

            var newRequest = new Event
            {
                UserId = userId,
                EventTitle = request.EventTitle,
                EventDescription = request.EventDescription,
                EventStartDate = request.EventStartDate,
                EventEndDate = request.EventEndDate,
                EventTime = request.EventTime,
                IsApproved = false,
                IsAdded = false
            };
            _context.Events.Add(newRequest);
            _context.SaveChanges();
            return RedirectToAction("ViewEvent","Contribution");
        }


        [Authorize]
        [HttpGet]
        public IActionResult AddEvent()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public IActionResult AddEvent(Event model)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                ModelState.AddModelError("", "User identification failed.");
                return View(model);
            }

            var approvedRequest = _context.Events.Any(e => e.UserId == userId && e.IsApproved == true);

            if (!approvedRequest)
            {
                ModelState.AddModelError("", "You must request admin approval before adding an event.");
                return RedirectToAction("RequestEvent");
            }

            var newEvent = new Event
            {
                UserId = userId,
                EventTitle = model.EventTitle,
                EventLocation = model.EventLocation,
                EventDescription = model.EventDescription,
                EventStartDate = model.EventStartDate,
                EventEndDate = model.EventEndDate,
                EventTime = model.EventTime,
                IsApproved = true,
                IsAdded = true
            };

            _context.Events.Add(newEvent);
            _context.SaveChanges();

            return RedirectToAction("ViewEvent", "Contribution");
        }


        [Authorize]
        [HttpGet]
        public IActionResult CheckEventRequest()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return RedirectToAction("RequestEvent");
            }

            var eventRequest = _context.Events.FirstOrDefault(e => e.UserId == userId && e.IsApproved == true);

            if (eventRequest == null)
            {
                return RedirectToAction("RequestEvent");
            }

            var addedEvent = _context.Events.Any(e => e.UserId == userId && e.IsApproved == true && e.IsAdded == true);

            if (addedEvent)
            {
                eventRequest.IsApproved = false;
                eventRequest.IsAdded = false;
                eventRequest.EventTitle = "Pending Request";
                eventRequest.EventDescription = "Awaiting new event details";
                eventRequest.EventStartDate = DateTime.Now;
                eventRequest.EventEndDate = DateTime.Now.AddDays(1);
                eventRequest.EventTime = null;

                _context.SaveChanges();

                return RedirectToAction("RequestEvent");
            }

            return RedirectToAction("AddEvent");
        }

    }
}

