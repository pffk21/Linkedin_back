using System.ComponentModel.DataAnnotations;

namespace Project_X_Data.Models.LogInOut
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Email or phone number")]
        public string Email { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
    }
}
