using Backend_aspnet_lab.Models;
using Backend_aspnet_lab.Utils.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace Backend_aspnet_lab.dto.PatientController
{
    public class InspectionCreateModel
    {
        [Required]
        public DateTimeOffset Date { get; set; } = DateTimeOffset.UtcNow;

        [MaxLength(5000)]
        [MinLength(1)]
        [Required]
        public string Anamnesis { get; set; }

        [MaxLength(5000)]
        [MinLength(1)]
        [Required]
        public string Complaints { get; set; }

        [MaxLength(5000)]
        [MinLength(1)]
        [Required]
        public string Treatment { get; set; }

        [Required]
        public Conclusion Conclusion { get; set; }

        [DateLaterValidation(ErrorMessage = "Дата следующего визита не может быть раньше текущего времени.")]
        public DateTimeOffset? NextVisitDate { get; set; }


        [DatePriorValidation(ErrorMessage = "Дата смерти не может быть позже текущего времени")]
        public DateTimeOffset? DeathDate { get; set; }

        public Guid? PreviousInspectionId { get; set; }

        public List<DiagnosisCreateModel> diagnoses { get; set; } = new List<DiagnosisCreateModel>();

        public List<ConsultationCreateModel> consultations { get; set; } = new List<ConsultationCreateModel>();
    }
}
