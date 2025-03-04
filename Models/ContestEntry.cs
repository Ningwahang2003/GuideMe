using System;
using System.Collections.Generic;

namespace GuideMe.Models;

public partial class ContestEntry
{
    public int ContestEntryId { get; set; }

    public int ContestId { get; set; }

    public int UserId { get; set; }

    public string Submission { get; set; } = null!;

    public int VoteCount { get; set; }

    public string? Title { get; set; }

    public string? Descriptions { get; set; }

    public virtual WeeklyContest Contest { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public virtual ICollection<UserVote> UserVotes { get; set; } = new List<UserVote>();
}
