using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.AspNetCore.Identity;

namespace Backend_aspnet_lab.Models
{
    public class Doctor : IdentityUser<Guid>
    {

        [Required]
        public DateTimeOffset CreateTime { get; set; } = DateTimeOffset.UtcNow;

        [Required]
        [MinLength(1)]
        public string Name { get; set; }

        public DateTimeOffset? Birthday { get; set; }

        [Required]
        [EnumDataType(typeof(Gender), ErrorMessage = "Недопустимое значение для пола")]
        public Gender Gender { get; set; }

        [Required]
        public Guid SpecialityId { get; set; }

        [Required]
        public Speciality Speciality { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }

        public virtual ICollection<Inspection> Inspections { get; set; } = new List<Inspection>();

    }
}
