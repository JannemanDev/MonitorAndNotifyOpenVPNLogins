using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorAndNotifyOpenVPNLogins.Extensions
{
    public static class ListExtensions
    {
        private static List<T> LoadFromJsonFile<T>(string filename)
        {
            if (!File.Exists(filename)) return new List<T>();

            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs))
            {
                string contents = sr.ReadToEnd();

                List<T> list = JsonConvert.DeserializeObject<List<T>>(contents) ?? new List<T>();

                return list;
            }
        }

        public static List<T> InitializeFromJsonFile<T>(string filename)
        {
            return LoadFromJsonFile<T>(filename);
        }

        public static void AddFromJsonFile<T>(this List<T> list, string filename)
        {
            list.AddRange(LoadFromJsonFile<T>(filename));
        }

        public static void SaveToJsonFile<T>(this List<T> list, string filename)
        {
            string json = list.AsJson();
            File.WriteAllText(filename, json);
        }
    }
}
