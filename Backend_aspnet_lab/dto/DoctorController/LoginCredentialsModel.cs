using System.ComponentModel.DataAnnotations;

namespace Backend_aspnet_lab.dto.DoctorController
{
    public class LoginCredentialsModel
    {
        [Required]
        [EmailAddress]
        [MinLength(1)]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
