using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog.Config;

namespace NLog.Targets.NetworkJSON
{
/*
    [Target("GDService")]
    public class GDServiceTarget : TargetWithLayout
    {
        #region NetworkJson Reliability Service Variables

        private Uri _guaranteedDeliveryEndpoint;
        private Uri _networkJsonEndpoint;

        #endregion

        #region Task Properties

        [Required]
        public string GuaranteedDeliveryEndpoint
        {
            get { return _guaranteedDeliveryEndpoint.ToString(); }
            set
            { _guaranteedDeliveryEndpoint = value != null ? new Uri(Environment.ExpandEnvironmentVariables(value)) : null; }
        }

        [Required]
        public string NetworkJsonEndpoint
        {
            get { return _networkJsonEndpoint.ToString(); }
            set
            { _networkJsonEndpoint = value != null ? new Uri(Environment.ExpandEnvironmentVariables(value)) : null; }
        }
//        [Required]
        public string LocalLogStorageConnectionString { get; set; }

        [ArrayParameter(typeof(ParameterInfo), "parameter")]
        public IList<ParameterInfo> Parameters { get; }
        
        #endregion

        private IConverter Converter { get; }

        public GDServiceTarget() : this(new JsonConverter())
        {
        }

        public GDServiceTarget(IConverter converter)
        {
            Converter = converter;
            this.Parameters = new List<ParameterInfo>();
        }
        
        public void WriteLogEventInfo(LogEventInfo logEvent)
        {
            Write(logEvent);
        }

        protected override void Write(LogEventInfo logEvent)
        {
            foreach (var par in Parameters)
            {
                if (!logEvent.Properties.ContainsKey(par.Name))
                {
                    var stringValue = par.Layout.Render(logEvent);

                    logEvent.Properties.Add(par.Name, stringValue);
                }
            }
            
            var jsonObject = Converter.GetLogEventJson(logEvent);
            if (jsonObject == null) return;
            var jsonObjectStr = jsonObject.ToString(Formatting.None, null);

            var task = WriteAsync(jsonObjectStr);
            task.GetAwaiter().GetResult();
        }

        /// <summary>
        /// Exposed for unit testing and load testing purposes.
        /// </summary>
        public Task WriteAsync(string logEventAsJsonString)
        {
            return Task.Run(() =>
                {
                    LogStorageDbGlobals.ConnectionString = LocalLogStorageConnectionString;
                    LogStorageTable.InsertLogRecord(NetworkJsonEndpoint, logEventAsJsonString);
                    Trace.WriteLine(logEventAsJsonString);
                }
            );
        }
    }
*/
}
