using System.Collections.Generic;
using System.Reflection;
using NLog;
using NLog.Targets.NetworkJSON.ExtensionMethods;
using PV.Logging;

namespace GDNetworkJSONService.Loggers
{
    internal class RedisMonitorLogger : LoggerBase
    {
        public RedisMonitorLogger(Assembly parentApplication, string applicationLoggingId)
            : base(parentApplication, LogMessageTypes.Instrumentation.ToString(), applicationLoggingId)
        {
            EmptyAllQueuesOnLogWrite = true;
        }

        #region Properties
        public Dictionary<string, string> Stats { get; set; }
        public long MonitorIntervalMS { get; set; }

        #endregion

        #region Instrumentation Logging Methods

        private const string MonitorMessage = "Redis Monitoring Stats";

        public void LogStats()
        {
            if (Stats != null)
            {
                foreach (var statItem in Stats)
                {
                    PushInfo(statItem.Key, statItem.Value);
                }
            }
            PushInfo(nameof(MonitorIntervalMS), MonitorIntervalMS.ToString());
            LogInfo(MonitorMessage);

            
        }

        #endregion

        #region Override Methods

        protected sealed override void SetCustomProperties(LogEventInfo logEvent)
        {

            if (Stats != null)
            {
                foreach (var statItem in Stats)
                {
                    logEvent.Properties[statItem.Key.SafeLowerCaseFirst()] = statItem.Value;
                }
            }

            logEvent.Properties["monitorIntervalMS"] = MonitorIntervalMS;
        }

        #endregion
    }
}
