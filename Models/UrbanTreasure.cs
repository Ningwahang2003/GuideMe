using System;
using System.Collections.Generic;

namespace GuideMe.Models;

public partial class UrbanTreasure
{
    public int UrbanTreasureId { get; set; }

    public string Image { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int UserId { get; set; }

    public int ProvinceId { get; set; }

    public string Title { get; set; } = null!;

    public string Location { get; set; } = null!;

    public virtual Province Province { get; set; } = null!;

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual User User { get; set; } = null!;
}
