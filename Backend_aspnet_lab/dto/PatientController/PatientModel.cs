using Backend_aspnet_lab.Models;
using System.ComponentModel.DataAnnotations;

namespace Backend_aspnet_lab.dto.PatientController
{
    public class PatientModel
    {

        public PatientModel(Patient patient)
        {
            Id = patient.Id;
            CreateTime = patient.CreateTime;
            Name = patient.Name;
            Birthday = patient.Birthday;
            Gender = patient.Gender;
        }

        [Required]
        public Guid Id { get; set; }

        [Required]
        public DateTimeOffset CreateTime { get; set; }

        [Required]
        [MinLength(1)]
        public string Name { get; set; }

        public DateTimeOffset? Birthday { get; set; }

        [Required]
        public Gender Gender { get; set; }
    }
}
