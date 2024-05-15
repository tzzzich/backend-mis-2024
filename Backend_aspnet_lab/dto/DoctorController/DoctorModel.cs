using Backend_aspnet_lab.Models;
using System.ComponentModel.DataAnnotations;

namespace Backend_aspnet_lab.dto.DoctorController
{
    public class DoctorModel
    {
        public DoctorModel(Doctor user)
        {
            Id = user.Id;
            CreateTime = user.CreateTime;
            Name = user.Name;
            Birthday = user.Birthday;
            Gender = user.Gender;
            Email = user.Email;
            Phone = user.PhoneNumber;
        }

        [Required]
        public Guid Id { get; set; }

        [Required]
        public DateTimeOffset CreateTime { get; set; }

        [Required]
        [MinLength(1)]
        public string Name { get; set; }

        public DateTimeOffset? Birthday { get; set; }

        [Required]
        [EnumDataType(typeof(Gender), ErrorMessage = "Недопустимое значение для пола")]
        public Gender Gender { get; set; }

        [Required]
        [EmailAddress]
        [MinLength(1)]
        public string Email { get; set; }

        [Phone]
        public string Phone { get; set; }
    }
}
