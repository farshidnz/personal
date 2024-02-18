using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Common.Dto
{
    public interface IPaginationModel<T>
    {
        int TotalCount { get; set; }
        
        int? Count { get; set; }
        
        IList<T> Data { get; set; }
    }

    public class PagedList<T> : IPaginationModel<T>
    {   
        public PagedList(int totalCount, int count, IList<T> list)
        {
            TotalCount = totalCount;
            Count = count;
            Data = list;
        }

        public int TotalCount { get; set; }
        public int? Count { get; set; }

        public IList<T> Data { get; set; }
    }
}
