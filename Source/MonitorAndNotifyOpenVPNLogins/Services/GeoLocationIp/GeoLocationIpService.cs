using Polly;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonitorAndNotifyOpenVPNLogins.Extensions;

namespace MonitorAndNotifyOpenVPNLogins.Services.GeoLocationIp
{
    internal class GeoLocationIpService
    {
        private readonly RestClient restClient;

        public GeoLocationIpService(string apiUrl)
        {
            var options = new RestClientOptions(apiUrl)
            {
                ThrowOnAnyError = true,
                Timeout = 5000          //1 second is not enough!
            };
            restClient = new RestClient(options);
            restClient.UseNewtonsoftJson();
        }

        public GeoLocationIpResponse GetGeoLocationIpInfo(string ip)
        {
            string s = $"Requesting GetGeoLocationIpInfo";
            Log.Information(s);

            GeoLocationIpResponse geoLocationIpResponse = null;

            var retryPolicy = Policy
              .Handle<Exception>()
              .WaitAndRetry(
                5, //retries
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (e, timeSpan, retryCount, context) =>
                {
                    // Add logic to be executed before each retry, such as logging
                    Log.Logger.Error($"Retry #{retryCount} {s}: {e.Message}{e.InnerException.Message}");
                }
              );

            var policyResult = retryPolicy
                .ExecuteAndCapture(() =>
                {
                    return restClient.GetJsonAsync<GeoLocationIpResponse>($"/json/{ip}").Result;
                }
                );

            geoLocationIpResponse = policyResult.Result;
            Log.Logger.Information($"Response GetGeoLocationIpInfo: {geoLocationIpResponse.AsJson()}");

            return geoLocationIpResponse;
        }
    }
}
