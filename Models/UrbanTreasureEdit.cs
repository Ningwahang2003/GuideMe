using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuideMe.Models
{
    public partial class UrbanTreasureEdit
    {
        public int UrbanTreasureId { get; set; }

        public string Image { get; set; } = null!;

        [Required]
        public string Title { get; set; } = null!;

        [Required]
        public string Location { get; set; } = null!;

        [Required]
        public string Description { get; set; } = null!;

        [Required]
        public int UserId { get; set; }

        [Required]
        public int ProvinceId { get; set; }


        [DataType(DataType.Upload)]
        public IFormFile[]? UrbanImage { get; set; }

        public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

        [NotMapped]
        public double AverageRating => Ratings.Any() ? Ratings.Average(r => r.RatingValue) : 0;

        [NotMapped]
        public int RatingCount => Ratings.Count;
    }
}
