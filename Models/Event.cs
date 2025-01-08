using System;
using System.Collections.Generic;

namespace GuideMe.Models;

public partial class Event
{
    public int EventId { get; set; }

    public string EventTitle { get; set; } = null!;

    public string EventLocation { get; set; } = null!;

    public DateTime EventStartDate { get; set; }

    public DateTime EventEndDate { get; set; }

    public int UserId { get; set; }

    public virtual User User { get; set; } = null!;
}
