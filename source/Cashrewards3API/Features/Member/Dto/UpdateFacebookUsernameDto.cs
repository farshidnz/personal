using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Member.Dto
{
    public class UpdateFacebookUsernameDto
    {
        public UpdateFacebookUsernameDto(bool status)
        {
            Status = status;
        }
        public bool Status { get; set; }
    }
}
