using Backend_aspnet_lab.Models;
using System.ComponentModel.DataAnnotations;

namespace Backend_aspnet_lab.dto.ConsultationController
{
    public class CommentModel
    {
        public CommentModel(Comment comment)
        {
            Id = comment.Id;
            CreateTime= comment.CreateTime;
            ModifiedDate = comment.ModifyTime;
            Content= comment.Content;
            AuthorId= comment.AuthorId;
            Author = comment.Author.Name;
            ParentId= comment.ParentId;
        }

        [Required]
        public Guid Id { get; set; }

        [Required]
        public DateTimeOffset CreateTime { get; set; }

        public DateTimeOffset? ModifiedDate { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 1)]
        public string Content { get; set; }

        [Required]
        public Guid AuthorId { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 1)]
        public string Author { get; set; }

        public Guid? ParentId { get; set; }
    }
}
