using System.Collections.Generic;
using System.Linq;

namespace MyCNBlog.Core.Abstractions
{
    public class PaginationList<T> : List<T>
    {
        public int PageIndex { get; }
        public int PageSize { get; }
        public int TotalItemsCount { get; }
        public int PageCount => (int)(TotalItemsCount / PageSize - 0.5) + 1;

        public PaginationList(int pageIndex, int pageSize, int totalItemsCount, IEnumerable<T> data)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalItemsCount = totalItemsCount;
            AddRange(data);
        }

        public PaginationList(
            int pageIndex, 
            int pageSize, 
            int totalItemsCount, 
            params T[] data):this(pageIndex, pageSize, totalItemsCount, data.AsEnumerable())
        {
        }
    }
}
