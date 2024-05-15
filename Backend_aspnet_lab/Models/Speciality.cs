using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend_aspnet_lab.Models
{
    public class Speciality
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public DateTimeOffset CreateTime { get; set; }

        [Required]
        [MinLength(1)]
        [MaxLength(1000)]
        public string Name { get; set; }

        public virtual ICollection<Consultation> Consultations { get; set; } = new List<Consultation>(); 

        public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    }
}
