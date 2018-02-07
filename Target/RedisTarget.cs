using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog.Config;

namespace NLog.Targets.NetworkJSON
{
    [Target("Redis")]
    public class RedisTarget : TargetWithLayout
    {
        protected const string ListDataType = "list";
        protected const string ChannelDataType = "channel";

        /// <summary>
        /// Sets the host name or IP Address of the redis server
        /// </summary>
        [RequiredParameter]
        public string Host { get; set; }

        /// <summary>
        /// Sets the port number redis is running on
        /// </summary>
        [RequiredParameter]
        public int Port { get; set; }

        /// <summary>
        /// Sets the key to be used for either the list or the pub/sub channel in redis
        /// </summary>
        [RequiredParameter]
        public string Key { get; set; }

        /// <summary>
        /// Sets what redis data type to use, either "list" or "channel"
        /// </summary>
        [RequiredParameter]
        public string DataType { get; set; }

        /// <summary>
        /// Sets the database id to be used in redis if the log entries are sent to a list. Defaults to 0
        /// </summary>
        public int Db { get; set; }

        /// <summary>
        /// Sets the password to be used when accessing Redis with authentication required
        /// </summary>
        public string Password { get; set; }


        [ArrayParameter(typeof(ParameterInfo), "parameter")]
        public IList<ParameterInfo> Parameters { get; }

        private RedisConnectionManager _redisConnectionManager;

        private IConverter Converter { get; }
        public RedisTarget() : this(new JsonConverter())
        {
        }

        public RedisTarget(IConverter converter)
        {
            Converter = converter;
            this.Parameters = new List<ParameterInfo>();
        }

        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            _redisConnectionManager = new RedisConnectionManager(Host, Port, Db, Password);
        }

        protected override void CloseTarget()
        {
            if (_redisConnectionManager != null)
            {
                _redisConnectionManager.Dispose();
            }

            base.CloseTarget();
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


            //var message = this.Layout.Render(logEvent);
            var redisDatabase = _redisConnectionManager.GetDatabase();
            switch (DataType.ToLower())
            {
                case ListDataType:
                    redisDatabase.ListRightPush(Key, jsonObjectStr);  //left = top, right = end
                    break;
                case ChannelDataType:
                    redisDatabase.Publish(Key, jsonObjectStr);
                    break;
                default:
                    throw new Exception("no data type defined for redis");
            }
        }

        protected override void Write(Common.AsyncLogEventInfo logEvent)
        {

            foreach (var par in Parameters)
            {
                if (!logEvent.LogEvent.Properties.ContainsKey(par.Name))
                {
                    var stringValue = par.Layout.Render(logEvent.LogEvent);

                    logEvent.LogEvent.Properties.Add(par.Name, stringValue);
                }
            }

            var jsonObject = Converter.GetLogEventJson(logEvent.LogEvent);
            if (jsonObject == null) return;
            var jsonObjectStr = jsonObject.ToString(Formatting.None, null);
            // var message = this.Layout.Render(logEvent.LogEvent);
            var redisDatabase = _redisConnectionManager.GetDatabase();
            switch (DataType.ToLower())
            {
                case ListDataType:
                    redisDatabase.ListRightPushAsync(Key, jsonObjectStr);
                    break;
                case ChannelDataType:
                    redisDatabase.PublishAsync(Key, jsonObjectStr);
                    break;
                default:
                    throw new Exception("no data type defined for redis");
            }
        }

    }
}
