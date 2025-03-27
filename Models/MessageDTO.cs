namespace GuideMe.Models
{
    public class MessageDTO
    {
        public string MessageText { get; set; } // The message content
        public IFormFile? Attachment { get; set; } // Optional attachment file
    }
}
