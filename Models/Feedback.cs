using System;
using System.Collections.Generic;

namespace GuideMe.Models;

public partial class Feedback
{
    public int FeedbackId { get; set; }

    public int? UserId { get; set; }

    public string? Content { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsRead { get; set; }

    public virtual User? User { get; set; }
}
