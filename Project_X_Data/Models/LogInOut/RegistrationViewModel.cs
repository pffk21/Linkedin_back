using System.ComponentModel.DataAnnotations;

namespace Project_X_Data.Models.LogInOut
{
    public class RegistrationViewModel
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;

        public string? WorkingPlace { get; set; }

        public string? Location { get; set; }

        public DateTime BirthDate { get; set; }

        public IFormFile ProfileImageUrl { get; set; } = null!;

        public string Password { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;
    }
}
