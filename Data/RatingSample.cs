/*using GuideMe.Models;
using Microsoft.EntityFrameworkCore;

namespace GuideMe.Data
{
    public static class RatingSample
    {
        public static void RatingData(GuideMeContext context)
        {
            if (!context.Ratings.Any())
            {
                var random = new Random();
                var users = context.Users.Select(u => u.UserId).ToList();
                var treasures = context.UrbanTreasures.Select(ut => ut.UrbanTreasureId).ToList();

                var ratings = new List<Rating>();
                var ratedCombinations = new Dictionary<int, HashSet<int>>(); // Track user's rated treasures

                foreach (var userId in users)
                {
                    ratedCombinations[userId] = new HashSet<int>();
                    int numberOfRatings = random.Next(0, treasures.Count + 1);

                    for (int i = 0; i < numberOfRatings && treasures.Any(); i++)
                    {
                        var unratedTreasures = treasures.Where(t => !ratedCombinations[userId].Contains(t)).ToList();
                        if (!unratedTreasures.Any()) break;

                        var treasureId = unratedTreasures[random.Next(unratedTreasures.Count)];
                        ratedCombinations[userId].Add(treasureId);

                        ratings.Add(new Rating
                        {
                            UserId = userId,
                            UrbanTreasureId = treasureId,
                            RatingValue = random.Next(1, 6),
                            CreatedAt = DateTime.Now.AddDays(-random.Next(365))
                        });
                    }
                }

                context.Ratings.AddRange(ratings);
                context.SaveChanges();
            }
        }

        public static void ExportRatingsToCSV(GuideMeContext context, string filePath = "c:\\Users\\acer\\source\\repos\\GuideMe\\Data\\ratings_data.csv")
        {
            var ratings = context.Ratings
                .Include(r => r.User)
                .Include(r => r.UrbanTreasure)
                .OrderBy(r => r.RatingId)
                .ToList();

            using (var writer = new StreamWriter(filePath))
            {
                writer.WriteLine("RatingId,UserId,UrbanTreasureId,RatingValue,CreatedAt");

                foreach (var rating in ratings)
                {
                    writer.WriteLine($"{rating.RatingId},{rating.UserId},{rating.UrbanTreasureId},{rating.RatingValue},{rating.CreatedAt}");
                }
            }
        }
    }
}*/