using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonitorAndNotifyOpenVPNLogins.Extensions;

namespace MonitorAndNotifyOpenVPNLogins
{
    internal class RoutingTableEntry
    {
        public string VirtualAddress { get; set; }
        public string CommonName { get; set; }
        public string RealAddress { get; set; }
        //public string RealIp => RealAddress
        public DateTime LastRef { get; set; }

        public RoutingTableEntry(string virtualAddress, string commonName, string realAddress, string lastRef)
            : this(virtualAddress, commonName, realAddress, Convert.ToDouble(lastRef).UnixTimeStampInSecondsToDateTime())
        {

        }

        [JsonConstructor]
        public RoutingTableEntry(string virtualAddress, string commonName, string realAddress, DateTime lastRef)
        {
            VirtualAddress = virtualAddress;
            CommonName = commonName;
            RealAddress = realAddress;
            LastRef = lastRef;
        }
    }
}
