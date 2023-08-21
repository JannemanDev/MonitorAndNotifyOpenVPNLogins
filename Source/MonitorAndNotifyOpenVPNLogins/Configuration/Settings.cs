using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace MonitorAndNotifyOpenVPNLogins.Configuration
{
    public class Settings
    {
        public Application Application { get; set; }
        public PushOver PushOver { get; set; }
        public Logging Logging { get; set; }
    }

    public class Application
    {
        public string OpenVpnLogFile { get; set; }
        public int ReadOpenVpnLogFileEveryXSeconds { get; set; }
        public List<string> WhitelistVpnUsers { get; set; } = new List<string>();
    }

    public class Logging
    {
        public LogEventLevel MinimumLevel { get; set; }
        public File File { get; set; }
        public Console Console { get; set; }
    }

    public class File
    {
        public bool Enabled { get; set; }
        public string Path { get; set; }
        public RollingInterval RollingInterval { get; set; }
        public bool RollOnFileSizeLimit { get; set; }
        public LogEventLevel RestrictedToMinimumLevel { get; set; }
    }

    public class Console
    {
        public bool Enabled { get; set; }
        public LogEventLevel RestrictedToMinimumLevel { get; set; }
    }

    public class PushOver
    {
        public bool Enabled { get; set; }

        public string EndPoint { get; set; }
        public string ApiToken { get; set; }

        public List<string> GroupOrUserKeys { get; set; }

        [JsonConstructor]
        public PushOver(bool enabled, string endPoint, string apiToken, List<string> groupOrUserKeys)
        {
            Enabled = enabled;
            EndPoint = endPoint;
            ApiToken = apiToken;
            GroupOrUserKeys = groupOrUserKeys.Where(k => k != "").Distinct().ToList();
        }
    }

    public class Device
    {
        public string Name { get; set; }

        public bool Include { get; set; }

        public override string ToString()
        {
            return $"Device name {Name}";
        }
    }
}
