using Backend_aspnet_lab.dto.PatientController;
using Backend_aspnet_lab.Models;
using System.ComponentModel.DataAnnotations;

namespace Backend_aspnet_lab.dto.InspectionController
{
    public class InspectionEditModel
    {
        [Required(ErrorMessage = "Поле Complaints является обязательным.")]
        [StringLength(5000, MinimumLength = 1, ErrorMessage = "Поле Anamnesis должно содержать от 1 до 5000 символов.")]
        public string Anamnesis { get; set; }

        [Required(ErrorMessage = "Поле Complaints является обязательным.")]
        [StringLength(5000, MinimumLength = 1, ErrorMessage = "Поле Complaints должно содержать от 1 до 5000 символов.")]
        public string Complaints { get; set; }

        [Required(ErrorMessage = "Поле Treatment является обязательным.")]
        [StringLength(5000, MinimumLength = 1, ErrorMessage = "Поле Treatment должно содержать от 1 до 5000 символов.")]
        public string Treatment { get; set; }

        [Required(ErrorMessage = "Поле Conclusion является обязательным.")]
        public Conclusion Conclusion { get; set; }

        public DateTimeOffset? NextVisitDate { get; set; }

        public DateTimeOffset? DeathDate { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Требуется хотя бы один диагноз.")]
        public List<DiagnosisCreateModel> Diagnoses { get; set; }
    }
}
