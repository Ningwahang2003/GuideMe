using System;
using System.Collections.Generic;

namespace GuideMe.Models;

public partial class PrivateMessage
{
    public int PrivateMessageId { get; set; }

    public int SenderId { get; set; }

    public int ReceiverId { get; set; }

    public string MessageText { get; set; } = null!;

    public DateOnly? SentAt { get; set; }
}
