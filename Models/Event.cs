using System;
using System.Collections.Generic;

namespace GuideMe.Models;

public partial class Event
{
    public int EventId { get; set; }

    public string EventTitle { get; set; } = null!;

    public string? EventLocation { get; set; }

    public DateTime? EventStartDate { get; set; }

    public DateTime? EventEndDate { get; set; }

    public int UserId { get; set; }

    public bool? IsApproved { get; set; }

    public string EventDescription { get; set; } = null!;

    public bool IsAdded { get; set; }

    public string? EventTime { get; set; }

    public bool IsExpired { get; set; }

    public virtual User User { get; set; } = null!;
}
