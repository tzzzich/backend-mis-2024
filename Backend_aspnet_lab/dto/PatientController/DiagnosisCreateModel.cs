using Backend_aspnet_lab.Models;
using System.ComponentModel.DataAnnotations;

namespace Backend_aspnet_lab.dto.PatientController
{
    public class DiagnosisCreateModel
    {
        [Required]
        public Guid IcdDiagnosisId { get; set; }

        [StringLength(5000)]
        public string Description { get; set; }

        [Required]
        public DiagnosisType Type { get; set; }
    }
}
