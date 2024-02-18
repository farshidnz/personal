using Cashrewards3API.Features.Banners.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Banners.Interface
{
    public interface IBanner
    {
        Task<IEnumerable<Banner>> GetBannersFromClientId(int clientId);
    }
}
