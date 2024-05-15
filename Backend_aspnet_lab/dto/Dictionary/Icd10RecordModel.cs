using Backend_aspnet_lab.Models;
using System.ComponentModel.DataAnnotations;

namespace Backend_aspnet_lab.dto.Dictionary
{
    public class Icd10RecordModel
    {
        public Icd10RecordModel(Icd10Record icd)
        {
            Id = icd.Id;
            Name = icd.Name;
            Code= icd.Code;
            CreateTime = icd.CreateTime;
        }


        [Required]
        public Guid Id { get; set; }

        [Required]
        public DateTimeOffset CreateTime { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }
    }
}
