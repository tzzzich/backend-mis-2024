using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend_aspnet_lab.Models
{
    public class Icd10Record
    {
        [Key]
		public Guid Id { get; set; }

        [Required]
        public DateTimeOffset CreateTime { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }

		public Guid? PreviousId { get; set; }

        public Icd10Record? Previous { get; set; }

		public Guid? RootId { get; set; }


	}
}
