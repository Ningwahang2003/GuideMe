using System;
using System.Collections.Generic;

namespace GuideMe.Models;

public partial class GroupMember
{
    public int GroupMemberId { get; set; }

    public int GroupId { get; set; }

    public string UserName { get; set; } = null!;

    public int UserId { get; set; }

    public DateTime JoinedAt { get; set; }

    public string? LeaveReason { get; set; }

    public virtual Group Group { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
