using Backend_aspnet_lab.dto.Dictionary;
using Backend_aspnet_lab.Models;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Backend_aspnet_lab.dto.InspectionController
{
    public class InspectionConsultationModel
    {
        public InspectionConsultationModel(Consultation consultation) 
        { 
            Id= consultation.Id;
            CreateTime = consultation.CreateTime;
            InspectionId= consultation.InspectionId;
            Speciality = new SpecialityModel(consultation.Speciality);
            RootComment = new InspectionCommentModel(consultation.Comments.First(c => c.ParentId == null));
            CommentsNumber = consultation.Comments.Count;
        }

        public Guid Id { get; set; }

        public DateTimeOffset CreateTime { get; set; }

        public Guid InspectionId { get; set; }

        public SpecialityModel Speciality { get; set; }

        [Required(ErrorMessage = "Поле 'rootComment' является обязательным.")]
        public InspectionCommentModel RootComment { get; set; }

        [Required(ErrorMessage = "Поле 'commentsNumber' является обязательным.")]
        [Range(0, int.MaxValue, ErrorMessage = "Количество комментариев должно быть положительным.")]
        public int CommentsNumber { get; set; }
    }
}
