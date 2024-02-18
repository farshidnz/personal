using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Common.Services.Interfaces
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }

        DateTime Now { get; }
    }
}
