using GuideMe.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace GuideMe.Controllers
{
    public class GroupFormationController : Controller
    {
        private readonly GuideMeContext _context;

        public GroupFormationController(GuideMeContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.Name);
            if (userIdString == null)
                return Unauthorized();

            if (!int.TryParse(userIdString, out int userId))
                return BadRequest("Invalid User ID.");

            // Get all groups the user is a member of
            var joinedgroups = await _context.GroupMembers.Where(m => m.UserId == userId).Select(m => m.GroupId).ToListAsync();
            ViewData["JoinedGroups"] = joinedgroups;

            var groups = await _context.Groups.ToListAsync();
            return View(groups);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        // Create a new group
        [HttpPost]
        public async Task<IActionResult> Create(Group gc)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.Name);
            if (userIdString == null) return Unauthorized();

            // Convert string UserId to int
            if (!int.TryParse(userIdString, out int userId))
            {
                return BadRequest("Invalid User ID.");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("User not found.");

            string userName = user.Name;

            // Check if user is already in a group
            bool isInGroup = await _context.GroupMembers.AnyAsync(m => m.UserId == userId);
            if (isInGroup) return BadRequest("You can only be in one group at a time.");

            Group groups = new()
            {
                Name = gc.Name,
                Location = gc.Location,
                TravelStartDate = gc.TravelStartDate,
                TravelEndDate = gc.TravelEndDate,
                Description = gc.Description,
                IsActive = true
            };

            _context.Groups.Add(groups);
            await _context.SaveChangesAsync();

            var groupMember = new GroupMember
            {
                GroupId = groups.GroupId, 
                UserId = userId,
                UserName = userName,
                JoinedAt = DateTime.Now
            };

            _context.GroupMembers.Add(groupMember);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> Join(int groupId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.Name);
            if (userIdString == null) return Unauthorized();

            if (!int.TryParse(userIdString, out int userId))
            {
                return BadRequest("Invalid User ID.");
            }

            // Ensure user is not in another group
            bool isInGroup = await _context.GroupMembers.AnyAsync(m => m.UserId == userId);
            if (isInGroup) return BadRequest("You can only join one group at a time.");

            var group = await _context.Groups.FindAsync(groupId);
            if (group == null || !group.IsActive) return NotFound("Group not found or inactive.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("User not found.");

            var groupMember = new GroupMember
            {
                GroupId = groupId,
                UserId = userId,
                UserName = user.Name,
                JoinedAt = DateTime.Now
            };

            _context.GroupMembers.Add(groupMember);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int groupId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.Name);
            if (userIdString == null)
            {
                ModelState.AddModelError("", "You must be logged in to edit a group.");
                return View("Edit");
            }

            if (!int.TryParse(userIdString, out int userId))
            {
                ModelState.AddModelError("", "Invalid User ID.");
                return View("Edit");
            }

            var isMember = await _context.GroupMembers.AnyAsync(m => m.GroupId == groupId && m.UserId == userId);
            if (!isMember)
            {
                ModelState.AddModelError("", "You can only edit groups you are a member of.");
                return View("Edit");
            }

            var group = await _context.Groups.FindAsync(groupId);
            if (group == null)
            {
                ModelState.AddModelError("", "Group not found.");
                return View("Edit");
            }

            return View(group);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int groupId, Group updatedGroup)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.Name);
            if (userIdString == null)
            {
                ModelState.AddModelError("", "You must be logged in to edit a group.");
                return View(updatedGroup);
            }

            if (!int.TryParse(userIdString, out int userId))
            {
                ModelState.AddModelError("", "Invalid User ID.");
                return View(updatedGroup);
            }

            // Ensure the user is a member before allowing editing
            var isMember = await _context.GroupMembers.AnyAsync(m => m.GroupId == groupId && m.UserId == userId);
            if (!isMember)
            {
                ModelState.AddModelError("", "You can only edit groups you are a member of.");
                return View(updatedGroup);
            }

            var group = await _context.Groups.FindAsync(groupId);
            if (group == null)
            {
                ModelState.AddModelError("", "Group not found.");
                return View(updatedGroup);
            }

            if (!ModelState.IsValid)
            {
                return View(updatedGroup);
            }

            group.Name = updatedGroup.Name;
            group.Location = updatedGroup.Location;
            group.TravelStartDate = updatedGroup.TravelStartDate;
            group.TravelEndDate = updatedGroup.TravelEndDate;
            group.Description = updatedGroup.Description;

            _context.Groups.Update(group);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Group updated successfully!";
            return RedirectToAction("Index");
        }



        [HttpGet]
        public IActionResult Leave(int groupId)
        {
            var group = _context.Groups.FirstOrDefault(g => g.GroupId == groupId);
            if (group == null)
            {
                return NotFound();
            }

            ViewData["GroupId"] = groupId;

            return View(groupId);
        }

        [HttpPost]
        public async Task<IActionResult> Leave(int groupId, string leaveReason)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.Name);
            if (userIdString == null) return Unauthorized();

            if (!int.TryParse(userIdString, out int userId))
            {
                return BadRequest("Invalid User ID.");
            }

            var membership = await _context.GroupMembers.FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId);
            if (membership == null) return BadRequest("You are not in this group.");

            if (string.IsNullOrEmpty(leaveReason))
            {
                ModelState.AddModelError("leaveReason", "Leave reason cannot be empty.");
                return View();
            }

            membership.LeaveReason = leaveReason;
            _context.GroupMembers.Remove(membership);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Terminate(int groupId)
        {
            var group = await _context.Groups.FindAsync(groupId);
            if (group == null) return NotFound();

            group.IsActive = false;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }



        /*GroupChat*/
        [HttpGet]
        public async Task<IActionResult> GroupChat(int groupId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                ModelState.AddModelError("", "User identification failed.");
                return View();
            }

            var isMember = await _context.GroupMembers
                .AnyAsync(m => m.GroupId == groupId && m.UserId == userId);

            if (!isMember)
            {
                ModelState.AddModelError("", "You must be a member of this group to chat.");
                return View();
            }

            var groupMembers = await _context.GroupMembers
                .Where(m => m.GroupId == groupId)
                .Include(m => m.User)
                .ToListAsync();

            Console.WriteLine($"Group Members Count: {groupMembers.Count}");

            if (groupMembers == null || !groupMembers.Any())
            {
                ViewBag.GroupMembers = new List<GuideMe.Models.User>();
            }
            else
            {
                ViewBag.GroupMembers = groupMembers.Select(m => m.User).ToList();
            }

            var messages = await _context.ChatMessages
                .Where(m => m.GroupId == groupId)
                .Include(m => m.User)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            return View(messages);
        }


        [HttpPost]
        public async Task<IActionResult> SendMessage(int groupId, string messageText)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                ModelState.AddModelError("", "User identification failed.");
                return RedirectToAction("GroupChat", new { groupId });
            }

            var isMember = await _context.GroupMembers
                .AnyAsync(m => m.GroupId == groupId && m.UserId == userId);

            if (!isMember)
            {
                ModelState.AddModelError("", "You must be a member of this group to send messages.");
                return RedirectToAction("GroupChat", new { groupId });
            }

            var message = new ChatMessage
            {
                GroupId = groupId,
                UserId = userId,
                MessageText = messageText,
                SentAt = DateTime.Now
            };

            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();

            return RedirectToAction("GroupChat", new { groupId });
        }




    }
}
