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

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
}
