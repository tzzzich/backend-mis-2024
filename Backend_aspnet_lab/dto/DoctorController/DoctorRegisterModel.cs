using Backend_aspnet_lab.Models;
using Backend_aspnet_lab.Utils.ValidationAttributes;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend_aspnet_lab.dto.DoctorController
{
    public class DoctorRegisterModel
    {
        [Required]
        [StringLength(1000, MinimumLength = 1)]
        public string Name { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        [EmailAddress]
        [MinLength(1)]
        public string Email { get; set; }

        [DatePriorValidation]
        public DateTimeOffset? Birthday { get; set; }

        [Required]
        [EnumDataType(typeof(Gender), ErrorMessage = "Недопустимое значение для пола")]
        public Gender Gender { get; set; }

        [Phone]
        public string? Phone { get; set; }

        [Required]
        public Guid Speciality { get; set; }
    }
}
