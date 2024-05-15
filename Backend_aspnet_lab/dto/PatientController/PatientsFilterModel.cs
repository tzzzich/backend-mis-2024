namespace Backend_aspnet_lab.dto.PatientController
{
    public class PatientsFilterModel
    {
        public string? Name { get; set; }

        public string[]? Conclusions { get; set; }

        public Sorting? Sorting { get; set; }

        public bool? ScheduledVisits { get; set; }

        public bool? OnlyMine { get; set; }

        public int? Page { get; set; }

        public int? Size { get; set; }
    }
}
