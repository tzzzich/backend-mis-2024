using Backend_aspnet_lab.dto.DoctorController;
using Backend_aspnet_lab.Models;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Backend_aspnet_lab.dto.InspectionController
{
    public class InspectionCommentModel
    {
        public InspectionCommentModel(Comment comment)
        {
            Id = comment.Id;
            CreateTime = comment.CreateTime;
            ParentId = comment.ParentId;
            Content= comment.Content;
            Author = new DoctorModel(comment.Author);
            ModifyTime= comment.ModifyTime;
        }


        [Required]
        public Guid Id { get; set; }

        [Required]
        public DateTimeOffset CreateTime { get; set; }

        public Guid? ParentId { get; set; }

        [StringLength(1000, ErrorMessage = "Комментарий должен содержать не более 1000 символов.")]
        public string Content { get; set; }

        [Required]
        public DoctorModel Author { get; set; }

        public DateTimeOffset? ModifyTime { get; set; }
    }
}
