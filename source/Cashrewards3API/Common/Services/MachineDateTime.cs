using Cashrewards3API.Common.Services.Interfaces;
using System;
using System.Runtime.InteropServices;

namespace Cashrewards3API.Common.Services
{
    public class MachineDateTime : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;

        public DateTime Now
        {
            get
            {
                TimeZoneInfo cstZone = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ?
                                TimeZoneInfo.FindSystemTimeZoneById("Australia/Sydney") : TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time");

                return TimeZoneInfo.ConvertTimeFromUtc(UtcNow, cstZone);                
            }
        }
    }
}