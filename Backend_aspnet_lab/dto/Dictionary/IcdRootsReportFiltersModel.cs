namespace Backend_aspnet_lab.dto.Dictionary
{
    public class IcdRootsReportFiltersModel
    {
        public DateTimeOffset Start { get; set; }

        public DateTimeOffset End { get; set; }

        public List<string> IcdRoots { get; set; }
    }
}
