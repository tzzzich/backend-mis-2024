using Backend_aspnet_lab.Models;
using System.ComponentModel.DataAnnotations;

namespace Backend_aspnet_lab.dto.PatientController
{
    public class InspectionPreviewModel
    {

        public InspectionPreviewModel(Inspection inspection) 
        {
            Id = inspection.Id;
            CreateTime = inspection.CreateTime;
            PreviousId = inspection.PreviousInspectionId;
            Date = inspection.Date;
            Conclusion= inspection.Conclusion;
            DoctorId= inspection.DoctorId;
            Doctor = inspection.Doctor.Name;
            PatientId = inspection.PatientId;
            Patient = inspection.Patient.Name;
            Diagnosis = new DiagnosisModel(inspection.Diagnoses.Where(d => d.Type == DiagnosisType.Main).First());
            HasChain = inspection.PatientId == null && inspection.ChildInspection != null ? true : false;
            HasNested = inspection.ChildInspection != null ? true : false;
        }


        [Required(ErrorMessage = "Поле 'id' является обязательным.")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Поле 'createTime' является обязательным.")]
        public DateTimeOffset CreateTime { get; set; }

        public Guid? PreviousId { get; set; }

        [Required(ErrorMessage = "Поле 'date' является обязательным.")]
        public DateTimeOffset Date { get; set; }

        [Required(ErrorMessage = "Поле 'conclusion' является обязательным.")]
        public Conclusion Conclusion { get; set; }

        [Required(ErrorMessage = "Поле 'doctorId' является обязательным.")]
        public Guid DoctorId { get; set; }

        [Required(ErrorMessage = "Поле 'doctor' является обязательным.")]
        [MinLength(1, ErrorMessage = "Поле 'doctor' должно содержать минимум 1 символ.")]
        public string Doctor { get; set; }

        [Required(ErrorMessage = "Поле 'patientId' является обязательным.")]
        public Guid PatientId { get; set; }

        [Required(ErrorMessage = "Поле 'patient' является обязательным.")]
        [MinLength(1, ErrorMessage = "Поле 'patient' должно содержать минимум 1 символ.")]
        public string Patient { get; set; }

        [Required(ErrorMessage = "Поле 'diagnosis' является обязательным.")]
        public DiagnosisModel Diagnosis { get; set; }

        public bool HasChain { get; set; }

        public bool HasNested { get; set; }
    }
}
