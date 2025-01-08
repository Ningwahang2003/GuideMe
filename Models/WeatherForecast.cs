using System;
using System.Collections.Generic;

namespace GuideMe.Models;

public partial class WeatherForecast
{
    public int WeatherId { get; set; }

    public string Location { get; set; } = null!;

    public string Temperature { get; set; } = null!;

    public string Condition { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }
}
