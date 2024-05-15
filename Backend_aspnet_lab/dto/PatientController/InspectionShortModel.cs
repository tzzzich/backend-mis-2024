using Backend_aspnet_lab.Models;
using System.ComponentModel.DataAnnotations;

namespace Backend_aspnet_lab.dto.PatientController
{
    public class InspectionShortModel
    {
        public InspectionShortModel(Inspection inspection) 
        {
            Id = inspection.Id;
            CreateTime = inspection.CreateTime;
            Date = inspection.Date;
            Diagnosis = new DiagnosisModel (inspection.Diagnoses.First(d => d.Type == DiagnosisType.Main));
        }


        [Required(ErrorMessage = "Поле 'id' является обязательным.")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Поле 'createTime' является обязательным.")]
        public DateTimeOffset CreateTime { get; set; }

        [Required(ErrorMessage = "Поле 'date' является обязательным.")]
        public DateTimeOffset Date { get; set; }

        [Required(ErrorMessage = "Поле 'diagnosis' является обязательным.")]
        public DiagnosisModel Diagnosis { get; set; }
    }
}
