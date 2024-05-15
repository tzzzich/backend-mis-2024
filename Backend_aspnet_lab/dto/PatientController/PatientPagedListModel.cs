namespace Backend_aspnet_lab.dto.PatientController
{
    public class PatientPagedListModel
    {
        public List<PatientModel> Patients { get; set; }

        public PageInfoModel Pagination { get; set; }
    }
}
