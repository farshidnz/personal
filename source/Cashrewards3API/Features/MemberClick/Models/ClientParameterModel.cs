using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.MemberClick
{
    public class ClientParameterModel
    {
        public int ClientId { get; set; }

        public int ClientParameterId { get; set; }

        public int ClientParameterTypeId { get; set; }

        public string ClientParameterValue { get; set; }
    }
}
