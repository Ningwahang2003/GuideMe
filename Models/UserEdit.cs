using System.ComponentModel.DataAnnotations;

namespace GuideMe.Models
{
    public partial class UserEdit
    {
        public int UserId { get; set; }

        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        [Required]
        public string Name { get; set; } = null!;

        public string? Role { get; set; }

        public DateTime? LastLogin { get; set; }

        public string? UserImage { get; set; }

        [DataType(DataType.Upload)]
        public IFormFile? UserFile { get; set; }

        public String? EmailToken { get; set; }
    }
}
