using GuideMe.Hubs;
using GuideMe.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace GuideMe.Controllers
{
    public class GroupFormationController : Controller
    {
        private readonly GuideMeContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public GroupFormationController(GuideMeContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.Name);
            if (userIdString == null)
                return Unauthorized();

            if (!int.TryParse(userIdString, out int userId))
                return BadRequest("Invalid User ID.");

            var joinedgroups = await _context.GroupMembers.Where(m => m.UserId == userId).Select(m => m.GroupId).ToListAsync();
            ViewData["JoinedGroups"] = joinedgroups;

            var groups = await _context.Groups.Include(g => g.GroupMembers).ThenInclude(gm => gm.User).ToListAsync();
            return View(groups);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Group gc)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.Name);
            if (userIdString == null) return Unauthorized();

            if (!int.TryParse(userIdString, out int userId))
            {
                return BadRequest("Invalid User ID.");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("User not found.");

            string userName = user.Name;

            bool isInGroup = await _context.GroupMembers.AnyAsync(m => m.UserId == userId);
            if (isInGroup)
            {
                TempData["ErrorMessage"] = "You can only be in one group at a time.";
                TempData["AlertType"] = "danger";
                return RedirectToAction("Index");
            }

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
            if (userIdString == null || !int.TryParse(userIdString, out int userId))
                return Unauthorized();

            bool isInGroup = await _context.GroupMembers.AnyAsync(m => m.UserId == userId);
            if (isInGroup)
            {
                TempData["ErrorMessage"] = "You can only join one group at a time.";
                return RedirectToAction("Index");
            }

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

            await _hubContext.Clients.Group(groupId.ToString())
                .SendAsync("UserJoined", userId, user.Name);

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


        [HttpPost]
        public async Task<IActionResult> SendMessage(IFormFile[] files, string messageText, int groupId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.Name));
            var timestamp = DateTime.Now;

            try
            {
                var attachmentPaths = new List<string>();
                if (files != null && files.Length > 0)
                {
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ChatFiles");
                    Directory.CreateDirectory(uploadPath);

                    foreach (var file in files)
                    {
                        var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                        var filePath = Path.Combine(uploadPath, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        attachmentPaths.Add($"/ChatFiles/{uniqueFileName}");
                    }
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user == null) return NotFound("User not found");

                var chatMessage = new ChatMessage
                {
                    GroupId = groupId,
                    UserId = userId,
                    MessageText = messageText ?? "",
                    Attachment = string.Join(",", attachmentPaths),
                    SentAt = timestamp
                };

                _context.ChatMessages.Add(chatMessage);
                await _context.SaveChangesAsync();

                string imageUrl = !string.IsNullOrEmpty(user.UserImage)
                    ? $"/UserFile/{user.UserImage}"
                    : "/UserFile/default-profile.png";

                await _hubContext.Clients.Group(groupId.ToString()).SendAsync("ReceiveMessage",
                    groupId,
                    user.Name,
                    messageText,
                    string.Join(",", attachmentPaths),
                    timestamp,
                    imageUrl);

                return Ok(new
                {
                    success = true,
                    message = "Message sent successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetMessages(int groupId)
        {
            var messages = await _context.ChatMessages
                .Where(m => m.GroupId == groupId)
                .Include(m => m.User)
                .OrderBy(m => m.SentAt)
                .Select(m => new
                {
                    messageId = m.ChatMessageId,
                    userName = m.User.Name,
                    messageText = m.MessageText,
                    attachment = m.Attachment,
                    sentAt = m.SentAt,
                    userImageUrl = !string.IsNullOrEmpty(m.User.UserImage)
                        ? $"/UserFile/{m.User.UserImage}"
                        : "/UserFile/default-profile.jpg"
                })
                .ToListAsync();

            return Json(messages);
        }

    }
}
