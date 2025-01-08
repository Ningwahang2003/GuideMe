using System.ComponentModel.DataAnnotations;

namespace GuideMe.Models
{
   public class ChangePassword
   {
      [Required(ErrorMessage = "Plz Current Password")]
      [DataType(DataType.Password)]
      public string CurrentPassword { get; set; } = null!;

      [Required(ErrorMessage = "Enter New Password")]
      [DataType(DataType.Password)]
      public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "Plz Enter Same Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = null!;


      public string? EmailToken { get; set; }
        }
    }

