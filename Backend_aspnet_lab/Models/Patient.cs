using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend_aspnet_lab.Models
{
    public class Patient
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public DateTimeOffset CreateTime { get; set; } = DateTimeOffset.UtcNow;

        [Required]
        [MinLength(1)]
        public string Name { get; set; }

        public DateTimeOffset? Birthday { get; set; }

        [Required]
        [EnumDataType(typeof(Gender), ErrorMessage = "Недопустимое значение для пола")]
        public Gender Gender { get; set; }
    }
}
