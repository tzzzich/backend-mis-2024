using Backend_aspnet_lab.Models;
using Backend_aspnet_lab.Utils.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace Backend_aspnet_lab.dto.DoctorController
{
    public class DoctorEditModel
    {
        [Required]
        [EmailAddress]
        [MinLength(1)]
        public string Email { get; set; }

        [Required]
        [MinLength(1)]
        [MaxLength(1000)]
        public string Name { get; set; }

        [DatePriorValidation]

        public DateTimeOffset? Birthday { get; set; }

        [Required]
        public Gender Gender { get; set; }

        [Phone]
        public string? Phone { get; set; }
    }
}
