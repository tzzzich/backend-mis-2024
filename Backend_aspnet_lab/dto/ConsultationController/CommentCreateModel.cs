using System.ComponentModel.DataAnnotations;

namespace Backend_aspnet_lab.dto.ConsultationController
{
    public class CommentCreateModel
    {

        [Required]
        [StringLength(1000, MinimumLength = 1)]
        public string Content { get; set; }

        [Required]
        public Guid ParentId { get; set; }
    }
}
