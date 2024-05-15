namespace Backend_aspnet_lab.dto
{
    public class PageInfoModel
    {
        public PageInfoModel(int size, int current, int count)
        {
            Size = size;
            Current = current;
            Count = count;
        }
        public int Size { get; set; }
        public int Count { get; set; }
        public int Current { get; set; }
    }
}
