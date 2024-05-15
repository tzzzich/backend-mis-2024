using Backend_aspnet_lab.Models;

namespace Backend_aspnet_lab.dto.Dictionary
{
    public class IcdRootsReportRecordModel
    {
        public string PatientName { get; set; }

        public DateTimeOffset? PatientBirthdate { get; set; }

        public Gender Gender { get; set; }

        public Dictionary<string, int> VisitsByRoot { get; set; }
    }
}
