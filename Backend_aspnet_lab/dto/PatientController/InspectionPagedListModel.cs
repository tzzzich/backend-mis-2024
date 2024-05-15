namespace Backend_aspnet_lab.dto.PatientController
{
    public class InspectionPagedListModel
    {
        public List<InspectionPreviewModel> Inspections { get; set; }
        public PageInfoModel Pagination { get; set; }
    }
}
