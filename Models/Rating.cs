using System;
using System.Collections.Generic;

namespace GuideMe.Models;

public partial class Rating
{
    public int RatingId { get; set; }

    public int UserId { get; set; }

    public int UrbanTreasureId { get; set; }

    public int RatingValue { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual UrbanTreasure UrbanTreasure { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
