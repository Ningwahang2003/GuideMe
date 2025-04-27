using GuideMe.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace GuideMe.Services
{
    public class LocationRecommendationService
    {
        private readonly GuideMeContext _context;
        private const int K = 3;

        public LocationRecommendationService(GuideMeContext context)
        {
            _context = context;
        }

        public async Task<List<Location>> GetRecommendations(int? userId)
        {
            if (!userId.HasValue)
            {
                return await _context.Locations
                    .OrderByDescending(l => l.SearchCount)
                    .Take(K)
                    .ToListAsync();
            }

            var userLastLocation = await _context.Locations
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.LocationCreatedAt)
                .FirstOrDefaultAsync();

            if (userLastLocation == null)
            {
                return await _context.Locations
                    .OrderByDescending(l => l.SearchCount)
                    .Take(K)
                    .ToListAsync();
            }

            // Finding nearest locations based on geographical distance
            var allLocations = await _context.Locations.ToListAsync();
            var nearestLocations = allLocations
                .Where(l => l.LocationId != userLastLocation.LocationId)
                .Select(l => new
                {
                    Location = l,
                    Distance = CalculateDistance(
                        userLastLocation.Latitude ?? 0,
                        userLastLocation.Longitude ?? 0,
                        l.Latitude ?? 0,
                        l.Longitude ?? 0)
                })
                .OrderBy(x => x.Distance)
                .Take(K)
                .Select(x => x.Location)
                .ToList();

            return nearestLocations;
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            //Euclidean distance calculation
            var latDiff = lat1 - lat2;
            var lonDiff = lon1 - lon2;
            return Math.Sqrt(latDiff * latDiff + lonDiff * lonDiff);
        }
    }
}