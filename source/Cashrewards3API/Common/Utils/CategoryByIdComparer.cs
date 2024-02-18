using Cashrewards3API.Features.Category;
using System.Collections.Generic;

namespace Cashrewards3API.Common.Utils
{
    public class CategoryByIdComparer : IEqualityComparer<CategoryModel>
    {
        public bool Equals(CategoryModel x, CategoryModel y)
        {
            if (x == null && y == null)
                return true;
            else if (x == null || y == null)
                return false;
            return x.CategoryId == y.CategoryId;
        }

        public int GetHashCode(CategoryModel category)
        {
            return category.CategoryId.GetHashCode();
        }
    }
}
