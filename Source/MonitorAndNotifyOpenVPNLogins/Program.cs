using MonitorAndNotifyOpenVPNLogins;
using MonitorAndNotifyOpenVPNLogins.Configuration;
using MonitorAndNotifyOpenVPNLogins.Extensions;
using MonitorAndNotifyOpenVPNLogins.Services;
using MonitorAndNotifyOpenVPNLogins.Services.GeoLocationIp;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;

internal class Program
{
    //TODO: - if logged in followed by one or more logged out/in's within ReadAfterXSeconds timeframe (happens easily on mobile), it gives multiple notifications.
    //        too prevent this merge these into 1 reconnected message 
    private static async Task Main(string[] args)
    {
        var executingDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
        
        DefaultLogging();

        Settings settings = LoadSettings("settings.json");

        InitLogging(settings);

        Log.Logger.Information($"Starting, executing directory is \"{executingDir}\"");

        //TESTING
        //System.IO.File.Delete(Path.Combine(executingDir, "previousClients.json"));
        //System.IO.File.Delete(Path.Combine(executingDir, "previousRoutingTableEntries.json"));

        var vpnLogFile = settings.Application.OpenVpnLogFile;
        PushOverService pushOverService = new PushOverService(settings.PushOver.EndPoint, settings.PushOver.ApiToken);
        GeoLocationIpService geoLocationIpService = new GeoLocationIpService("http://ip-api.com/");
        
        Dictionary<string, GeoLocationIpResponse> geoLocationIpResponses = DeserializeJsonFile<Dictionary<string, GeoLocationIpResponse>>("geoLocationIpResponses.json");

        CancellationTokenSource tokenSource = new CancellationTokenSource();
        HookupAppEvents();
        
        await Task.Run(() => LongRunningMethod(tokenSource.Token));

        async Task LongRunningMethod(CancellationToken token)
        {
            try
            {
                while (true)
                {
                    ProcessLogFile(vpnLogFile);
                    await Task.Delay(TimeSpan.FromSeconds(settings.Application.ReadOpenVpnLogFileEveryXSeconds), token);
                }
            }
            catch (Exception)
            {
                Log.Logger.Information("Application canceled");
            }
        }

        Log.Information("Finished");

        async void ProcessLogFile(string currentVpnLogFile)
        {
            Log.Logger.Information($"\nProcessing openvpn log file at \"{currentVpnLogFile}\"");
            
            if (System.IO.File.Exists(currentVpnLogFile) == false) //happens when OpenVPN is not running (yet) for example just after a reboot
            {
                Log.Logger.Information($"Log file \"{currentVpnLogFile}\" does not exist. OpenVPN not (yet) running or has been stopped?! Skipping this round...");
                return;
            }

            //Get CURRENT clients and routing tables entries
            List<string> logLines = System.IO.File.ReadAllLines(currentVpnLogFile).ToList();
            List<string> logClientLines = logLines.Where(line => line.StartsWith("CLIENT_LIST")).ToList();
            List<string> logRoutingTableLines = logLines.Where(line => line.StartsWith("ROUTING_TABLE")).ToList();

            List<Client> currentClients = logClientLines.Select(line =>
            {
                string[] columns = line.Split(',');
                return new Client(columns[1], columns[2], columns[3], columns[4], columns[5], columns[7], columns[8]); //skip first column and ConnectedSince column (use unixtime column)
            }).ToList();

            List<RoutingTableEntry> currentRoutingTableEntries = logRoutingTableLines.Select(line =>
            {
                string[] columns = line.Split(',');
                return new RoutingTableEntry(columns[1], columns[2], columns[3], columns[5]); //skip first column and Last Ref column (use unixtime column)
            }).ToList();


            //get PREVIOUS clients and routing tables entries
            List<Client> previousClients = DeserializeJsonFile<List<Client>>("previousClients.json");
            List<RoutingTableEntry> previousRoutingTableEntries = DeserializeJsonFile<List<RoutingTableEntry>>("previousRoutingTableEntries.json");

            //determine NEW routing table entries to send a notification for
            List<RoutingTableEntry> newRoutingTableEntries = currentRoutingTableEntries
                .Where(crte => !previousRoutingTableEntries.Any(prte => prte.RealAddress == crte.RealAddress && prte.VirtualAddress == crte.VirtualAddress && prte.CommonName == crte.CommonName))
                .ToList();

            //determine REMOVED routing table entries to send a notification for
            List<RoutingTableEntry> removedRoutingTableEntries = previousRoutingTableEntries
                .Where(crte => !currentRoutingTableEntries.Any(prte => prte.RealAddress == crte.RealAddress && prte.VirtualAddress == crte.VirtualAddress && prte.CommonName == crte.CommonName))
                .ToList();

            Log.Debug($"previousClients:\n{previousClients.AsJson()}\n");
            Log.Debug($"currentClients:\n{currentClients.AsJson()}\n");

            Log.Debug($"previousRoutingTableEntries:\n{previousRoutingTableEntries.AsJson()}\n");
            Log.Debug($"currentRoutingTableEntries:\n{currentRoutingTableEntries.AsJson()}\n");

            Log.Debug($"newRoutingTableEntries:\n{newRoutingTableEntries.AsJson()}\n");
            Log.Debug($"removedRoutingTableEntries:\n{removedRoutingTableEntries.AsJson()}\n");

            if (settings.PushOver.Enabled)
            {
                foreach (RoutingTableEntry nrte in newRoutingTableEntries)
                {
                    Client currentClient = currentClients.Single(cc => cc.RealAddress.Equals(nrte.RealAddress, StringComparison.OrdinalIgnoreCase));
                    DateTime connectedSince = currentClient.ConnectedSince;

                    GeoLocationIpResponse geoLocationIpResponse;
                    if (!geoLocationIpResponses.TryGetValue(currentClient.Ip, out geoLocationIpResponse))
                    {
                        geoLocationIpResponse = geoLocationIpService.GetGeoLocationIpInfo(currentClient.Ip);
                        geoLocationIpResponses.Add(currentClient.Ip, geoLocationIpResponse);
                    }

                    string countryInfo = $"{geoLocationIpResponse.City}/{geoLocationIpResponse.Country})";

                    string latLongGoogleMapsUrl = $"https://maps.google.com/?ll={geoLocationIpResponse.Lat.ToStringUsingDecimalSeperator(".")},{geoLocationIpResponse.Lon.ToStringUsingDecimalSeperator(".")}";
                    string message = $"{nrte.CommonName} logged IN from {currentClient.Ip} ({countryInfo}) using {nrte.VirtualAddress} is connected since {connectedSince}\n";

                    message += $"\n{latLongGoogleMapsUrl}";

                    await pushOverService.SendNotificationsAsync("OpenVPN new login", message, settings.PushOver.GroupOrUserKeys);
                }

                foreach (RoutingTableEntry nrte in removedRoutingTableEntries)
                {
                    string message = $"{nrte.CommonName} logged OUT from {nrte.RealAddress} using {nrte.VirtualAddress}";
                    await pushOverService.SendNotificationsAsync("OpenVPN logged out", message, settings.PushOver.GroupOrUserKeys);
                }
            }

            //update files: previous clients and routing table entries
            System.IO.File.WriteAllText("previousClients.json", currentClients.AsJson());
            System.IO.File.WriteAllText("previousRoutingTableEntries.json", currentRoutingTableEntries.AsJson());

            var blockedIpJsonFile = Path.Combine(executingDir, "block-ips.json");
            List<Client> blockedIpClients = ListExtensions.InitializeFromJsonFile<Client>(blockedIpJsonFile);
            Log.Debug($"blockedIpClients:\n{blockedIpClients.AsJson()}\n");

            blockedIpClients.AddRange(currentClients);

            blockedIpClients = blockedIpClients
                //group by ip and keep the latest entry
                .GroupBy(c => c.Ip)
                .SelectMany(x => x.Where(z => z.ConnectedSince == x.Max(i => i.ConnectedSince)))
                //ignore whitelisted user or when its already in routingtable
                .Where(c => !settings.Application.WhitelistVpnUsers.Contains(c.CommonName, StringComparer.InvariantCultureIgnoreCase) &&
                            !currentRoutingTableEntries.Any(r => r.CommonName.Equals(c.CommonName, StringComparison.InvariantCultureIgnoreCase)))
                //remove duplicate rows (same Ip)
                .Distinct()
                .ToList();

            Log.Debug($"new blockedIpClients:\n{blockedIpClients.AsJson()}\n");
            System.IO.File.WriteAllText(blockedIpJsonFile, blockedIpClients.AsJson());

            var blockedIpTextFile = Path.Combine(executingDir, "block-ips.txt");
            System.IO.File.WriteAllLines(blockedIpTextFile, blockedIpClients.Select(c => c.Ip).ToList());

            SerializeToJsonFile(geoLocationIpResponses, "geoLocationIpResponses.json");
        }

        T DeserializeJsonFile<T>(string filename) where T : class, new()
        {
            if (System.IO.File.Exists(filename)) return JsonConvert.DeserializeObject<T>(System.IO.File.ReadAllText(filename));
            else return new T();
        }

        void SerializeToJsonFile(object obj, string filename)
        {
            System.IO.File.WriteAllText(filename, obj.AsJson());
        }

        Settings LoadSettings(string filename)
        {
            if (!System.IO.File.Exists(filename))
            {
                Log.Error("settings.json not found in app directory!");
                Environment.Exit(1);
            }

            string settingsJson = System.IO.File.ReadAllText(filename);

            Settings loadedSettings = null;
            try
            {
                loadedSettings = JsonConvert.DeserializeObject<Settings>(settingsJson);
            }
            catch (Exception e)
            {
                Log.Logger.Error($"{e.Message} - {e.InnerException?.Message}");
                Environment.Exit(1);
            }

            return loadedSettings;
        }

        void DefaultLogging()
        {
            //Create default minimal logger until settings are loaded
            Log.Logger = new LoggerConfiguration()
             .MinimumLevel.Verbose() //send all events to sinks
             .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Debug)
             .CreateLogger();
        }

