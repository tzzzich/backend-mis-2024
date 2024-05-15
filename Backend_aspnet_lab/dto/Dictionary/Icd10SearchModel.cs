namespace Backend_aspnet_lab.dto.Dictionary
{
    public class Icd10SearchModel
    {
        public List<Icd10RecordModel> Records { get; set; } = new List<Icd10RecordModel>();

        public PageInfoModel Pagination { get; set; }
    }
}
