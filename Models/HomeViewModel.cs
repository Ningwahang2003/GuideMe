namespace GuideMe.Models
{
    public class HomeViewModel
    {
        public HomeViewModel()
        {
            UpcomingEvents = new List<Event>();
            UpcomingContests = new List<WeeklyContest>();
            ContestEntries = new List<ContestEntry>();
            UserPosts = new List<UserPost>();
            Winner = null;
        }

        public List<Event> UpcomingEvents { get; set; }
        public List<WeeklyContest> UpcomingContests { get; set; }
        public User? Winner { get; set; }
        public List<ContestEntry> ContestEntries { get; set; }
        public List<UserPost> UserPosts { get; set; }
    }
}