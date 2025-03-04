using GuideMe.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

public class ChatHub : Hub
{
    private readonly GuideMeContext _context;

    public ChatHub(GuideMeContext context)
    {
        _context = context;
    }

    public async Task SendMessage(int groupId, string userName, string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        // Get user ID from database based on username
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Name == userName);
        if (user == null)
        {
            // Handle case where user doesn't exist, e.g., log an error or send a response.
            return;
        }

        // Create a new chat message
        var chatMessage = new ChatMessage
        {
            GroupId = groupId,
            UserId = user.UserId,
            MessageText = message,
            SentAt = DateTime.Now
        };

        _context.ChatMessages.Add(chatMessage);
        await _context.SaveChangesAsync();

        // Broadcast message to the group in real-time
        await Clients.Group(groupId.ToString()).SendAsync("ReceiveMessage", userName, message, DateTime.Now);
    }

    public async Task JoinGroup(string groupId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
    }

    public async Task LeaveGroup(string groupId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
    }
}
