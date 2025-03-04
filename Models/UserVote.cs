using System;
using System.Collections.Generic;

namespace GuideMe.Models;

public partial class UserVote
{
    public int UserVoteId { get; set; }

    public int UserId { get; set; }

    public int ContestEntryId { get; set; }

    public DateTime? VoteDate { get; set; }

    public virtual ContestEntry ContestEntry { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
