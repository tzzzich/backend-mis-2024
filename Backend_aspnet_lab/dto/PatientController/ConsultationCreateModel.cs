using System.ComponentModel.DataAnnotations;

namespace Backend_aspnet_lab.dto.PatientController
{
    public class ConsultationCreateModel
    {
        [Required]
        public Guid SpecialityId { get; set; }

        [Required]
        public InspectionCommentCreateModel Comment { get; set; }
    }
}
