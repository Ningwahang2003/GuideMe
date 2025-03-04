using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GuideMe.Models;

public partial class UserPost
{
    public int PostId { get; set; }

    public int UserId { get; set; }

    public string MediaPath { get; set; } = null!;

    public string? Caption { get; set; }

    public DateTime CreatedAt { get; set; }

    [NotMapped]
    [DataType(DataType.Upload)]
    public IFormFile[]? MediaFiles { get; set; }

    public virtual ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();

    public virtual User User { get; set; } = null!;

    public virtual ICollection<UserComment> UserComments { get; set; } = new List<UserComment>();
}
