using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorAndNotifyOpenVPNLogins.Extensions
{
    public static class DoubleExtensions
    {
        public static string ToStringUsingDecimalSeperator(this double d, string newDecimalSeparator)
        {
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = newDecimalSeparator;
            return d.ToString(nfi);
        }

        public static DateTime UnixTimeStampInSecondsToDateTime(this double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
        public static DateTime UnixTimeStampInMilliSecondsToDateTime(this double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddMilliseconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
    }
}
