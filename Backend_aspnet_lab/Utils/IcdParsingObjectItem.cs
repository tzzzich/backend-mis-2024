namespace Backend_aspnet_lab.Utils
{
    public class Icd10ParsingObjectItem
    {
        public string? Id { get; set; }
        public bool? Actual { get; set; }
        public string? Mkb_code { get; set; }
        public string? Mkb_name { get; set; }
        public string? Rec_code { get; set; }
        public string? Id_parent { get; set; }

        public Guid InnerId { get; set; } 

        public Guid? ParentId { get; set; }

        public Guid? RootId { get; set; }

    }
}
