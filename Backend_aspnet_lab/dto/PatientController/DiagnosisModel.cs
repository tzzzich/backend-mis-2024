using Backend_aspnet_lab.Models;
using System.ComponentModel.DataAnnotations;

namespace Backend_aspnet_lab.dto.PatientController
{
    public class DiagnosisModel
    {

        public DiagnosisModel(Diagnosis diagnosis)
        {
            Id= diagnosis.Id;
            CreateTime= diagnosis.CreateTime;
            Code = diagnosis.Icd10Record.Code;
            Name = diagnosis.Icd10Record.Name;
            Description = diagnosis.Description;
            Type = diagnosis.Type;

        }

        [Required]
        public Guid Id { get; set; }

        [Required]
        public DateTimeOffset CreateTime { get; set; }

        [Required]
        [MinLength(1)]
        public string Code { get; set; }

        [Required]
        [MinLength(1)]
        public string Name { get; set; }

        [StringLength(5000)]
        public string Description { get; set; }

        [Required]
        public DiagnosisType Type { get; set; }
    }
}
