using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Common.Model
{
    public interface IHavePremiumDisabled
    {
        public bool? IsPremiumDisabled { get; set; }
    }
}
