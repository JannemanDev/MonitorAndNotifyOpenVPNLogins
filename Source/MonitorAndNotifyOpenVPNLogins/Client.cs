using MonitorAndNotifyOpenVPNLogins.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MonitorAndNotifyOpenVPNLogins
{
    internal class Client : IEquatable<Client>
    {
        public string CommonName { get; set; }
        public string RealAddress { get; set; }
        public string VirtualAddress { get; set; }
        public long BytesReceived { get; set; }
        public long BytesSent { get; set; }
        public DateTime ConnectedSince { get; set; }
        public string Username { get; set; }

        public string Ip { get {
                string regEx = @"(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})";
                Match match = Regex.Match(RealAddress, regEx, RegexOptions.IgnoreCase);

                return match.Groups[1].Value; 
            } 
        }

        [JsonConstructor]
        public Client(string commonName, string realAddress, string virtualAddress, long bytesReceived, long bytesSent, DateTime connectedSince, string username)
        {
            CommonName = commonName;
            RealAddress = realAddress;
            VirtualAddress = virtualAddress;
            BytesReceived = bytesReceived;
            BytesSent = bytesSent;
            ConnectedSince = connectedSince;
            Username = username;
        }

        public Client(string commonName, string realAddress, string virtualAddress, string bytesReceived, string bytesSent, string connectedSince, string username)
        {
            CommonName = commonName;
            RealAddress = realAddress;
            VirtualAddress = virtualAddress;
            BytesReceived = Convert.ToInt64(bytesReceived);
            BytesSent = Convert.ToInt64(bytesSent);
            ConnectedSince = Convert.ToDouble(connectedSince).UnixTimeStampInSecondsToDateTime();
            Username = username;
        }

        bool IEquatable<Client>.Equals(Client? other)
        {
            return Ip.Equals(other?.Ip);
        }

        public override int GetHashCode()
        {
            return Ip.GetHashCode();
        }
    }
}
