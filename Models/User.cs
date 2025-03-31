using System;
using System.Collections.Generic;

namespace GuideMe.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Role { get; set; }

    public DateTime? LastLogin { get; set; }

    public string? UserImage { get; set; }

    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    public virtual ICollection<ContestEntry> ContestEntries { get; set; } = new List<ContestEntry>();

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();

    public virtual ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual ICollection<UrbanTreasure> UrbanTreasures { get; set; } = new List<UrbanTreasure>();

    public virtual ICollection<UserComment> UserComments { get; set; } = new List<UserComment>();

    public virtual ICollection<UserPost> UserPosts { get; set; } = new List<UserPost>();

    public virtual ICollection<UserVote> UserVotes { get; set; } = new List<UserVote>();

    public virtual ICollection<WeeklyContest> WeeklyContests { get; set; } = new List<WeeklyContest>();
}
