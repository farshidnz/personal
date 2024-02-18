using System;
using System.Collections.Generic;

namespace Cashrewards3API.Features.Banners.Model
{
    public class Banner
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string DesktopHtml { get; set; }
        public string MobileHtml { get; set; }
        public string DesktopLink { get; set; }
        public string MobileLink { get; set; }
        public string DesktopImageUrl { get; set; }
        public string MobileImageUrl { get; set; }
        public int Position { get; set; }
        public int ClientId { get; set; }
        public string MobileAppImageUrl { get; set; }
        public string MobileAppLink { get; set; }

        public List<int> Clients { get; set; } = new List<int>();
    }
}