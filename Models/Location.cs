using System;
using System.Collections.Generic;

namespace GuideMe.Models;

public partial class Location
{
    public int LocationId { get; set; }

    public string LocationName { get; set; } = null!;

    public DateTime? LocationCreatedAt { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public int? UserId { get; set; }

    public int? SearchCount { get; set; }

    public virtual User? User { get; set; }

    public double GetDistanceTo(Location other)
    {
        // Return max value if coordinates are missing
        if (!Latitude.HasValue || !Longitude.HasValue ||
            !other.Latitude.HasValue || !other.Longitude.HasValue)
            return double.MaxValue;

        // Simple distance calculation using Pythagorean theorem
        // Note: This is less accurate but easier to understand
        double latDiff = (other.Latitude.Value - Latitude.Value) * 111; // 1 degree ≈ 111 km
        double lonDiff = (other.Longitude.Value - Longitude.Value) * 111 *
                         Math.Cos(Latitude.Value * Math.PI / 180); // Adjust for Earth's curvature

        // Calculate straight-line distance
        return Math.Sqrt(latDiff * latDiff + lonDiff * lonDiff);
    }
}
