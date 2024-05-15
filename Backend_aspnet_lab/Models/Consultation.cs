using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend_aspnet_lab.Models
{
    public class Consultation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public DateTimeOffset CreateTime { get; set; } = DateTimeOffset.UtcNow;

        [Required]
        public Guid InspectionId { get; set; }

        [Required]
        public Inspection Inspection { get; set; }

        [Required]
        public Guid SpecialityId { get; set; }

        [Required]
        public Speciality Speciality { get; set; }

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
