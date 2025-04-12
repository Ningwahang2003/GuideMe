using GuideMe.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Web;
using GuideMe.Services;
using Microsoft.EntityFrameworkCore;

namespace GuideMe.Controllers
{
    public class GeographicController : Controller
    {
        private readonly GuideMeContext _context;
        private readonly HttpClient _httpClient;
        private readonly LocationRecommendationService _recommendationService;


        public GeographicController(GuideMeContext context, HttpClient httpClient, LocationRecommendationService recommendationService)
        {
            _context = context;
            _httpClient = httpClient;
            _recommendationService = recommendationService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Locations(string search)
        {
            int? userId = null;
            if (User.Identity.IsAuthenticated)
            {
                userId = int.Parse(User.FindFirstValue(ClaimTypes.Name));
            }

            if (string.IsNullOrWhiteSpace(search))
            {
                // Get last location for display even when no search
                if (userId.HasValue)
                {
                    ViewBag.UserLocation = await _context.Locations
                        .Where(l => l.UserId == userId)
                        .OrderByDescending(l => l.LocationCreatedAt)
                        .FirstOrDefaultAsync();
                }
                ViewBag.Recommendations = await _recommendationService.GetRecommendations(userId);
                ModelState.AddModelError("", "Please enter your location.");
                return View();
            }

            string apiKey = "AlzaSyeEdHFkMe9ez6PeMFg7ZT4OKTvc1iukwYT";
            string apiUrl = $"https://maps.gomaps.pro/maps/api/geocode/json?address={search}&key={apiKey}";

            var response = await _httpClient.GetAsync(apiUrl);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Could not fetch the entered location from API.");
                return View();
            }

            var data = await response.Content.ReadAsStringAsync();
            var jsonData = JObject.Parse(data);

            var locations = jsonData["results"]?.Select(result => new Location
            {
                LocationName = result["formatted_address"]?.ToString(),
                Latitude = result["geometry"]?["location"]?["lat"]?.ToObject<double>(),
                Longitude = result["geometry"]?["location"]?["lng"]?.ToObject<double>(),
                LocationCreatedAt = DateTime.Now,
                UserId = userId,
                SearchCount = 1
            }).ToList();

            if (locations != null && locations.Any() && User.Identity.IsAuthenticated)
            {
                foreach (var location in locations)
                {
                    var existingLocation = await _context.Locations
                        .FirstOrDefaultAsync(l =>
                            l.LocationName == location.LocationName &&
                            l.Latitude == location.Latitude &&
                            l.Longitude == location.Longitude);

                    if (existingLocation != null)
                    {
                        existingLocation.SearchCount++;
                        existingLocation.LocationCreatedAt = location.LocationCreatedAt;
                    }
                    else
                    {
                        _context.Locations.Add(location);
                    }
                }
                await _context.SaveChangesAsync();

                // Get updated last location after saving
                if (userId.HasValue)
                {
                    ViewBag.UserLocation = await _context.Locations.Where(l => l.UserId == userId).OrderByDescending(l => l.LocationCreatedAt).FirstOrDefaultAsync();
                }
            }
            else if (locations == null || !locations.Any())
            {
                ModelState.AddModelError("", "Could not find the entered location.");
            }

            // Get recommendations after updating locations
            ViewBag.Recommendations = await _recommendationService.GetRecommendations(userId);

            var distinctLocations = locations?.GroupBy(l => new { l.LocationName, l.Latitude, l.Longitude }).Select(g => g.First()).ToList();

            return View(distinctLocations);
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Weather(string location)
        {
            if (string.IsNullOrWhiteSpace(location))
            {
                TempData["Error"] = "Plz enter Location";
                return View();
            }

            string apiKey = "861c06370d01455241614fdd3796a687";
            string apiUrl = $"https://api.openweathermap.org/data/2.5/forecast?q={Uri.EscapeDataString(location)}&appid={apiKey}&units=metric";

            var response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Unable to retrieve weather data. Please check the location.";
                return View("Weather");
            }

            var responseData = await response.Content.ReadAsStringAsync();
            var weatherJson = JObject.Parse(responseData);

            var forecasts = weatherJson["list"]
                .Select(f => new WeatherForecast
                {
                    Location = weatherJson["city"]["name"]?.ToString(),
                    CreatedAt = DateTime.Parse(f["dt_txt"]?.ToString() ?? ""),
                    Temperature = f["main"]["temp"]?.ToString(),
                    Condition = f["weather"]?[0]?["description"]?.ToString()
                })
                .GroupBy(f => f.CreatedAt)
                .Select(g => g.First())
                .ToList();

            var today = DateTime.UtcNow.Date;
            var todayWeather = forecasts.FirstOrDefault(f => f.CreatedAt == today);
            if (todayWeather != null)
            {
                _context.WeatherForecasts.Add(todayWeather);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = $"Weather data for {location} retrieved successfully!";
            return View(forecasts);
        }




    }
}