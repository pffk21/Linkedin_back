using System.ComponentModel.DataAnnotations;

namespace Project_X_Data.Models.LogInOut
{
    public class RegistrationSave
    {
        public string FullName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string? WorkingPlace { get; set; }

        public string? Location { get; set; }

        public DateTime BirthDate { get; set; }

        public string ProfileImageUrl { get; set; } = null!;

        public string Password { get; set; } = null!;

        public string Code { get; set; }
        }
}
