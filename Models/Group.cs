using System;
using System.Collections.Generic;

namespace GuideMe.Models;

public partial class Group
{
    public int GroupId { get; set; }

    public string Name { get; set; } = null!;

    public string? Location { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateOnly? TravelStartDate { get; set; }

    public DateOnly? TravelEndDate { get; set; }

    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    public virtual ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();
}
