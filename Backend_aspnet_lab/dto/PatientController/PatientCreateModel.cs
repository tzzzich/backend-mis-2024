using Backend_aspnet_lab.Models;
using Backend_aspnet_lab.Utils.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace Backend_aspnet_lab.dto.PatientController
{
    public class PatientCreateModel
    {
        [Required]
        [StringLength(1000)]
        public string Name { get; set; }

        [DatePriorValidation]
        public DateTimeOffset? Birthday { get; set; }

        [Required]
        [EnumDataType(typeof(Gender), ErrorMessage = "Недопустимое значение для пола")]
        public Gender Gender { get; set; }
    }
}
