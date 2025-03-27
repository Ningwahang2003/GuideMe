namespace GuideMe.Models
{
    public class UserViewModel
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string UserImage { get; set; }
        public List<ChatMessage> Messages { get; set; }
    }
}
