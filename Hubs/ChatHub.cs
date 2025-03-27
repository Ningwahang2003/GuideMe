using GuideMe.Models; // Assuming your User model is in this namespace
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace GuideMe.Hubs
{
    public class ChatHub : Hub
    {
        private readonly GuideMeContext _context;

        public ChatHub(GuideMeContext context)
        {
            _context = context;
        }


        // Method to send private message
        public async Task SendPrivateMessage(int senderId, int receiverId, string message)
        {
            await Clients.User(receiverId.ToString()).SendAsync("ReceivePrivateMessage", senderId, message);
        }



        // Join a group for real-time messaging
        public async Task JoinGroup(int groupId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupId.ToString());
        }

        public async Task LeaveGroup(int groupId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId.ToString());
        }

        public async Task SendMessage(int groupId, int userId, string message, List<IFormFile> attachments, DateTime timestamp)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return;

            var attachmentPaths = new List<string>();

            if (attachments != null && attachments.Any())
            {
                foreach (var file in attachments)
                {
                    if (file.Length > 0)
                    {
                        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ChatFiles", fileName);

                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        attachmentPaths.Add($"/ChatFiles/{fileName}");
                    }
                }
            }

            var chatMessage = new ChatMessage
            {
                GroupId = groupId,
                UserId = userId,
                MessageText = message ?? "",
                Attachment = string.Join(",", attachmentPaths),
                SentAt = timestamp
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            string imageUrl = !string.IsNullOrEmpty(user.UserImage)
                ? $"/UserFile/{user.UserImage}"
                : "/UserFile/default-profile.png";

            await Clients.Group(groupId.ToString()).SendAsync("ReceiveMessage",
                groupId,
                user.Name,
                message,
                string.Join(",", attachmentPaths),
                timestamp,
                imageUrl);
        }
    }
}
