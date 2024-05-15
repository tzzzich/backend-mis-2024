using Backend_aspnet_lab.dto.Dictionary;
using Backend_aspnet_lab.Models;
using System.ComponentModel.DataAnnotations;

namespace Backend_aspnet_lab.dto.ConsultationController
{
    public class ConsultationModel
    {
        public ConsultationModel(Consultation consultation)
        {
            Id = consultation.Id;
            CreateTime= consultation.CreateTime;
            InspectionId= consultation.InspectionId;
            Speciality = new SpecialityModel(consultation.Speciality);
            Comments = consultation.Comments.Select(c => new CommentModel(c)).ToList();
        }


        [Required]
        public Guid Id { get; set; }

        [Required]
        public DateTimeOffset CreateTime { get; set; }

        public Guid InspectionId { get; set; }

        [Required]
        public SpecialityModel Speciality { get; set; }

        public List<CommentModel> Comments { get; set; }
    }
}
