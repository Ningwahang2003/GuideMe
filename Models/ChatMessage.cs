using System;
using System.Collections.Generic;

namespace GuideMe.Models;

public partial class ChatMessage
{
    public int ChatMessageId { get; set; }

    public int GroupId { get; set; }

    public int UserId { get; set; }

    public string MessageText { get; set; }

    public string? Attachment { get; set; }

    public DateTime SentAt { get; set; }

    public virtual Group Group { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
