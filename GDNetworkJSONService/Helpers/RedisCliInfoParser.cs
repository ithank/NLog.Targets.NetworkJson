using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GDNetworkJSONService.Loggers;
using NLog.Targets.NetworkJSON.ExtensionMethods;

namespace GDNetworkJSONService.Helpers
{
    internal class RedisCliInfoParser
    {
        public static Dictionary<string, string> Parse(string data)
        {
            var returnDict = new Dictionary<string, string>();

            var data2 = data.Replace("\r\r\n", "|");

            var dataLines = data2.Split('|');

            foreach (var dataLine in dataLines)
            {
                if (dataLine.IsNullOrEmpty() || dataLine.StartsWith("#")) { continue; }

                var lineParts = dataLine.Split(':');
                var dataProp = lineParts[0].SafeToTitleCase();
                returnDict.Add(dataProp, lineParts[1]);
            }
            return returnDict;
        }
    }
}
