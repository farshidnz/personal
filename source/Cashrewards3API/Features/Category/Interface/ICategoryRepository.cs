using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cashrewards3API.Enum;

namespace Cashrewards3API.Features.Category.Interface
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<CategoryModel>>GetCategoriesByClientIdAndStatusAsync(List<int> clientIds, Status status);
        Task<IEnumerable<CategoryModel>> GetCategoriesByClientIdAsync(List<int> clientIds);
        Task<IEnumerable<CategoryModel>> GetRootCategoriesByClientIdAndStatusWithCountsAsync(List<int> clientIds, Status status);
        Task<IEnumerable<CategoryModel>> GetNonRootCategoriesAsync(Status status, CategoryTypeEnum categoryType);
        Task<IEnumerable<CategoryModel>> GetCategoriesByMerchantIdAsync(int merchantId);
    }
}
