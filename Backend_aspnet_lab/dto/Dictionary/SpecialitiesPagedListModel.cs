namespace Backend_aspnet_lab.dto.Dictionary
{
    public class SpecialitiesPagedListModel
    {
        public List<SpecialityModel> Specialties { get; set; }

        public PageInfoModel Pagination { get; set; }
    }
}
