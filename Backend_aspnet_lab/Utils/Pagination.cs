using Backend_aspnet_lab.dto;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Backend_aspnet_lab.Utils
{
    public static class Pagination
    {
        public static PageInfoModel MakePagination (int size, int page, int amount)
        {
            int count = (int)Math.Ceiling((double)amount / size);

            if (page > count && amount != 0)
            {
                throw new ArgumentException("Invalid arguments for filtration/pagination.");
            }
            return new PageInfoModel(size, page, count );
        }
    }
}
