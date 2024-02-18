using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Banners.Model
{
    public class BannerDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string MobileLink { get; set; }
        public string MobileImageUrl { get; set; }
        public string DesktopLink { get; set; }
        public string DesktopImageUrl { get; set; }

        public string MobileBrowserLink { get; set; }
        public string MobileBrowserImageUrl { get; set; }
    }
}
