using Backend_aspnet_lab.Models;
using System.ComponentModel.DataAnnotations;

namespace Backend_aspnet_lab.dto.Dictionary
{
    public class SpecialityModel
    {
        public SpecialityModel(Speciality speciality)
        {
            Id = speciality.Id;
            CreateTime = speciality.CreateTime;
            Name = speciality.Name;
        }

        [Required]
        public Guid Id { get; set; }

        [Required]
        public DateTimeOffset CreateTime { get; set; }

        [Required]
        [StringLength(1, MinimumLength = 1)]
        public string Name { get; set; }
    }
}
