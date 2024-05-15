using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend_aspnet_lab.Models
{
    public class Diagnosis
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public DateTimeOffset CreateTime { get; set; } = DateTimeOffset.UtcNow;

        [Required]
        public Guid Icd10RecordId { get; set; }

        [Required]
        public Icd10Record Icd10Record { get; set; }

        [StringLength(5000)]
        public string? Description { get; set; }

        [Required]
        [EnumDataType(typeof(DiagnosisType), ErrorMessage = "Недопустимое значение для типа диагноза")]
        public DiagnosisType Type { get; set; }

        public virtual ICollection<Inspection> Inspections { get; set; } = new List<Inspection>();
    }
}
