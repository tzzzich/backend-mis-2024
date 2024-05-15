using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend_aspnet_lab.Models
{
    public class Inspection
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public DateTimeOffset CreateTime { get; set; } = DateTimeOffset.UtcNow;

        [Required]
        public DateTimeOffset Date { get; set; }

        [Required]
        [MinLength(1)]
        [MaxLength(5000)]
        public string Anamnesis { get; set; }

        [Required]
        [MinLength(1)]
        [MaxLength(5000)]
        public string Complaints { get; set; }

        [Required]
        [MinLength(1)]
        [MaxLength(5000)]
        public string Treatment { get; set; }

        [Required]
        [EnumDataType(typeof(Conclusion), ErrorMessage = "Недопустимое значение для заключения")]
        public Conclusion Conclusion { get; set; }

        public DateTimeOffset? NextVisitDate { get; set; }

        public DateTimeOffset? DeathDate { get; set; }

        public Guid? PreviousInspectionId { get; set; }

        public Inspection? PreviousInspection { get; set; }

        public Inspection? ChildInspection { get; set; }

        [Required]
        public virtual ICollection<Diagnosis> Diagnoses { get; set; } = new List<Diagnosis>();

        public virtual ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();

        public Guid DoctorId { get; set; }

        public Doctor Doctor { get; set; }

        public Guid PatientId { get; set; }

        public Patient Patient { get; set; }

    }
}
