using GuideMe.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GuideMe.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace GuideMe.Controllers
{
    public class ChatController : Controller
    {
        private readonly GuideMeContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(GuideMeContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index()
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.Name));

            // Get users with chat history
            var usersWithMessages = await _context.PrivateMessages
                .Where(m => m.SenderId == currentUserId || m.ReceiverId == currentUserId)
                .Select(m => m.SenderId == currentUserId ? m.ReceiverId : m.SenderId)
                .Distinct()
                .ToListAsync();

            var chatUsers = await _context.Users
                .Where(u => usersWithMessages.Contains(u.UserId))
                .Select(u => new {
                    User = u,
                    LastMessage = _context.PrivateMessages
                        .Where(m => (m.SenderId == currentUserId && m.ReceiverId == u.UserId) ||
                                   (m.SenderId == u.UserId && m.ReceiverId == currentUserId))
                        .OrderByDescending(m => m.SentAt)
                        .FirstOrDefault()
                })
                .OrderByDescending(x => x.LastMessage.SentAt)
                .ToListAsync();

            ViewBag.CurrentUserId = currentUserId;
            ViewBag.ChatUsers = chatUsers;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> SearchUsers(string searchTerm)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.Name));

            if (!string.IsNullOrEmpty(searchTerm))
            {
                if (string.IsNullOrEmpty(searchTerm))
                {
                    TempData["SearchError"] = "Please enter the username you want to search.";
                    return View("Index");
                }

                var searchResults = await _context.Users
                .Where(u => u.UserId != currentUserId &&
                           u.Role == "User" &&
                           EF.Functions.Collate(u.Name, "SQL_Latin1_General_CP1_CI_AS") == searchTerm)
                    .ToListAsync();

                if (!searchResults.Any())
                {
                    TempData["SearchError"] = "Username does not exist. Please try again.";
                }

                ViewBag.SearchResults = searchResults;
                return View("Index");
            }
            ViewBag.SearchResults = null;
            return View("Index");
        }


        [HttpPost]
        public async Task<IActionResult> SendPrivateMessage(int senderId, int receiverId, string message)
        {
            try
            {
                if (string.IsNullOrEmpty(message))
                    return Json(new { success = false, message = "Message cannot be empty" });

                var sender = await _context.Users.FindAsync(senderId);
                if (sender == null)
                    return Json(new { success = false, message = "Sender not found" });

                var currentTime = DateTime.Now;
                var privateMessage = new PrivateMessage
                {
                    SenderId = senderId,
                    ReceiverId = receiverId,
                    MessageText = message,
                    SentAt = DateOnly.FromDateTime(currentTime)
                };

                await _context.PrivateMessages.AddAsync(privateMessage);
                await _context.SaveChangesAsync();

                string senderImage = !string.IsNullOrEmpty(sender.UserImage)
                    ? $"/UserFile/{sender.UserImage}"
                    : "/UserFile/default-profile.png";

                // Send to both sender and receiver
                await _hubContext.Clients.Users(new[] { senderId.ToString(), receiverId.ToString() })
                    .SendAsync("ReceivePrivateMessage",
                        senderId,
                        sender.Name,
                        message,
                        currentTime,
                        senderImage);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetPrivateMessages(int userId)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.Name));

            var messages = await _context.PrivateMessages
                .Where(m =>
                    (m.SenderId == currentUserId && m.ReceiverId == userId) ||
                    (m.SenderId == userId && m.ReceiverId == currentUserId))
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            return Json(messages);
        }

        [HttpGet]
        public async Task<IActionResult> GetChatUsers()
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.Name));

            var usersWithMessages = await _context.PrivateMessages
                .Where(m => m.SenderId == currentUserId || m.ReceiverId == currentUserId)
                .Select(m => m.SenderId == currentUserId ? m.ReceiverId : m.SenderId)
                .Distinct()
                .ToListAsync();

            var chatUsers = await _context.Users
                .Where(u => usersWithMessages.Contains(u.UserId))
                .Select(u => new {
                    User = new
                    {
                        UserId = u.UserId,
                        Name = u.Name,
                        UserImage = !string.IsNullOrEmpty(u.UserImage) ? u.UserImage : null
                    },
                    LastMessage = _context.PrivateMessages
                        .Where(m => (m.SenderId == currentUserId && m.ReceiverId == u.UserId) ||
                                   (m.SenderId == u.UserId && m.ReceiverId == currentUserId))
                        .OrderByDescending(m => m.SentAt)
                        .Select(m => new {
                            MessageText = m.MessageText,
                            SentAt = m.SentAt,
                            SenderId = m.SenderId,
                            ReceiverId = m.ReceiverId
                        })
                        .FirstOrDefault(),
                    UnreadCount = _context.PrivateMessages
                        .Count(m => m.SenderId == u.UserId &&
                                   m.ReceiverId == currentUserId)
                })
                .OrderByDescending(x => x.LastMessage.SentAt)
                .ToListAsync();

            return Json(chatUsers);
        }


        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.Name));
            var users = await _context.Users
                .Where(u => u.UserId != currentUserId)
                .Select(u => new {
                    userId = u.UserId,
                    name = u.Name,
                    userImage = u.UserImage
                })
                .ToListAsync();
            return Json(users);
        }
    }
}