using Backend_aspnet_lab.dto.DoctorController;
using Backend_aspnet_lab.dto.PatientController;
using Backend_aspnet_lab.Models;
using System.ComponentModel.DataAnnotations;

namespace Backend_aspnet_lab.dto.InspectionController
{
    public class InspectionModel
    {
        public InspectionModel(Inspection inspection)
        {
            Id = inspection.Id;
            CreateTime= inspection.CreateTime;
            Date = inspection.Date;
            Anamnesis = inspection.Anamnesis;
            Complaints= inspection.Complaints;
            Treatment= inspection.Treatment;
            Conclusion= inspection.Conclusion;
            NextVisitDate= inspection.NextVisitDate;
            DeathDate= inspection.DeathDate;
            //BaseInspectionId = ;
            PreviousInspectionId= inspection.PreviousInspectionId;
            Patient = new PatientModel (inspection.Patient);
            Doctor = new DoctorModel(inspection.Doctor);
            Diagnoses = inspection.Diagnoses.Select(diagnosis => new DiagnosisModel(diagnosis)).ToList(); 
            Consultations = inspection.Consultations.Select(consultation => new InspectionConsultationModel(consultation)).ToList();

        }


        [Required(ErrorMessage = "Поле 'id' является обязательным.")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Поле 'createTime' является обязательным.")]
        public DateTimeOffset CreateTime { get; set; }

        [Required(ErrorMessage = "Поле 'date' является обязательным.")]
        public DateTimeOffset Date { get; set; }

        public string Anamnesis { get; set; }

        public string Complaints { get; set; }

        public string Treatment { get; set; }

        public Conclusion Conclusion { get; set; }

        public DateTimeOffset? NextVisitDate { get; set; }

        public DateTimeOffset? DeathDate { get; set; }

        public Guid? BaseInspectionId { get; set; }

        public Guid? PreviousInspectionId { get; set; }

        [Required(ErrorMessage = "Поле 'patient' является обязательным.")]
        public PatientModel Patient { get; set; }

        [Required(ErrorMessage = "Поле 'doctor' является обязательным.")]
        public DoctorModel Doctor { get; set; }

        public List<DiagnosisModel> Diagnoses { get; set; }

        public List<InspectionConsultationModel> Consultations { get; set; }
    }
}
