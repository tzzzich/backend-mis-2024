using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend_aspnet_lab.Models
{
    public class Comment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [MinLength(1)]
        [MaxLength(1000)]
        public string Content { get; set; }

        [Required]
        public DateTimeOffset CreateTime { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset? ModifyTime { get; set; }

        [Required]
        public Guid AuthorId { get; set; }

        [Required]
        public Doctor Author { get; set; }
        
        public Comment? Parent { get; set; }

        public Guid? ParentId { get; set; }

        public virtual ICollection<Comment> Children { get; set; } = new List<Comment>();

        [Required]
        public Guid ConsultationId { get; set; }

        [Required]
        public Consultation Consultation { get; set; }
    }
}