        void InitLogging(Settings settings)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(settings.Logging.MinimumLevel)
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                    restrictedToMinimumLevel: settings.Logging.Console.RestrictedToMinimumLevel)
                .WriteTo.Logger(logconfig => logconfig
                    .WriteTo.File(
                        outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                        path: settings.Logging.File.Path,
                        rollingInterval: settings.Logging.File.RollingInterval,
                        rollOnFileSizeLimit: settings.Logging.File.RollOnFileSizeLimit,
                        restrictedToMinimumLevel: settings.Logging.File.RestrictedToMinimumLevel))
                .CreateLogger();
        }

        void CurrentDomainOnProcessExit(object? sender, EventArgs eventArgs)
        {
            Log.Warning("Exiting process");
            tokenSource.Cancel();
        }

        void DefaultOnUnloading(AssemblyLoadContext assemblyLoadContext)
        {
            Log.Warning("Unloading process");
            tokenSource.Cancel();
        }

        void ConsoleOnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            Log.Warning("Canceling process");
            e.Cancel = true; //terminate process
            tokenSource.Cancel();
        }

        void HookupAppEvents()
        {
            AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnProcessExit;
            AssemblyLoadContext.Default.Unloading += DefaultOnUnloading;
            System.Console.CancelKeyPress += ConsoleOnCancelKeyPress;
        }
    }
}