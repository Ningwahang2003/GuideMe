using System;
using System.Collections.Generic;

namespace GuideMe.Models;

public partial class Location
{
    public int LocationId { get; set; }

    public string LocationName { get; set; } = null!;

    public DateOnly? LocationCreatedAt { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }
}
