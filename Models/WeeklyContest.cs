using System;
using System.Collections.Generic;

namespace GuideMe.Models;

public partial class WeeklyContest
{
    public int ContestId { get; set; }

    public string ContestType { get; set; } = null!;

    public string Instructions { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string? Status { get; set; }

    public int UserId { get; set; }

    public string? ContestPhase { get; set; }

    public int? WinnerUserId { get; set; }

    public virtual ICollection<ContestEntry> ContestEntries { get; set; } = new List<ContestEntry>();

    public virtual User User { get; set; } = null!;
}
